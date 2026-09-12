Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class ScrollPanel
    Inherits Widget

    Public Property ScrollOffset As Integer = 0
    Public Property ScrollDirection As ScrollAxis = ScrollAxis.Vertical
    Public Property ShowScrollbar As Boolean = True
    Public Property ScrollbarColor As Color = New Color(80, 80, 100)
    Public Property ScrollbarThumbColor As Color = New Color(140, 140, 160)
    Public Property BackgroundColor As Color = New Color(24, 24, 37)

    Private _maxScroll As Integer = 0
    Private _isDragging As Boolean = False
    Private _dragStartPos As Integer = 0
    Private _dragStartOffset As Integer = 0

    Public Enum ScrollAxis
        Vertical
        Horizontal
    End Enum

    Public Sub New()
        Me.BackgroundColor = New Color(24, 24, 37)
    End Sub

    Public Sub New(width As Integer, height As Integer)
        Me.Width = width
        Me.Height = height
        Me.BackgroundColor = New Color(24, 24, 37)
    End Sub

    Public Overrides Sub Update(gameTime As GameTime)
        ComputeMaxScroll()
        ClampScroll()
        MyBase.Update(gameTime)
    End Sub

    Private Sub ComputeMaxScroll()
        If ScrollDirection = ScrollAxis.Vertical Then
            Dim contentBottom = 0
            For Each child In Children
                Dim childBottom = child.Y + child.Height
                If childBottom > contentBottom Then contentBottom = childBottom
            Next
            _maxScroll = Math.Max(0, contentBottom - Height + Padding.Vertical)
        Else
            Dim contentRight = 0
            For Each child In Children
                Dim childRight = child.X + child.Width
                If childRight > contentRight Then contentRight = childRight
            Next
            _maxScroll = Math.Max(0, contentRight - Width + Padding.Horizontal)
        End If
    End Sub

    Private Sub ClampScroll()
        ScrollOffset = Math.Max(0, Math.Min(ScrollOffset, _maxScroll))
    End Sub

    Public Overrides Sub Draw(batch As SpriteBatch, font As StardustFont)
        If Not Visible Then Return

        Dim client = AbsoluteClientBounds
        If BackgroundColor.A > 0 Then
            FillRect(batch, client, BackgroundColor)
        End If

        batch.End()

        Dim prevScissor = batch.GraphicsDevice.ScissorRectangle
        batch.GraphicsDevice.ScissorRectangle = client
        batch.Begin(sortMode:=SpriteSortMode.Deferred, blendState:=BlendState.AlphaBlend,
                     samplerState:=SamplerState.LinearClamp, effect:=Nothing,
                     rasterizerState:=New RasterizerState With {.ScissorTestEnable = True})

        For Each child In Children
            Dim offsetX = If(ScrollDirection = ScrollAxis.Horizontal, -ScrollOffset, 0)
            Dim offsetY = If(ScrollDirection = ScrollAxis.Vertical, -ScrollOffset, 0)
            Dim savedX = child.X
            Dim savedY = child.Y
            child.X += offsetX
            child.Y += offsetY
            child.Draw(batch, font)
            child.X = savedX
            child.Y = savedY
        Next

        batch.End()
        batch.GraphicsDevice.ScissorRectangle = prevScissor
        batch.Begin()

        If ShowScrollbar AndAlso _maxScroll > 0 Then
            DrawScrollbar(batch, client)
        End If
    End Sub

    Private Sub DrawScrollbar(batch As SpriteBatch, client As Rectangle)
        If ScrollDirection = ScrollAxis.Vertical Then
            Dim sbX = client.Right - 6
            Dim sbY = client.Y
            Dim sbW = 6
            Dim sbH = client.Height

            FillRect(batch, New Rectangle(sbX, sbY, sbW, sbH), ScrollbarColor)

            If _maxScroll > 0 Then
                Dim thumbH = Math.Max(20, CInt(sbH * (sbH / CSng(sbH + _maxScroll))))
                Dim thumbY = sbY + CInt((sbH - thumbH) * (ScrollOffset / CSng(_maxScroll)))
                FillRect(batch, New Rectangle(sbX + 1, thumbY, sbW - 2, thumbH), ScrollbarThumbColor)
            End If
        Else
            Dim sbX = client.X
            Dim sbY = client.Bottom - 6
            Dim sbW = client.Width
            Dim sbH = 6

            FillRect(batch, New Rectangle(sbX, sbY, sbW, sbH), ScrollbarColor)

            If _maxScroll > 0 Then
                Dim thumbW = Math.Max(20, CInt(sbW * (sbW / CSng(sbW + _maxScroll))))
                Dim thumbX = sbX + CInt((sbW - thumbW) * (ScrollOffset / CSng(_maxScroll)))
                FillRect(batch, New Rectangle(thumbX, sbY + 1, thumbW, sbH - 2), ScrollbarThumbColor)
            End If
        End If
    End Sub

    Public Overrides Sub OnMouseScroll(delta As Integer)
        ScrollOffset -= delta * 20
        ClampScroll()
    End Sub

    Public Overrides Function HitTest(localX As Integer, localY As Integer) As Widget
        If Not Visible OrElse Not Enabled Then Return Nothing
        If Not Bounds.Contains(localX, localY) Then Return Nothing

        Dim offsetX = If(ScrollDirection = ScrollAxis.Horizontal, -ScrollOffset, 0)
        Dim offsetY = If(ScrollDirection = ScrollAxis.Vertical, -ScrollOffset, 0)

        For i = Children.Count - 1 To 0 Step -1
            Dim child = Children(i)
            Dim childHit = child.HitTest(localX - child.X - offsetX, localY - child.Y - offsetY)
            If childHit IsNot Nothing Then Return childHit
        Next

        Return Me
    End Function

    Public Sub ScrollToTop()
        ScrollOffset = 0
    End Sub

    Public Sub ScrollToBottom()
        ScrollOffset = _maxScroll
    End Sub
End Class
