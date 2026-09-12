Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class Label
    Inherits Widget

    Public Property Text As String = ""
    Public Property TextColor As Color = Theme.Ink
    Public Property Alignment As TextAlignment = TextAlignment.Left

    Public Enum TextAlignment
        Left
        Center
        Right
    End Enum

    Public Sub New()
        Height = 14
    End Sub

    Public Sub New(text As String)
        Me.Text = text
        Height = 14
        Width = text.Length * 6
    End Sub

    Public Sub New(text As String, textColor As Color)
        Me.Text = text
        Me.TextColor = textColor
        Height = 14
        Width = text.Length * 6
    End Sub

    Protected Overrides Sub DrawContent(batch As SpriteBatch, font As StardustFont)
        If String.IsNullOrEmpty(Text) Then Return

        Dim client = AbsoluteClientBounds
        Dim tx As Integer
        Select Case Alignment
            Case TextAlignment.Center
                tx = client.X + (client.Width - Text.Length * font.CharWidth) \ 2
            Case TextAlignment.Right
                tx = client.Right - Text.Length * font.CharWidth
            Case Else
                tx = client.X
        End Select

        Dim ty = client.Y + (client.Height - font.CharHeight) \ 2
        font.DrawString(batch, Text, New Vector2(tx, ty), TextColor)
    End Sub
End Class
