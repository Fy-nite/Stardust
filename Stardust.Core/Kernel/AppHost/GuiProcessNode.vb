Imports javax.swing

Public MustInherit Class GuiProcessNode
    Inherits ProcessNode

    Protected ReadOnly Property Window As JInternalFrame

    Protected Sub New(Optional title As String = "Stardust App", Optional width As Integer = 800, Optional height As Integer = 600)
        Window = New JInternalFrame(title, True, True, True, True)
        Window.setDefaultCloseOperation(WindowConstants.DISPOSE_ON_CLOSE)
        Window.setSize(width, height)
        Window.setLocation(30 + (DisplayServer.Instance.FrameCount * 20), 30 + (DisplayServer.Instance.FrameCount * 20))
    End Sub

    Public MustOverride Sub BuildUI()

    Public Overrides Sub run()
        SwingUtilities.invokeAndWait(New SwingRunner(Sub()
                                                         BuildUI()
                                                         ' Add directly to the desktop — no double-deferred invokeLater.
                                                         DisplayServer.Instance.AddFrameDirect(Window)
                                                     End Sub))
        While Window.isShowing()
            Threading.Thread.Sleep(100)
        End While
        ExitProcess()
    End Sub

    Public Overrides Sub ExitProcess()
        SwingUtilities.invokeLater(New SwingRunner(Sub() Window.dispose()))
        MyBase.ExitProcess()
    End Sub
End Class
