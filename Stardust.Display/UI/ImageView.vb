Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class ImageView
    Inherits Widget

    Public Property Texture As Texture2D
    Public Property TintColor As Color = Color.White
    Public Property ScaleMode As ImageScaleMode = ImageScaleMode.Fit

    Public Enum ImageScaleMode
        Stretch
        Fit
        Center
        Tile
    End Enum

    Public Sub New()
        Width = 64
        Height = 64
    End Sub

    Public Sub New(texture As Texture2D)
        Me.Texture = texture
        If texture IsNot Nothing Then
            Width = texture.Width
            Height = texture.Height
        End If
    End Sub

    Public Sub New(texture As Texture2D, width As Integer, height As Integer)
        Me.Texture = texture
        Me.Width = width
        Me.Height = height
    End Sub

    Protected Overrides Sub DrawContent(batch As SpriteBatch, font As StardustFont)
        If Texture Is Nothing OrElse Texture.IsDisposed Then Return

        Dim client = AbsoluteClientBounds
        Dim destRect As Rectangle

        Select Case ScaleMode
            Case ImageScaleMode.Stretch
                destRect = client

            Case ImageScaleMode.Fit
                Dim texAspect = Texture.Width / CSng(Texture.Height)
                Dim boxAspect = client.Width / CSng(Math.Max(1, client.Height))
                Dim drawW As Integer, drawH As Integer
                If texAspect > boxAspect Then
                    drawW = client.Width
                    drawH = CInt(client.Width / texAspect)
                Else
                    drawH = client.Height
                    drawW = CInt(client.Height * texAspect)
                End If
                destRect = New Rectangle(
                    client.X + (client.Width - drawW) \ 2,
                    client.Y + (client.Height - drawH) \ 2,
                    drawW, drawH)

            Case ImageScaleMode.Center
                destRect = New Rectangle(
                    client.X + (client.Width - Texture.Width) \ 2,
                    client.Y + (client.Height - Texture.Height) \ 2,
                    Texture.Width, Texture.Height)

            Case Else
                destRect = client
        End Select

        batch.Draw(Texture, destRect, TintColor)
    End Sub
End Class
