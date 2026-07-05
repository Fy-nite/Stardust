Imports javax.swing
Imports javax.swing.plaf

Public Class PicoTronTitlePane
    Inherits JComponent

    Private ReadOnly _frame As JInternalFrame

    Public Sub New(frame As JInternalFrame)
        _frame = frame
    End Sub

    Protected Overrides Sub paintComponent(g As java.awt.Graphics)
        Dim g2d = TryCast(g, java.awt.Graphics2D)
        If g2d IsNot Nothing Then
            g2d.setRenderingHint(java.awt.RenderingHints.KEY_INTERPOLATION, java.awt.RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR)
        End If

        Dim w = getWidth(), h = getHeight()
        Dim bg = PicoTronResources.LoadIcon("title_bar")
        If bg IsNot Nothing Then
            PicoTronResources.SliceNine(g2d, bg, 0, 0, w, h)
        Else
            g2d.setColor(New ColorUIResource(48, 48, 48))
            g2d.fillRect(0, 0, w, h)
        End If

        g2d.setColor(New ColorUIResource(255, 255, 255))
        g2d.setFont(New FontUIResource("SansSerif", 1, 11))
        Dim title = _frame.getTitle()
        If title IsNot Nothing AndAlso title.Length > 0 Then
            Dim fm = g2d.getFontMetrics()
            Dim tx = 4, ty = (h + fm.getAscent() - fm.getDescent()) \ 2
            g2d.drawString(title, tx, ty)
        End If
    End Sub
End Class
