Imports java.awt
Imports java.beans
Imports javax.swing
Imports javax.swing.plaf
Imports javax.swing.plaf.basic

Public Class PicoTronInternalFrameUI
    Inherits BasicInternalFrameUI

    Public Sub New(frame As JInternalFrame)
        MyBase.New(frame)
    End Sub

    Public Shared Overloads Function createUI(c As JComponent) As ComponentUI
        Return New PicoTronInternalFrameUI(TryCast(c, JInternalFrame))
    End Function

    Public Overrides Sub installUI(c As JComponent)
        MyBase.installUI(c)
        ReplaceTitlePane()
    End Sub

    Private Sub ReplaceTitlePane()
        Dim oldPane = getNorthPane()
        If oldPane Is Nothing Then Return

        Dim f = TryCast(frame, JInternalFrame)
        If f Is Nothing Then Return

        Dim titlePane As New PicoTronTitlePane(f)
        setNorthPane(titlePane)

        Dim parent = oldPane.getParent()
        If parent IsNot Nothing Then
            parent.remove(oldPane)
            parent.add(titlePane, java.awt.BorderLayout.NORTH)
            parent.revalidate()
            parent.repaint()
        End If
    End Sub

    Public Overrides Sub paint(g As Graphics, c As JComponent)
        Dim g2d = TryCast(g, Graphics2D)
        If g2d IsNot Nothing Then
            g2d.setRenderingHint(RenderingHints.KEY_INTERPOLATION, RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR)
        End If
        MyBase.paint(g, c)
    End Sub
End Class
