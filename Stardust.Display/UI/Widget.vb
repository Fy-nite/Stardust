Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public MustInherit Class Widget
    Public Property X As Integer
    Public Property Y As Integer
    Public Property Width As Integer
    Public Property Height As Integer
    Public Property Visible As Boolean = True
    Public Property Enabled As Boolean = True
    Public Property Padding As Thickness
    Public Property Margin As Thickness
    Public Property BackgroundColor As Color = Color.Transparent
    Public Property BorderColor As Color = Color.Transparent
    Public Property ForegroundColor As Color = Color.White
    Public Property Tag As Object

    Private _parent As Widget = Nothing
    Private _children As New List(Of Widget)
    Private _isHovered As Boolean = False

    Public Property Parent As Widget
        Get
            Return _parent
        End Get
        Set(value As Widget)
            _parent = value
        End Set
    End Property

    Public ReadOnly Property Children As List(Of Widget)
        Get
            Return _children
        End Get
    End Property

    Public ReadOnly Property Bounds As Rectangle
        Get
            Return New Rectangle(X, Y, Width, Height)
        End Get
    End Property

    Public ReadOnly Property ClientBounds As Rectangle
        Get
            Return New Rectangle(X + Padding.Left, Y + Padding.Top,
                                  Math.Max(0, Width - Padding.Horizontal),
                                  Math.Max(0, Height - Padding.Vertical))
        End Get
    End Property

    ''' Client bounds in absolute (window-client) coordinates, used for drawing.
    Public ReadOnly Property AbsoluteClientBounds As Rectangle
        Get
            Dim ab = AbsoluteBounds
            Return New Rectangle(ab.X + Padding.Left, ab.Y + Padding.Top,
                                  Math.Max(0, Width - Padding.Horizontal),
                                  Math.Max(0, Height - Padding.Vertical))
        End Get
    End Property

    ''' Absolute X of this widget's origin, in the window-client coordinate
    ''' space used for drawing. Children are positioned relative to their
    ''' parent's origin (padding is baked into the layout X), so this is the
    ''' simple sum of the X values up the tree.
    Public ReadOnly Property AbsoluteX As Integer
        Get
            Return If(_parent IsNot Nothing, _parent.AbsoluteX + X, X)
        End Get
    End Property

    Public ReadOnly Property AbsoluteY As Integer
        Get
            Return If(_parent IsNot Nothing, _parent.AbsoluteY + Y, Y)
        End Get
    End Property

    Public ReadOnly Property AbsoluteBounds As Rectangle
        Get
            Return New Rectangle(AbsoluteX, AbsoluteY, Width, Height)
        End Get
    End Property

    Public Sub AddChild(child As Widget)
        If child._parent IsNot Nothing Then
            child._parent.RemoveChild(child)
        End If
        child._parent = Me
        _children.Add(child)
    End Sub

    Public Sub RemoveChild(child As Widget)
        child._parent = Nothing
        _children.Remove(child)
    End Sub

    Public Sub ClearChildren()
        For Each c In _children
            c._parent = Nothing
        Next
        _children.Clear()
    End Sub

    Public Sub BringToFront(child As Widget)
        _children.Remove(child)
        _children.Add(child)
    End Sub

    Public Overridable Sub Update(gameTime As GameTime)
        If Not Visible Then Return
        For Each child In _children
            child.Update(gameTime)
        Next
    End Sub

    Public Overridable Sub Draw(batch As SpriteBatch, font As StardustFont)
        If Not Visible Then Return

        Dim client = AbsoluteClientBounds

        If BackgroundColor.A > 0 Then
            FillRect(batch, client, BackgroundColor)
        End If

        If BorderColor.A > 0 AndAlso client.Width > 0 AndAlso client.Height > 0 Then
            DrawBorder(batch, client, BorderColor)
        End If

        DrawContent(batch, font)

        For Each child In _children
            child.Draw(batch, font)
        Next
    End Sub

    Protected Overridable Sub DrawContent(batch As SpriteBatch, font As StardustFont)
    End Sub

    Public Overridable Function HitTest(localX As Integer, localY As Integer) As Widget
        If Not Visible OrElse Not Enabled Then Return Nothing
        If localX < 0 OrElse localY < 0 OrElse localX >= Width OrElse localY >= Height Then Return Nothing

        For i = _children.Count - 1 To 0 Step -1
            Dim childHit = _children(i).HitTest(localX - _children(i).X, localY - _children(i).Y)
            If childHit IsNot Nothing Then Return childHit
        Next

        Return Me
    End Function

    Public Overridable Sub OnMouseEnter()
        _isHovered = True
    End Sub

    Public Overridable Sub OnMouseLeave()
        _isHovered = False
    End Sub

    Public Overridable Sub OnMouseClick(localX As Integer, localY As Integer)
    End Sub

    Public Overridable Sub OnMouseDoubleClick(localX As Integer, localY As Integer)
    End Sub

    Public Overridable Sub OnMouseMove(localX As Integer, localY As Integer)
    End Sub

    Public Overridable Sub OnMouseScroll(delta As Integer)
    End Sub

    Public Overridable Sub OnKeyPress(keyInfo As ConsoleKeyInfo)
    End Sub

    Public Overridable Sub OnTextInput(text As String)
    End Sub

    Public ReadOnly Property IsHovered As Boolean
        Get
            Return _isHovered
        End Get
    End Property

    Public Shared Sub FillRect(batch As SpriteBatch, rect As Rectangle, color As Color)
        If rect.Width <= 0 OrElse rect.Height <= 0 Then Return
        batch.Draw(DisplayServer.Instance.WhitePixel, rect, color)
    End Sub

    Public Shared Sub DrawBorder(batch As SpriteBatch, rect As Rectangle, color As Color)
        If rect.Width <= 0 OrElse rect.Height <= 0 Then Return
        Dim px = DisplayServer.Instance.WhitePixel
        batch.Draw(px, New Rectangle(rect.X, rect.Y, rect.Width, 1), color)
        batch.Draw(px, New Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), color)
        batch.Draw(px, New Rectangle(rect.X, rect.Y, 1, rect.Height), color)
        batch.Draw(px, New Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), color)
    End Sub

    Public Shared Sub DrawRect(batch As SpriteBatch, rect As Rectangle, color As Color)
        If rect.Width <= 0 OrElse rect.Height <= 0 Then Return
        Dim px = DisplayServer.Instance.WhitePixel
        batch.Draw(px, New Rectangle(rect.X, rect.Y, rect.Width, 1), color)
        batch.Draw(px, New Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), color)
        batch.Draw(px, New Rectangle(rect.X, rect.Y, 1, rect.Height), color)
        batch.Draw(px, New Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), color)
        If rect.Width > 2 AndAlso rect.Height > 2 Then
            batch.Draw(px, New Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2), color)
        End If
    End Sub

    ''' Prerendered rounded fill from the shape textures, with null-guard fallback.
    Public Shared Sub FillRounded(batch As SpriteBatch, rect As Rectangle, color As Color)
        If rect.Width <= 0 OrElse rect.Height <= 0 Then Return
        Dim shapes = DisplayServer.Instance.Shapes
        If shapes IsNot Nothing Then
            shapes.DrawRoundedControl(batch, rect, color)
        Else
            batch.Draw(DisplayServer.Instance.WhitePixel, rect, color)
        End If
    End Sub

    ''' Prerendered rounded outline from the shape textures, with null-guard fallback.
    Public Shared Sub DrawRoundedOutline(batch As SpriteBatch, rect As Rectangle, color As Color)
        If rect.Width <= 0 OrElse rect.Height <= 0 Then Return
        Dim shapes = DisplayServer.Instance.Shapes
        If shapes IsNot Nothing Then
            shapes.DrawRoundedOutlineControl(batch, rect, color)
        Else
            DrawBorder(batch, rect, color)
        End If
    End Sub
End Class
