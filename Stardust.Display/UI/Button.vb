Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class Button
    Inherits Widget

    Public Property Text As String = ""
    Public Property TextColor As Color = Color.White
    Public Property ButtonColor As Color = New Color(66, 135, 245)
    Public Property HoverColor As Color = New Color(86, 155, 255)
    Public Property PressedColor As Color = New Color(46, 115, 225)
    Public Property DisabledColor As Color = New Color(80, 80, 100)
    Public Property Icon As IconRenderer.IconType = IconRenderer.IconType.None
    Public Property IconSize As Integer = 12

    Private _isPressed As Boolean = False

    Public Event OnClick As EventHandler

    Public Sub New()
        Width = 80
        Height = 26
    End Sub

    Public Sub New(text As String)
        Me.Text = text
        Width = Math.Max(80, text.Length * 6 + 24)
        Height = 26
    End Sub

    Public Sub New(text As String, onClick As EventHandler)
        MyBase.New()
        Me.Text = text
        Width = Math.Max(80, text.Length * 6 + 24)
        Height = 26
        AddHandler OnClick, onClick
    End Sub

    Public Overrides Sub Update(gameTime As GameTime)
        MyBase.Update(gameTime)
    End Sub

    Protected Overrides Sub DrawContent(batch As SpriteBatch, font As StardustFont)
        Dim client = ClientBounds
        Dim bgColor = If(Not Enabled, DisabledColor,
                      If(_isPressed, PressedColor,
                         If(IsHovered, HoverColor, ButtonColor)))

        FillRect(batch, client, bgColor)
        DrawBorder(batch, client, bgColor)

        If Not String.IsNullOrEmpty(Text) Then
            Dim tx = client.X + (client.Width - Text.Length * font.CharWidth) \ 2
            Dim ty = client.Y + (client.Height - font.CharHeight) \ 2
            font.DrawString(batch, Text, New Vector2(tx, ty), TextColor)
        End If

        If Icon <> IconRenderer.IconType.None Then
            Dim ix = client.X + 6
            Dim iy = client.Y + (client.Height - IconSize) \ 2
            IconRenderer.Draw(batch, Icon, ix, iy, IconSize, TextColor)
        End If
    End Sub

    Public Overrides Sub OnMouseClick(localX As Integer, localY As Integer)
        If Not Enabled Then Return
        RaiseEvent OnClick(Me, EventArgs.Empty)
    End Sub

    Public Overrides Sub OnMouseEnter()
        MyBase.OnMouseEnter()
    End Sub

    Public Overrides Sub OnMouseLeave()
        MyBase.OnMouseLeave()
        _isPressed = False
    End Sub
End Class
