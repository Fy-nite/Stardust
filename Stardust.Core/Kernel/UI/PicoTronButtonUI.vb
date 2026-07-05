Imports java.awt
Imports javax.swing
Imports javax.swing.plaf
Imports javax.swing.plaf.basic

Public Class PicoTronButtonUI
    Inherits BasicButtonUI

    Public Shared Overloads Function createUI(c As JComponent) As ComponentUI
        Return New PicoTronButtonUI()
    End Function

    Public Overrides Sub paint(g As Graphics, c As JComponent)
        Dim b = TryCast(c, AbstractButton)
        If b Is Nothing Then
            MyBase.paint(g, c)
            Return
        End If

        Dim g2d = TryCast(g, Graphics2D)
        If g2d IsNot Nothing Then
            g2d.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON)
            g2d.setRenderingHint(RenderingHints.KEY_INTERPOLATION, RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR)
        End If

        ' TODO: Draw button background from image slice
        ' e.g. draw nine-patch from theme image cache:
        '   PicoTronImages.DrawButton(g2d, b, b.getModel().isPressed(), b.getModel().isRollover())

        MyBase.paint(g, c)
    End Sub
End Class
