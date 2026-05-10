Imports System.Reflection
Namespace AppHost
    Public Class NativeAppProcess
        Inherits ProcessNode

        Private ReadOnly _main As MethodInfo
        Private ReadOnly _args As String()

        Public Sub New(main As MethodInfo, Optional args As String() = Nothing)
            _main = main
            _args = If(args, Array.Empty(Of String)())
        End Sub

        Public Overrides Sub init()
            ' nothing to do, app initialises itself in run
        End Sub

        Public Overrides Sub run()
            Try
                ' handle both Main() and Main(String()) signatures
                Dim parameters = _main.GetParameters()
                If parameters.Length = 0 Then
                    _main.Invoke(Nothing, Nothing)
                Else
                    _main.Invoke(Nothing, New Object() {_args})
                End If
            Catch ex As TargetInvocationException
                ' unwrap so the process manager sees the real exception
                Console.Error.WriteLine($"[Stardust] App threw: {ex.InnerException?.Message}")
            Catch ex As Exception
                Console.Error.WriteLine($"[Stardust] RunDll error: {ex.Message}")
            End Try
        End Sub

    End Class
End Namespace