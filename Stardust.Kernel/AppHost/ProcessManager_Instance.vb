Imports Stardust.Kernel

Namespace AppHost

    Partial Public Class ProcessManager
        Private Shared _instance As ProcessManager
        Private Shared ReadOnly _instanceLock As New Object

        ''' <summary>
        ''' The process manager used across Stardust. Programs and the shell
        ''' launch and track processes through this singleton.
        ''' </summary>
        Public Shared ReadOnly Property Instance As ProcessManager
            Get
                If _instance Is Nothing Then
                    SyncLock _instanceLock
                        If _instance Is Nothing Then
                            _instance = New ProcessManager()
                        End If
                    End SyncLock
                End If
                Return _instance
            End Get
        End Property
    End Class

End Namespace
