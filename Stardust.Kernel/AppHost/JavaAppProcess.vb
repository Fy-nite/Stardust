Imports System.IO
Imports Stardust.Kernel

Namespace AppHost

    ''' <summary>
    ''' Wraps a Java app's entry point (main() from a JAR) as a Stardust process.
    ''' Holds the class loader and entry method so run() can invoke it and the
    ''' process lifecycle stays the same as any other ProcessNode.
    ''' </summary>
    Public Class JavaAppProcess
        Inherits ProcessNode

        Private ReadOnly _entry As java.lang.reflect.Method
        Private ReadOnly _mainClass As java.lang.Class
        Private ReadOnly _loader As java.net.URLClassLoader
        Private ReadOnly _args As String()

        Friend Sub New(mainClass As java.lang.Class, entry As java.lang.reflect.Method,
                        loader As java.net.URLClassLoader, Optional appArgs As String() = Nothing)
            _mainClass = mainClass
            _entry = entry
            _loader = loader
            _args = If(appArgs, System.Array.Empty(Of String)())
        End Sub

        Public Overrides Sub init()
            ' nothing to do, the java app initialises itself in run
        End Sub

        Public Overrides Sub run()
            Try
                ' Build an argument array of Java Strings the program expects.
                Dim stringClass = java.lang.Class.forName("java.lang.String")
                Dim argv = java.lang.reflect.Array.newInstance(stringClass, _args.Length)
                For i = 0 To _args.Length - 1
                    java.lang.reflect.Array.set(argv, i, _args(i))
                Next

                _entry.invoke(Nothing, New System.Object() {argv})
            Catch ex As java.lang.reflect.InvocationTargetException
                Console.Error.WriteLine($"[Stardust] Java app '{_mainClass.getName()}' threw: {ex.Message}")
            Catch ex As Exception
                Console.Error.WriteLine($"[Stardust] Java app error: {ex.Message}")
            End Try
        End Sub

        Public Overrides Sub ExitProcess()
            ' Nothing special to tear down. The class loader for this process is the
            ' one IKVM generated per JAR, and the extract temp file is managed by
            ' the ProcessManager that launched this node.
            MyBase.ExitProcess()
        End Sub

        Protected Overrides Sub OnExiting()
            ' Release the JAR file handle so the extracted temp file can be cleaned
            ' up on Windows, where an open loader keeps the file locked.
            Try
                _loader.close()
            Catch
            End Try
            MyBase.OnExiting()
        End Sub

    End Class

End Namespace