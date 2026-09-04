Imports Microsoft.Xna.Framework
Imports Stardust.Core

Public Class Browser
    Inherits GuiProcessNode

    Public Sub New()
        MyBase.New("Browser", 720, 500)
    End Sub

    Public Overrides Sub BuildUI()
        Dim w = Window.ClientWidth
        Dim h = Window.ClientHeight
        Dim font = DisplayServer.Instance.Font
        If font Is Nothing Then Return

        Dim pixels(w * h - 1) As Color
        For i = 0 To pixels.Length - 1
            pixels(i) = New Color(24, 24, 37)
        Next

        Dim lineHeight = font.CharHeight + 4
        Dim y = h \ 2 - lineHeight

        Dim msg1 = "Browser unavailable"
        Dim msg2 = "CEF requires Java runtime"
        Dim msg3 = "which is not supported in"
        Dim msg4 = "the MonoGame display server."

        Dim cx1 = (w - msg1.Length * font.CharWidth) \ 2
        Dim cx2 = (w - msg2.Length * font.CharWidth) \ 2
        Dim cx3 = (w - msg3.Length * font.CharWidth) \ 2
        Dim cx4 = (w - msg4.Length * font.CharWidth) \ 2

        DrawText(pixels, w, h, msg1, cx1, y, New Color(180, 180, 200), font)
        y += lineHeight
        DrawText(pixels, w, h, msg2, cx2, y, New Color(120, 120, 140), font)
        y += lineHeight
        DrawText(pixels, w, h, msg3, cx3, y, New Color(120, 120, 140), font)
        y += lineHeight
        DrawText(pixels, w, h, msg4, cx4, y, New Color(120, 120, 140), font)

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
