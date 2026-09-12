Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class Terminal
    Inherits Widget

    Public Property Prompt As String = "> "
    Public Property TextColor As Color = Theme.Ink
    Public Property OutputColor As Color = Theme.InkMuted
    Public Property BackgroundColor As Color = Theme.TerminalWell
    Public Property CommandColor As Color = Theme.Ink
    Public Property CursorColor As Color = Theme.Ink
    Public Property MaxScrollback As Integer = 500

    ' Per-terminal zoom: scales the glyph grid (via StardustFont.DrawStringScaled)
    ' without touching the desktop-wide UiScale. Ctrl+= / Ctrl+- adjust it; the
    ' bump factor is multiplicative so zooming feels like a terminal, not a ruler.
    Private Const ZoomMin As Single = 0.5F
    Private Const ZoomMax As Single = 3.0F
    Private Const ZoomStep As Single = 1.15F
    Private _zoom As Single = 1.0F

    Public Property Zoom As Single
        Get
            Return _zoom
        End Get
        Set(value As Single)
            _zoom = Math.Max(ZoomMin, Math.Min(ZoomMax, value))
            _scrollOffset = 0
        End Set
    End Property

    Public Event OnCommandSubmitted As EventHandler(Of CommandSubmittedEventArgs)

    Public Class CommandSubmittedEventArgs
        Inherits EventArgs
        Public Property Command As String
        Public Sub New(command As String)
            Me.Command = command
        End Sub
    End Class

    Private _outputLines As New List(Of OutputEntry)
    Private _inputBuffer As String = ""
    Private _scrollOffset As Integer = 0
    Private _blinkTimer As Double = 0
    Private _cursorVisible As Boolean = True
    Private _isFocused As Boolean = False

    Public Class OutputEntry
        Public Property Text As String
        Public Property Color As Color
        Public Sub New(text As String, color As Color)
            Me.Text = text
            Me.Color = color
        End Sub
    End Class

    Public Sub New()
        Me.BackgroundColor = Theme.TerminalWell
        Width = 300
        Height = 200
    End Sub

    Public Sub New(width As Integer, height As Integer)
        Me.Width = width
        Me.Height = height
        Me.BackgroundColor = Theme.TerminalWell
    End Sub

    Public ReadOnly Property OutputCount As Integer
        Get
            Return _outputLines.Count
        End Get
    End Property

    Public Sub Print(text As String)
        Print(text, OutputColor)
    End Sub

    Public Sub Print(text As String, color As Color)
        Dim font = DisplayServer.Instance.Font
        Dim charsPerLine = GetCharsPerLine()
        If charsPerLine <= 0 Then charsPerLine = 60

        Dim wrapped = WrapText(text, charsPerLine)
        For Each line In wrapped
            AddOutputLine(line, color)
        Next
        ScrollToBottom()
    End Sub

    Public Sub PrintCommand(commandLine As String)
        Print(Prompt & commandLine, CommandColor)
    End Sub

    Private Sub AddOutputLine(text As String, color As Color)
        _outputLines.Add(New OutputEntry(text, color))
        While _outputLines.Count > MaxScrollback
            _outputLines.RemoveAt(0)
        End While
    End Sub

    Public Sub Clear()
        _outputLines.Clear()
        _scrollOffset = 0
    End Sub

    Public Sub Focus()
        _isFocused = True
        _cursorVisible = True
        _blinkTimer = 0
    End Sub

    Public Sub Unfocus()
        _isFocused = False
        _cursorVisible = False
    End Sub

    Public ReadOnly Property IsFocused As Boolean
        Get
            Return _isFocused
        End Get
    End Property

    Private Function ScaledCharW(font As StardustFont) As Integer
        Return Math.Max(1, CInt(font.CharWidth * _zoom + 0.5F))
    End Function

    Private Function ScaledCharH(font As StardustFont) As Integer
        Return Math.Max(1, CInt(font.CharHeight * _zoom + 0.5F))
    End Function

    Private Function GetCharsPerLine() As Integer
        Dim font = DisplayServer.Instance.Font
        If font Is Nothing Then Return 60
        Return Math.Max(10, (Width - 8) \ ScaledCharW(font))
    End Function

    Private Shared Function WrapText(text As String, charsPerLine As Integer) As List(Of String)
        Dim result As New List(Of String)
        If String.IsNullOrEmpty(text) Then
            result.Add("")
            Return result
        End If

        Dim start = 0
        While start < text.Length
            Dim length = Math.Min(charsPerLine, text.Length - start)
            result.Add(text.Substring(start, length))
            start += length
        End While
        Return result
    End Function

    Public ReadOnly Property VisibleLineCount As Integer
        Get
            Dim font = DisplayServer.Instance.Font
            If font Is Nothing Then Return 10
            Return Math.Max(1, Height \ (ScaledCharH(font) + 2))
        End Get
    End Property

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
        Dim client = AbsoluteClientBounds
        FillRect(batch, client, BackgroundColor)

        Dim charW = ScaledCharW(font)
        Dim charH = ScaledCharH(font)
        Dim lineHeight = charH + 2
        Dim textLeft = client.X + 4
        Dim maxVisible = client.Height \ lineHeight

        Dim contentTotal = _outputLines.Count + 1
        Dim startIndex = Math.Max(0, contentTotal - maxVisible)
        startIndex = Math.Max(0, startIndex - _scrollOffset)

        Dim y = client.Bottom - lineHeight
        For i = 0 To maxVisible - 1
            Dim lineIndex = contentTotal - 1 - i - _scrollOffset
            If lineIndex < 0 Then Exit For

            If lineIndex < _outputLines.Count Then
                Dim entry = _outputLines(lineIndex)
                font.DrawStringScaled(batch, entry.Text, New Vector2(textLeft, y), entry.Color, _zoom)
            Else
                Dim promptLine = Prompt & _inputBuffer
                font.DrawStringScaled(batch, promptLine, New Vector2(textLeft, y), TextColor, _zoom)

                If _isFocused AndAlso _cursorVisible Then
                    Dim cursorX = textLeft + promptLine.Length * charW
                    Widget.FillRect(batch, New Rectangle(cursorX, y + 1, 1, charH), CursorColor)
                End If
            End If
            y -= lineHeight
        Next

        If _isFocused AndAlso _outputLines.Count > maxVisible Then
            DrawScrollbar(batch, client, maxVisible)
        End If
    End Sub

    Private Sub DrawScrollbar(batch As SpriteBatch, client As Rectangle, maxVisible As Integer)
        Dim trackX = client.Right - 5
        Dim trackW = 4
        Dim totalLines = _outputLines.Count + 1
        Dim overflow = totalLines - maxVisible
        If overflow <= 0 Then Return

        Dim thumbH = Math.Max(12, CInt((client.Height - 4) * (maxVisible / CSng(totalLines))))
        Dim maxOffset = Math.Max(0, overflow)
        Dim thumbY = client.Y + 2 + CInt(((client.Height - 4 - thumbH) * (_scrollOffset / CSng(maxOffset))))
        FillRounded(batch, New Rectangle(trackX, client.Y + 2, trackW, client.Height - 4), Theme.Hairline)
        FillRounded(batch, New Rectangle(trackX, thumbY, trackW, thumbH), Theme.InkFaint)
    End Sub

    Public Overrides Sub OnMouseClick(localX As Integer, localY As Integer)
        _isFocused = True
        _cursorVisible = True
        _blinkTimer = 0
    End Sub

    Public Overrides Sub OnMouseScroll(delta As Integer)
        _scrollOffset = Math.Max(0, _scrollOffset + delta)
        If _scrollOffset > 0 Then
            _isFocused = True
        Else
            _isFocused = True
        End If
    End Sub

    Public Overrides Sub OnKeyPress(keyInfo As ConsoleKeyInfo)
        If Not _isFocused Then Return

        If keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) Then
            Dim handled = True
            Select Case keyInfo.Key
                ' Ctrl+= / Ctrl+- (and numpad + / -) zoom the glyph grid; Ctrl+0 resets.
                Case ConsoleKey.OemPlus, ConsoleKey.Add
                    _zoom = Math.Min(ZoomMax, _zoom * ZoomStep)
                Case ConsoleKey.OemMinus, ConsoleKey.Subtract
                    _zoom = Math.Max(ZoomMin, _zoom / ZoomStep)
                Case ConsoleKey.D0
                    _zoom = 1.0F
                Case Else
                    handled = False
            End Select
            If handled Then
                _scrollOffset = 0
                _blinkTimer = 0
                _cursorVisible = True
                Return
            End If
        End If

        Select Case keyInfo.Key
            Case ConsoleKey.Backspace
                If _inputBuffer.Length > 0 Then
                    _inputBuffer = _inputBuffer.Substring(0, _inputBuffer.Length - 1)
                    _scrollOffset = 0
                End If
            Case ConsoleKey.Enter
                Submit()
            Case ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow
            Case Else
                If Not Char.IsControl(keyInfo.KeyChar) Then
                    _inputBuffer &= keyInfo.KeyChar
                    _scrollOffset = 0
                End If
        End Select

        _blinkTimer = 0
        _cursorVisible = True
    End Sub

    Private Sub Submit()
        Dim cmd = _inputBuffer
        _inputBuffer = ""
        _scrollOffset = 0
        RaiseEvent OnCommandSubmitted(Me, New CommandSubmittedEventArgs(cmd))
    End Sub

    Private Sub ScrollToBottom()
        _scrollOffset = 0
    End Sub
End Class
