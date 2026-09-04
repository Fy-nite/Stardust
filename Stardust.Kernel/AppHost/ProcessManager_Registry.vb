Imports System.Reflection
Imports Stardust.Kernel

Namespace AppHost

    Partial Public Class ProcessManager

        Private Shared ReadOnly _appRegistry As New Dictionary(Of String, Type)(StringComparer.OrdinalIgnoreCase)

        Public Sub RegisterApp(virtualPath As String, appType As Type)
            If Not GetType(ProcessNode).IsAssignableFrom(appType) Then
                Throw New ArgumentException($"Type '{appType.FullName}' must inherit ProcessNode.")
            End If
            If appType.IsAbstract Then
                Throw New ArgumentException($"Type '{appType.FullName}' cannot be abstract.")
            End If
            Dim path = NormalisePath(virtualPath)
            _appRegistry(path) = appType
        End Sub

        Public Sub RegisterApp(Of T As ProcessNode)(virtualPath As String)
            RegisterApp(virtualPath, GetType(T))
        End Sub

        Public Sub UnregisterApp(virtualPath As String)
            _appRegistry.Remove(NormalisePath(virtualPath))
        End Sub

        ''' <summary>
        ''' True if an app is registered to launch at the given virtual path.
        ''' </summary>
        Public Function IsRegistered(virtualPath As String) As Boolean
            Return _appRegistry.ContainsKey(NormalisePath(virtualPath))
        End Function

        Private Function TryStartRegistered(path As String, PPID As ULong, args As String()) As ProcessNode
            Dim appType As Type = Nothing
            If Not _appRegistry.TryGetValue(path, appType) Then Return Nothing

            Dim node = TryCast(Activator.CreateInstance(appType), ProcessNode)
            If node Is Nothing Then Return Nothing

            node.PID = NextPID()
            node.PPID = PPID
            Processes.Add(node)

            Dim t As New Threading.Thread(Sub()
                                              Try
                                                  node.init()
                                                  node.run()
                                              Catch ex As Exception
                                                  Console.Error.WriteLine($"[Stardust] App '{path}' threw: {ex.Message}")
                                              Finally
                                                  Processes.Remove(node)
                                                  node.ExitProcess()
                                              End Try
                                          End Sub)
            t.IsBackground = False
            t.Start()
            Return node
        End Function

    End Class

End Namespace
