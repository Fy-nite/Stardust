Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class Button
    Inherits Widget

    Public Property Text As String = ""
    Public Property TextColor As Color = Theme.Ink
    Public Property Icon As IconRenderer.IconType = IconRenderer.IconType.None
    Public Property IconSize As Integer = 12

    ' Per-widget overrides. Transparent = fall back to the theme palette.
    Public Property ButtonColor As Color = Color.Transparent
    Public Property HoverColor As Color = Color.Transparent
    Public Property PressedColor As Color = Color.Transparent
    Public Property DisabledColor As Color = Color.Transparent

    Private _isPressed As Boolean = False
    Private _lerpProgress As Single = 0F
    Private Const LerpSpeed As Single = 8F

    Public Event OnClick As EventHandler

    Public Sub New()
        Width = 80
        Height = 28
    End Sub

    Public Sub New(text As String)
        Me.Text = text
        Width = Math.Max(80, text.Length * 6 + 28)
        Height = 28
    End Sub

    Public Sub New(text As String, onClick As EventHandler)
        MyBase.New()
        Me.Text = text
        Width = Math.Max(80, text.Length * 6 + 28)
        Height = 28
        AddHandler OnClick, onClick
    End Sub

    Public Overrides Sub Update(gameTime As GameTime)
        Dim target = If(IsHovered, 1F, 0F)
        Dim dt = CSng(gameTime.ElapsedGameTime.TotalSeconds)
        _lerpProgress += (target - _lerpProgress) * Math.Min(1F, LerpSpeed * dt)
        MyBase.Update(gameTime)
    End Sub

    Protected Overrides Sub DrawContent(batch As SpriteBatch, font As StardustFont)
        Dim client = AbsoluteClientBounds

        Dim bgColor As Color
        If Not Enabled Then
            bgColor = If(Not DisabledColor.Equals(Color.Transparent), DisabledColor, Theme.SurfaceDisabled)
        ElseIf _isPressed Then
            bgColor = If(Not PressedColor.Equals(Color.Transparent), PressedColor, Theme.SurfacePressed)
        ElseIf _lerpProgress > 0.05F Then
            Dim base = If(Not ButtonColor.Equals(Color.Transparent), ButtonColor, Theme.SurfaceRaised)
            Dim hover = If(Not HoverColor.Equals(Color.Transparent), HoverColor, Theme.SurfaceHover)
            bgColor = Theme.Lerp(base, hover, _lerpProgress)
        Else
            bgColor = If(Not ButtonColor.Equals(Color.Transparent), ButtonColor, Theme.SurfaceRaised)
        End If

        FillRounded(batch, client, bgColor)
        DrawRoundedOutline(batch, client, Theme.Hairline)

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