Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class TextBox
    Inherits Widget

    Public Property Text As String = ""
    Public Property Placeholder As String = ""
    Public Property TextColor As Color = Color.White
    Public Property PlaceholderColor As Color = New Color(120, 120, 140)
    Public Property BackgroundColor As Color = New Color(30, 30, 46)
    Public Property BorderColor As Color = New Color(60, 60, 80)
    Public Property FocusedBorderColor As Color = New Color(66, 135, 245)
    Public Property CursorColor As Color = Color.White
    Public Property MaxLength As Integer = 0
    Public Property IsReadOnly As Boolean = False

    Private _cursorPos As Integer = 0
    Private _scrollOffset As Integer = 0
    Private _blinkTimer As Double = 0
    Private _cursorVisible As Boolean = True
    Private _isFocused As Boolean = False

    Public Event OnTextChanged As EventHandler
    Public Event OnSubmit As EventHandler

    Public Sub New()
        Width = 200
        Height = 24
        BackgroundColor = New Color(30, 30, 46)
    End Sub

    Public Sub New(width As Integer)
        Me.Width = width
        Height = 24
        BackgroundColor = New Color(30, 30, 46)
    End Sub

    Public Overrides Sub Update(gameTime As GameTime)
        MyBase.Update(gameTime)
        If _isFocused Then
            _blinkTimer += gameTime.ElapsedGameTime.TotalSeconds
            If _blinkTimer >= 0.5 Then
                _cursorVisible = Not _cursorVisible
                _blinkTimer = 0
            End If
        Else
            _cursorVisible = False
            _blinkTimer = 0
        End If
    End Sub

    Protected Overrides Sub DrawContent(batch As SpriteBatch, font As StardustFont)
        Dim client = ClientBounds
        Dim borderColor = If(_isFocused, FocusedBorderColor, Me.BorderColor)

        FillRect(batch, client, BackgroundColor)
        DrawBorder(batch, client, borderColor)

        Dim textArea = New Rectangle(client.X + 4, client.Y, client.Width - 8, client.Height)
        Dim visibleChars = textArea.Width \ font.CharWidth

        Dim displayText = Text
        Dim cursorScreenX As Integer

        If String.IsNullOrEmpty(Text) AndAlso Not _isFocused Then
            font.DrawStringClipped(batch, Placeholder, New Vector2(textArea.X, textArea.Y + (client.Height - font.CharHeight) \ 2), PlaceholderColor, textArea)
            cursorScreenX = textArea.X
        Else
            Dim visibleText = If(displayText.Length > _scrollOffset,
                                 displayText.Substring(_scrollOffset, Math.Min(visibleChars, displayText.Length - _scrollOffset)),
                                 "")
            font.DrawStringClipped(batch, visibleText, New Vector2(textArea.X, textArea.Y + (client.Height - font.CharHeight) \ 2), TextColor, textArea)
            cursorScreenX = textArea.X + (_cursorPos - _scrollOffset) * font.CharWidth
        End If

        If _isFocused AndAlso _cursorVisible AndAlso Not IsReadOnly Then
            Widget.FillRect(batch, New Rectangle(cursorScreenX, client.Y + 3, 1, client.Height - 6), CursorColor)
        End If
    End Sub

    Public Overrides Sub OnMouseClick(localX As Integer, localY As Integer)
        _isFocused = True
        _cursorPos = Text.Length
        _scrollOffset = Math.Max(0, Text.Length - (ClientBounds.Width - 8) \ 6)
        _blinkTimer = 0
        _cursorVisible = True
    End Sub

    Public Overrides Sub OnKeyPress(keyInfo As ConsoleKeyInfo)
        If Not _isFocused OrElse IsReadOnly Then Return

        Select Case keyInfo.Key
            Case ConsoleKey.Backspace
                If _cursorPos > 0 Then
                    Text = Text.Remove(_cursorPos - 1, 1)
                    _cursorPos -= 1
                    RaiseEvent OnTextChanged(Me, EventArgs.Empty)
                End If
            Case ConsoleKey.Delete
                If _cursorPos < Text.Length Then
                    Text = Text.Remove(_cursorPos, 1)
                    RaiseEvent OnTextChanged(Me, EventArgs.Empty)
                End If
            Case ConsoleKey.LeftArrow
                If _cursorPos > 0 Then _cursorPos -= 1
            Case ConsoleKey.RightArrow
                If _cursorPos < Text.Length Then _cursorPos += 1
            Case ConsoleKey.Home
                _cursorPos = 0
            Case ConsoleKey.End
                _cursorPos = Text.Length
            Case ConsoleKey.Enter
                RaiseEvent OnSubmit(Me, EventArgs.Empty)
            Case Else
                If Not char.IsControl(keyInfo.KeyChar) Then
                    If MaxLength <= 0 OrElse Text.Length < MaxLength Then
                        Text = Text.Insert(_cursorPos, keyInfo.KeyChar.ToString())
                        _cursorPos += 1
                        RaiseEvent OnTextChanged(Me, EventArgs.Empty)
                    End If
                End If
        End Select

        AdjustScroll()
        _blinkTimer = 0
        _cursorVisible = True
    End Sub

    Private Sub AdjustScroll()
        Dim visibleChars = (ClientBounds.Width - 8) \ 6
        If _cursorPos < _scrollOffset Then
            _scrollOffset = _cursorPos
        ElseIf _cursorPos >= _scrollOffset + visibleChars Then
            _scrollOffset = _cursorPos - visibleChars + 1
        End If
    End Sub

    Public Sub Focus()
        _isFocused = True
        _cursorVisible = True
        _blinkTimer = 0
    End Sub

    Public Sub Unfocus()
        _isFocused = False
    End Sub
End Class
