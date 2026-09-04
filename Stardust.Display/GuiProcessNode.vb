Imports Microsoft.Xna.Framework
Imports Stardust.Core
Imports Stardust.Kernel

Public MustInherit Class GuiProcessNode
    Inherits ProcessNode

    Protected ReadOnly Property Window As AppWindow

    Protected Sub New(Optional title As String = "Stardust App", Optional width As Integer = 800, Optional height As Integer = 600)
        Window = DisplayServer.Instance.CreateWindow(title, width, height)
    End Sub

    Public MustOverride Sub BuildUI()

    Public Overrides Sub run()
        BuildUI()
        DisplayServer.Instance.AddWindowDirect(Window)
        Window.WaitForClose()
        ExitProcess()
    End Sub

    Protected Overrides Sub OnExiting()
        If Window.IsOpen Then
            DisplayServer.Instance.RemoveWindow(Window)
        End If
    End Sub

    Protected Sub SetRootWidget(widget As Widget)
        Window.RootWidget = widget
    End Sub

    Protected ReadOnly Property FocusedWidget As Widget
        Get
            Return Window.FocusedWidget
        End Get
    End Property

    Protected Sub ClearClient(color As Color)
        Window.ClearContent(color)
    End Sub

    Protected Sub DrawText(text As String, x As Integer, y As Integer, Optional color As Color = Nothing)
        If color = Nothing Then color = Color.White
        Dim font = DisplayServer.Instance.Font
        If font Is Nothing Then Return

        Dim pixels(Window.ClientWidth * Window.ClientHeight - 1) As Color
        If Window.ContentTexture IsNot Nothing AndAlso Not Window.ContentTexture.IsDisposed Then
            Window.ContentTexture.GetData(pixels)
        End If

        Dim cx = x
        For Each c In text
            Dim idx = AscW(c) - 32
            If idx >= 0 AndAlso idx < 95 Then
                DrawCharToPixels(pixels, Window.ClientWidth, Window.ClientHeight, idx, cx, y, color, font)
            End If
            cx += font.CharWidth
        Next

        Window.SetPixels(pixels, Window.ClientWidth, Window.ClientHeight)
    End Sub

    Private Shared Sub DrawCharToPixels(pixels As Color(), bufferW As Integer, bufferH As Integer,
                                         charIndex As Integer, px As Integer, py As Integer,
                                         color As Color, font As StardustFont)
        Dim fontData = StardustFont.GetPublicFontData()
        If fontData Is Nothing Then Return

        For row = 0 To font.CharHeight - 1
            Dim b As Byte = fontData(charIndex * font.CharHeight + row)
            For col = 0 To font.CharWidth - 1
                If (b And CByte(1 << (font.CharWidth - 1 - col))) <> 0 Then
                    Dim dx = px + col
                    Dim dy = py + row
                    If dx >= 0 AndAlso dx < bufferW AndAlso dy >= 0 AndAlso dy < bufferH Then
                        pixels(dy * bufferW + dx) = color
                    End If
                End If
            Next
        Next
    End Sub
End Class
