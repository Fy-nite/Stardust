Imports java.awt
Imports javax.swing
Imports javax.swing.plaf
Imports javax.swing.plaf.basic

Public Class PicoTronPanelUI
    Inherits BasicPanelUI

    Public Shared Overloads Function createUI(c As JComponent) As ComponentUI
        Return New PicoTronPanelUI()
    End Function

    Public Overrides Sub paint(g As Graphics, c As JComponent)
        Dim g2d = TryCast(g, Graphics2D)
        If g2d IsNot Nothing Then
            g2d.setRenderingHint(RenderingHints.KEY_INTERPOLATION, RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR)
        End If

        ' TODO: Draw panel background from tiled image
        '   PicoTronImages.TileBackground(g2d, c, "panel_bg")

        MyBase.paint(g, c)
    End Sub
End Class
