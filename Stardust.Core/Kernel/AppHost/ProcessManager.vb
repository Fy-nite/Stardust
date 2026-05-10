Imports System.IO
Imports System.Reflection
Imports System.Runtime.Loader
Imports Stardust.Core.AppHost.Contexts

Namespace AppHost

    Partial Public Class ProcessManager
        Public Processes As List(Of ProcessNode)
        Public Function StartIRApp(ProcLocation As String, ByVal PPID As Integer) As ProcessNode
            Dim f = FS.ReadAllText(ProcLocation)
        End Function



        ''' <summary>
        ''' Loads and runs a .NET DLL as a Stardust process.
        ''' Supports apps with IApplication/ProcessNode, a standard Main method,
        ''' or falls back to a raw Main() call.
        ''' </summary>
        Public Function RunDll(dllPath As String,
                           Optional args As String() = Nothing,
                           Optional ppid As ULong = 0) As ProcessNode
            If dllPath.StartsWith("/") Then
                dllPath = dllPath.Skip(1)
            End If
            If Not IO.File.Exists(dllPath) Then
                Throw New IO.FileNotFoundException($"App DLL not found: {dllPath}")
            End If

            ' create isolated load context for this process
            Dim context = New StardustLoadContext(dllPath)
            Dim asm As Assembly

            Try
                asm = context.LoadFromAssemblyPath(dllPath)
            Catch ex As Exception
                context.Unload()
                Throw New Exception($"Failed to load assembly: {dllPath}", ex)
            End Try

            Dim node As ProcessNode = Nothing

            ' strategy 1: look for a ProcessNode subclass (native Stardust app)
            Dim processNodeType = asm.GetTypes().
            FirstOrDefault(Function(tss)
                               Return Not tss.IsAbstract AndAlso
                       GetType(ProcessNode).IsAssignableFrom(tss)
                           End Function)

            If processNodeType IsNot Nothing Then
                node = CType(Activator.CreateInstance(processNodeType), ProcessNode)
            End If

            ' strategy 2: look for a standard Main method
            If node Is Nothing Then
                Dim mainMethod = asm.GetTypes().
                SelectMany(Function(ts) ts.GetMethods(BindingFlags.Static Or BindingFlags.Public)).
                FirstOrDefault(Function(m)
                                   Return m.Name = "Main" AndAlso
                           (m.GetParameters().Length = 0 OrElse
                            (m.GetParameters().Length = 1 AndAlso
                             m.GetParameters()(0).ParameterType = GetType(String())))
                               End Function)

                If mainMethod IsNot Nothing Then
                    node = New NativeAppProcess(mainMethod, args)
                End If
            End If

            If node Is Nothing Then
                context.Unload()
                Throw New Exception($"No valid entry point found in {dllPath}. " &
                                "Implement ProcessNode or provide a static Main method.")
            End If

            ' assign process identifiers
            node.PID = NextPID()
            node.PPID = ppid

            ' store context reference on the node so we can unload on process exit
            ' we use a wrapper so the base class doesn't need to know about contexts
            Dim wrapper = New ManagedProcessWrapper(node, context)
            Processes.Add(wrapper)

            ' start on its own thread
            Dim t As New Threading.Thread(Sub()
                                              Try
                                                  node.init()
                                                  node.run()
                                              Finally
                                                  ' cleanup when the process naturally exits
                                                  Processes.Remove(wrapper)
                                                  context.Unload()
                                              End Try
                                          End Sub)

            t.IsBackground = True
            t.Start()

            Return node

        End Function

        Private _pidCounter As ULong = 1
        Private Function NormalisePath(path As String) As String
            If path.StartsWith("/") OrElse path.StartsWith("\") Then
                path = path.Substring(1)
            End If
            Return path.Replace("/", "\")
        End Function
        Private Function NextPID() As ULong
            _pidCounter += 1
            Return _pidCounter
        End Function

        Public Shared Function Init() As ProcessManager
            Throw New NotImplementedException()
        End Function

        Public Function Start(v As String, Optional PPID As ULong = 0, Optional args As String() = Nothing) As ProcessNode
            Dim path = NormalisePath(v)

            If Not FS.FileExists(path) Then
                Console.Error.WriteLine($"[Stardust] File not found: {v}")
                Return Nothing
            End If

            Dim ext = IO.Path.GetExtension(path).ToLower()

            Select Case ext
                Case ".oir", ".bir"
                    Return StartIRApp(path, PPID)

                Case ".dll"
                    Dim tempPath = FS.ExtractToTemp(path)
                    Dim node = RunDll(tempPath, args, PPID)
                    ' clean up temp file after process exits
                    AddHandler node.OnExit, Sub() IO.File.Delete(tempPath)
                    Return node
                    'Case ".sh"
                    '    Return StartShellScript(path, PPID, args)

                Case Else
                    Console.Error.WriteLine($"[Stardust] Unknown executable format: {ext}")
                    Return Nothing
            End Select
        End Function
    End Class

    ''' <summary>
    ''' Wraps a ProcessNode alongside its AssemblyLoadContext
    ''' so the context gets unloaded when the process dies.
    ''' </summary>
    Friend Class ManagedProcessWrapper
        Inherits ProcessNode

        Private ReadOnly _inner As ProcessNode
        Private ReadOnly _context As StardustLoadContext

        Public Sub New(inner As ProcessNode, context As StardustLoadContext)
            _inner = inner
            _context = context
            Me.PID = inner.PID
            Me.PPID = inner.PPID
        End Sub

        Public Overrides Sub init()
            _inner.init()
        End Sub

        Public Overrides Sub run()
            _inner.run()
        End Sub

        Public Sub Unload()
            _context.Unload()
        End Sub

    End Class

End Namespace

