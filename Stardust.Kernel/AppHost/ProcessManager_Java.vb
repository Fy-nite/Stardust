Imports System.IO
Imports System.Reflection
Imports Stardust.Kernel

Namespace AppHost

    Partial Public Class ProcessManager

        ''' <summary>
        ''' The Java facade assembly (Stardust.UiJava.dll, ikvmc output) that binds
        ''' ovh.finite.stardust.ui.* to the real Stardust.Display widgets. Loaded once
        ''' per process from next to the host; when present, its AssemblyClassLoader is
        ''' chained as the parent of every app URLClassLoader so Java UI apps resolve the
        ''' facade types instead of failing with ClassNotFoundException.
        ''' </summary>
        Private Shared _uiJavaClassLoader As java.lang.ClassLoader

        ''' <summary>
        ''' Resolves the class loader that can see the Java UI facade. Returns
        ''' Stardust.UiJava.dll's AssemblyClassLoader when the assembly is available,
        ''' otherwise Nothing (a JAR can still run, it just cannot use the UI API).
        ''' </summary>
        Private Shared Function GetUiJavaClassLoader() As java.lang.ClassLoader
            If _uiJavaClassLoader IsNot Nothing Then Return _uiJavaClassLoader
            Try
                Dim asmPath = Path.Combine(AppContext.BaseDirectory, "Stardust.UiJava.dll")
                If File.Exists(asmPath) Then
                    Dim asm = Assembly.LoadFrom(asmPath)
                    _uiJavaClassLoader = ikvm.runtime.AssemblyClassLoader.getAssemblyClassLoader(asm)
                    Console.WriteLine($"[Stardust] Java UI facade loaded from {asm.FullName}")
                End If
            Catch ex As Exception
                Console.Error.WriteLine($"[Stardust] Java UI facade unavailable: {ex.Message}")
                _uiJavaClassLoader = Nothing
            End Try
            Return _uiJavaClassLoader
        End Function

        ''' <summary>
        ''' Loads and runs a Java JAR as a Stardust process using IKVM's runtime
        ''' class loader. Java bytecode is translated to CIL on the fly, so a JAR
        ''' can be launched with no pre-build step.
        '''
        ''' The main class is taken from the JAR manifest (java -jar semantics),
        ''' or from the first argument if it names a class inside the JAR and is
        ''' followed by other arguments.
        ''' </summary>
        Public Function RunJar(jarPath As String,
                           Optional args As String() = Nothing,
                           Optional ppid As ULong = 0) As ProcessNode
            If Not FileSystem.FileExists(jarPath) Then
                Throw New IO.FileNotFoundException($"App JAR not found: {jarPath}")
            End If

            ' Extract the JAR out of the VFS into a real temp file so IKVM can
            ' build a file: URL for its class loader.
            Dim tempPath = FileSystem.ExtractToTemp(jarPath)

            Try
                Dim node = LaunchJarFromTemp(tempPath, args)
                If node Is Nothing Then
                    CleanupJar(tempPath)
                    Return Nothing
                End If

                node.PID = NextPID()
                node.PPID = ppid
                Processes.Add(node)

                Dim appName = node.GetType().Name
                Dim t As New Threading.Thread(Sub()
                                                  Try
                                                      node.init()
                                                      node.run()
                                                  Catch ex As Exception
                                                      Console.Error.WriteLine($"[Stardust] Java app '{appName}' threw: {ex.Message}")
                                                  Finally
                                                      Processes.Remove(node)
                                                      node.ExitProcess()
                                                      CleanupJar(tempPath)
                                                  End Try
                                              End Sub)
                t.IsBackground = False
                t.Start()

                Return node
            Catch ex As Exception
                CleanupJar(tempPath)
                Throw New Exception($"Failed to launch Java app: {jarPath}", ex)
            End Try
        End Function

        ' Loads the main class + entry method from an already-extracted JAR.
        ' Returns Nothing (after logging) if no usable main() could be found.
        Private Function LaunchJarFromTemp(tempPath As String, args As String()) As JavaAppProcess
            Dim url = New java.net.URL(New Uri(Path.GetFullPath(tempPath)).AbsoluteUri)
            Dim urls As java.net.URL() = {url}

            ' Parent the app loader to the Java UI facade loader (Stardust.UiJava.dll)
            ' when available, falling back to the system class loader for UI-less JARs.
            Dim parent = GetUiJavaClassLoader()
            If parent Is Nothing Then parent = java.lang.ClassLoader.getSystemClassLoader()
            Dim loader As java.net.URLClassLoader = New java.net.URLClassLoader(urls, parent)

            Dim appArgs As String() = If(args, System.Array.Empty(Of String)())

            Dim mainClassName = ResolveMainClass(loader, tempPath, appArgs)
            If String.IsNullOrEmpty(mainClassName) Then
                Console.Error.WriteLine($"[Stardust] No Main-Class found in JAR '{tempPath}'. " &
                                        "Set Main-Class in the manifest or pass the class name as the first argument.")
                Return Nothing
            End If

            Dim cls As java.lang.Class
            Try
                cls = java.lang.Class.forName(mainClassName, True, loader)
            Catch ex As Exception
                Console.Error.WriteLine($"[Stardust] Failed to load Java class '{mainClassName}': {ex.Message}")
                Return Nothing
            End Try

            Dim entry = FindMainMethod(cls)
            If entry Is Nothing Then
                Console.Error.WriteLine($"[Stardust] Class '{mainClassName}' has no public static void main(String[]).")
                Return Nothing
            End If

            Return New JavaAppProcess(cls, entry, loader, appArgs)
        End Function

' Resolves the class to launch. Priority:
        '  1) the first launch argument, if it names a class present in the JAR
        '     (removed from the app's own arguments in that case)
        '  2) the Main-Class attribute in META-INF/MANIFEST.MF
        Private Function ResolveMainClass(loader As java.net.URLClassLoader, jarFile As String, ByRef appArgs As String()) As String
            If appArgs.Length > 0 Then
                Dim candidate = appArgs(0)
                If candidate.StartsWith(".") Then candidate = candidate.Substring(1)
                candidate = candidate.Replace("/", ".").Replace("\", ".")
                Try
                    java.lang.Class.forName(candidate, False, loader)
                    appArgs = DropFirst(appArgs)
                    Return candidate
                Catch
                    ' not a class in this jar, fall through to manifest
                End Try
            End If

            ' Read the Main-Class attribute out of the JAR manifest.
            Try
                Dim jf As New java.util.jar.JarFile(jarFile)
                Try
                    Dim attrs = jf.getManifest()?.getMainAttributes()
                    If attrs IsNot Nothing Then
                        Dim mc = attrs.getValue("Main-Class")
                        If Not String.IsNullOrEmpty(mc) Then Return mc
                    End If
                Finally
                    jf.close()
                End Try
            Catch
                ' manifest unreadable, treat as no main class
            End Try

            Return Nothing
        End Function

        Private Function FindMainMethod(cls As java.lang.Class) As java.lang.reflect.Method
            Try
                Dim methods = cls.getMethods()
                For Each m In methods
                    If m.getName() = "main" AndAlso m.getReturnType().getName() = "void" Then
                        Dim ptypes = m.getParameterTypes()
                        If ptypes.Length = 1 AndAlso ptypes(0).isArray() Then
                            Return m
                        End If
                    End If
                Next
            Catch
                ' introspection failed
            End Try
            Return Nothing
        End Function

        Private Function DropFirst(items As String()) As String()
            If items.Length <= 1 Then Return System.Array.Empty(Of String)()
            Dim rest(items.Length - 2) As String
            System.Array.Copy(items, 1, rest, 0, rest.Length)
            Return rest
        End Function

        Private Sub CleanupJar(tempPath As String)
            Try
                If IO.File.Exists(tempPath) Then IO.File.Delete(tempPath)
            Catch
                ' best effort
            End Try
        End Sub

    End Class

End Namespace