Imports java.awt
Imports javax.swing

Public Class DriverViewer
    Inherits GuiProcessNode
    Public W As Container
    Public Sub New()
        MyBase.New("Driverviewer", 640, 480)
    End Sub

    Public Overrides Sub BuildUI()
        W = Window.getContentPane()
        Dim l As BoxLayout = New BoxLayout(W, BoxLayout.Y_AXIS)
        W.setLayout(l)
    End Sub

    Public Overrides Sub init()

    End Sub
End Class
