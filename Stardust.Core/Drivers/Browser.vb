Imports java.awt
Imports java.io

Imports javax.swing
Imports javax.swing.table
Imports [me].friwi.jcefmaven
Imports [me].friwi.jcefmaven.impl.progress


Public Class Browser
    Inherits GuiProcessNode


    Public Sub New()
        MyBase.New("Driver Viewer", 720, 500)
    End Sub

    Public Overrides Sub BuildUI()
        Dim contentPane = Window.getContentPane()
        contentPane.setLayout(New BorderLayout())


    End Sub

    Public Overrides Sub init()
        ' Create a New CefAppBuilder instanc

    End Sub
End Class
