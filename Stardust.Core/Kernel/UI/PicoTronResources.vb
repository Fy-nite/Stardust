Imports javax.swing

Public Module PicoTronResources
    Private _cache As New Dictionary(Of String, ImageIcon)

    Public Function LoadIcon(key As String, Optional path As String = Nothing) As ImageIcon
        If _cache.TryGetValue(key, Nothing) Then Return _cache(key)

        Dim imgPath = If(path, $"RootFS/ui/{key}.png")
        If Not IO.File.Exists(imgPath) Then Return Nothing

        Dim icon As New ImageIcon(imgPath)
        If icon.getImage() IsNot Nothing Then _cache(key) = icon
        Return icon
    End Function

    Public Function SliceNine(g As java.awt.Graphics2D, icon As ImageIcon, x As Integer, y As Integer, w As Integer, h As Integer) As Boolean
        If icon Is Nothing Then Return False
        Dim img = icon.getImage()
        Dim iw = img.getWidth(Nothing)
        Dim ih = img.getHeight(Nothing)
        If iw < 3 OrElse ih < 3 Then Return False

        Dim sX = iw \ 3, sY = ih \ 3
        Dim mX = iw - 2 * sX, mY = ih - 2 * sY

        g.drawImage(img, x, y, x + sX, y + sY, 0, 0, sX, sY, Nothing)
        g.drawImage(img, x + w - sX, y, x + w, y + sY, iw - sX, 0, iw, sY, Nothing)
        g.drawImage(img, x, y + h - sY, x + sX, y + h, 0, ih - sY, sX, ih, Nothing)
        g.drawImage(img, x + w - sX, y + h - sY, x + w, y + h, iw - sX, ih - sY, iw, ih, Nothing)

        g.drawImage(img, x + sX, y, x + w - sX, y + sY, sX, 0, sX + mX, sY, Nothing)
        g.drawImage(img, x + sX, y + h - sY, x + w - sX, y + h, sX, ih - sY, sX + mX, ih, Nothing)
        g.drawImage(img, x, y + sY, x + sX, y + h - sY, 0, sY, sX, sY + mY, Nothing)
        g.drawImage(img, x + w - sX, y + sY, x + w, y + h - sY, iw - sX, sY, iw, sY + mY, Nothing)

        g.drawImage(img, x + sX, y + sY, x + w - sX, y + h - sY, sX, sY, sX + mX, sY + mY, Nothing)
        Return True
    End Function
End Module
