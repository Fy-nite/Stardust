Imports Microsoft.Xna.Framework
Imports Stardust.Core

Public Class DriverViewer
    Inherits GuiProcessNode

    Public Sub New()
        MyBase.New("Driver Viewer", 720, 500)
    End Sub

    Public Overrides Sub BuildUI()
        RefreshDrivers()
    End Sub

    Private Sub RefreshDrivers()
        Dim w = Window.ClientWidth
        Dim h = Window.ClientHeight
        Dim font = DisplayServer.Instance.Font
        If font Is Nothing Then Return

        Dim pixels(w * h - 1) As Color
        For i = 0 To pixels.Length - 1
            pixels(i) = New Color(24, 24, 37)
        Next

        Dim y = 8
        Dim lineHeight = font.CharHeight + 4

        DrawText(pixels, w, h, " Installed Drivers", 8, y, New Color(66, 135, 245), font)
        y += lineHeight + 4

        DrawText(pixels, w, h, "─────────────────────────────────────────────", 8, y, New Color(60, 60, 80), font)
        y += lineHeight

        DrawText(pixels, w, h, "Mount Point          Driver Type", 8, y, New Color(180, 180, 200), font)
        y += lineHeight

        DrawText(pixels, w, h, "─────────────────────────────────────────────", 8, y, New Color(60, 60, 80), font)
        y += lineHeight

        Dim mounts = FS.GetMountInfo()
        For Each kv In mounts
            If y + lineHeight > h Then Exit For
            Dim line = $"{kv.Key}{kv.Value}"
            DrawText(pixels, w, h, line, 8, y, Color.White, font)
            y += lineHeight
        Next

        y += lineHeight
        Dim footer = $" {mounts.Count} driver(s) mounted"
        DrawText(pixels, w, h, footer, 8, y, New Color(120, 120, 140), font)

        Window.SetPixels(pixels, w, h)
    End Sub

    Private Shared Sub DrawText(pixels As Color(), bufferW As Integer, bufferH As Integer,
                                 text As String, px As Integer, py As Integer,
                                 color As Color, font As StardustFont)
        Dim fontData = StardustFont.GetPublicFontData()
        If fontData Is Nothing Then Return

        Dim cx = px
        For Each c In text
            Dim idx = AscW(c) - 32
            If idx >= 0 AndAlso idx < 95 Then
                For row = 0 To font.CharHeight - 1
                    Dim b As Byte = fontData(idx * font.CharHeight + row)
                    For col = 0 To font.CharWidth - 1
                        If (b And CByte(1 << (font.CharWidth - 1 - col))) <> 0 Then
                            Dim dx = cx + col
                            Dim dy = py + row
                            If dx >= 0 AndAlso dx < bufferW AndAlso dy >= 0 AndAlso dy < bufferH Then
                                pixels(dy * bufferW + dx) = color
                            End If
                        End If
                    Next
                Next
            End If
            cx += font.CharWidth
        Next
    End Sub

    Public Overrides Sub init()
    End Sub
End Class
