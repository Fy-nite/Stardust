Imports System.Reflection
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input

Public Class DisplayServer
    Inherits Game

    Private Shared _instance As DisplayServer
    Private _graphics As GraphicsDeviceManager
    Private _batch As SpriteBatch
    Private _font As StardustFont
    Private _windows As New List(Of AppWindow)
    Private _lock As New Object()
    Private _focusedWindow As AppWindow = Nothing
    Private _nextZOrder As Integer = 1

    Private _desktopColor As Color = New Color(30, 30, 46)
    Private _titleBarActiveColor As Color = New Color(66, 135, 245)
    Private _titleBarInactiveColor As Color = New Color(60, 60, 80)
    Private _borderActiveColor As Color = New Color(66, 135, 245)
    Private _borderInactiveColor As Color = New Color(50, 50, 65)
    Private _clientBgColor As Color = New Color(24, 24, 37)
    Private _closeBtnColor As Color = New Color(180, 50, 50)
    Private _closeBtnHoverColor As Color = New Color(220, 60, 60)

    Private _lastMouseState As MouseState
    Private _lastKeyState As KeyboardState
    Private _dragging As Boolean = False
    Private _dragWindow As AppWindow = Nothing
    Private _dragOffsetX As Integer = 0
    Private _dragOffsetY As Integer = 0
    Private _resizing As Boolean = False
    Private _resizeWindow As AppWindow = Nothing
    Private _startupAction As Action = Nothing

    Private _whitePixel As Texture2D

    Private Sub New()
        _graphics = New GraphicsDeviceManager(Me)
        _graphics.PreferredBackBufferWidth = 1024
        _graphics.PreferredBackBufferHeight = 768
        _graphics.SynchronizeWithVerticalRetrace = True
        IsMouseVisible = True
        Window.Title = $"Stardust DisplayServer {Assembly.GetExecutingAssembly().GetName().Version}"
        Window.AllowUserResizing = True
        IsFixedTimeStep = False
        TargetElapsedTime = System.TimeSpan.FromSeconds(1.0 / 60.0)
        AddHandler Window.ClientSizeChanged, AddressOf OnClientSizeChanged
        AddHandler Exiting, AddressOf OnGameExiting
    End Sub

    Private Sub OnClientSizeChanged(sender As Object, e As EventArgs)
        Dim w = Math.Max(200, Window.ClientBounds.Width)
        Dim h = Math.Max(200, Window.ClientBounds.Height)
        If w <> _graphics.PreferredBackBufferWidth OrElse h <> _graphics.PreferredBackBufferHeight Then
            _graphics.PreferredBackBufferWidth = w
            _graphics.PreferredBackBufferHeight = h
            _graphics.ApplyChanges()
        End If
    End Sub

    Private Sub OnGameExiting(sender As Object, e As EventArgs)
        CloseAllWindows()
    End Sub

    Private Sub CloseAllWindows()
        SyncLock _lock
            For Each w In _windows.ToArray()
                w.IsOpen = False
            Next
        End SyncLock
    End Sub

    Public Shared ReadOnly Property Instance As DisplayServer
        Get
            If _instance Is Nothing Then
                _instance = New DisplayServer()
            End If
            Return _instance
        End Get
    End Property

    Public Sub RunServer(startupAction As Action)
        _startupAction = startupAction
        Run()
    End Sub

    Protected Overrides Sub Initialize()
        MyBase.Initialize()
        _startupAction?.Invoke()
    End Sub

    Protected Overrides Sub LoadContent()
        _batch = New SpriteBatch(GraphicsDevice)
        _font = New StardustFont(GraphicsDevice)
        _whitePixel = New Texture2D(GraphicsDevice, 1, 1)
        _whitePixel.SetData({Color.White})
    End Sub

    Protected Overrides Sub UnloadContent()
        _whitePixel?.Dispose()
        SyncLock _lock
            For Each w In _windows
                w.Dispose()
            Next
        End SyncLock
    End Sub

    Public ReadOnly Property Font As StardustFont
        Get
            Return _font
        End Get
    End Property

    Public ReadOnly Property WhitePixel As Texture2D
        Get
            Return _whitePixel
        End Get
    End Property

    Public ReadOnly Property Graphics As GraphicsDevice
        Get
            Return GraphicsDevice
        End Get
    End Property

    Public ReadOnly Property FrameCount As Integer
        Get
            SyncLock _lock
                Return _windows.Count
            End SyncLock
        End Get
    End Property

    Public Function CreateWindow(title As String, Optional w As Integer = 480, Optional h As Integer = 320) As AppWindow
        Dim offset As Integer = 0
        SyncLock _lock
            offset = _windows.Count * 24
        End SyncLock
        Dim win As New AppWindow(title, 40 + offset, 40 + offset, w, h)
        Return win
    End Function

    Public Sub AddWindow(win As AppWindow)
        win.ZOrder = System.Threading.Interlocked.Increment(_nextZOrder)
        SyncLock _lock
            _windows.Add(win)
        End SyncLock
        win.InitGraphics(GraphicsDevice)
        If _focusedWindow Is Nothing Then
            FocusWindow(win)
        End If
    End Sub

    Public Sub AddWindowDirect(win As AppWindow)
        AddWindow(win)
    End Sub

    Public Sub RemoveWindow(win As AppWindow)
        SyncLock _lock
            _windows.Remove(win)
        End SyncLock
        If _focusedWindow Is win Then
            _focusedWindow = Nothing
        End If
        win.IsOpen = False
        win.Dispose()
    End Sub

    Public Sub FocusWindow(win As AppWindow)
        If _focusedWindow IsNot Nothing Then
            _focusedWindow.IsFocused = False
        End If
        _focusedWindow = win
        win.IsFocused = True
        win.ZOrder = System.Threading.Interlocked.Increment(_nextZOrder)
    End Sub

    Protected Overrides Sub Update(gameTime As GameTime)
        Dim curMouse As MouseState = Mouse.GetState()
        Dim curKeybd As KeyboardState = Keyboard.GetState()

        HandleMouse(curMouse)
        HandleKeyboard(curKeybd)

        SyncLock _lock
            For Each w In _windows
                w.UpdateWidgets(gameTime)
                If w.IsVisible AndAlso w.ContentTexture IsNot Nothing Then
                    w.ApplyPendingContent()
                End If
            Next
        End SyncLock

        _lastMouseState = curMouse
        _lastKeyState = curKeybd
        MyBase.Update(gameTime)
    End Sub

    Private Sub HandleMouse(mouse As MouseState)
        Dim mx = mouse.X
        Dim my = mouse.Y
        Dim leftClicked = mouse.LeftButton = ButtonState.Pressed AndAlso _lastMouseState.LeftButton = ButtonState.Released
        Dim leftDown = mouse.LeftButton = ButtonState.Pressed
        Dim leftReleased = mouse.LeftButton = ButtonState.Released AndAlso _lastMouseState.LeftButton = ButtonState.Pressed

        If _dragging AndAlso _dragWindow IsNot Nothing Then
            If leftDown Then
                _dragWindow.X = mx - _dragOffsetX
                _dragWindow.Y = my - _dragOffsetY
            Else
                _dragging = False
                _dragWindow = Nothing
            End If
            Return
        End If

        If _resizing AndAlso _resizeWindow IsNot Nothing Then
            If leftDown Then
                Dim newW = Math.Max(200, mx - _resizeWindow.X)
                Dim newH = Math.Max(100, my - _resizeWindow.Y)
                _resizeWindow.Width = newW
                _resizeWindow.Height = newH
            Else
                _resizing = False
                _resizeWindow = Nothing
            End If
            Return
        End If

        If leftClicked Then
            Dim topWindow As AppWindow = Nothing
            SyncLock _lock
                topWindow = _windows.Where(Function(w) w.IsVisible AndAlso w.ContainsPoint(mx, my)).
                    OrderByDescending(Function(w) w.ZOrder).FirstOrDefault()
            End SyncLock

            If topWindow IsNot Nothing Then
                FocusWindow(topWindow)

                If topWindow.InCloseButton(mx, my) Then
                    RemoveWindow(topWindow)
                    Return
                End If

                If topWindow.InTitleBar(mx, my) Then
                    _dragging = True
                    _dragWindow = topWindow
                    _dragOffsetX = mx - topWindow.X
                    _dragOffsetY = my - topWindow.Y
                Else
                    Dim localX = mx - topWindow.ClientX
                    Dim localY = my - topWindow.ClientY
                    topWindow.TrackMousePosition(localX, localY)
                    topWindow.HandleWidgetClick(localX, localY)
                End If
            Else
                If _focusedWindow IsNot Nothing Then
                    _focusedWindow.IsFocused = False
                    _focusedWindow = Nothing
                End If
            End If
        End If

        If _focusedWindow IsNot Nothing Then
            Dim localX = mx - _focusedWindow.ClientX
            Dim localY = my - _focusedWindow.ClientY
            _focusedWindow.TrackMousePosition(localX, localY)
            _focusedWindow.HandleWidgetMouseMove(localX, localY)
        End If

        Dim scrollDelta = 0
        If mouse.ScrollWheelValue > _lastMouseState.ScrollWheelValue Then
            scrollDelta = 1
        ElseIf mouse.ScrollWheelValue < _lastMouseState.ScrollWheelValue Then
            scrollDelta = -1
        End If
        If scrollDelta <> 0 AndAlso _focusedWindow IsNot Nothing Then
            _focusedWindow.HandleWidgetScroll(scrollDelta)
        End If
    End Sub

    Private Sub HandleKeyboard(keyboard As KeyboardState)
        If _focusedWindow Is Nothing Then Return

        Dim shift = keyboard.IsKeyDown(Keys.LeftShift) OrElse keyboard.IsKeyDown(Keys.RightShift)
        Dim capsLock = keyboard.CapsLock
        Dim shiftChars = shift Xor capsLock

        For Each key In keyboard.GetPressedKeys()
            If Not _lastKeyState.IsKeyDown(key) Then
                Dim consoleKey = MapToConsoleKey(key)
                If consoleKey.HasValue Then
                    Dim ch As Char = ChrW(0)
                    If key >= Keys.A AndAlso key <= Keys.Z Then
                        ch = ChrW(AscW(If(shiftChars, "A"c, "a"c)) + (key - Keys.A))
                    ElseIf key >= Keys.D0 AndAlso key <= Keys.D9 Then
                        Dim digitCh = ChrW(AscW("0"c) + (key - Keys.D0))
                        ch = If(shift, ShiftedDigit(digitCh), digitCh)
                    ElseIf key = Keys.NumPad0 AndAlso key <= Keys.NumPad9 Then
                        Dim digitCh = ChrW(AscW("0"c) + (key - Keys.NumPad0))
                        ch = If(shift, ShiftedDigit(digitCh), digitCh)
                    ElseIf key = Keys.Space Then
                        ch = " "c
                    ElseIf key = Keys.Enter Then
                        ch = CChar(vbCr)
                    ElseIf key = Keys.Back Then
                        ch = ChrW(8)
                    Else
                        ch = ShiftedOemChar(key, shift)
                    End If
                    Dim keyInfo As New ConsoleKeyInfo(
                        ch,
                        consoleKey.Value,
                        shift,
                        False, capsLock)
                    _focusedWindow.HandleWidgetKey(keyInfo)
                End If
            End If
        Next
    End Sub

    Private Shared Function ShiftedOemChar(key As Keys, shift As Boolean) As Char
        Select Case key
            Case Keys.OemTilde : Return If(shift, "~"c, "`"c)
            Case Keys.OemMinus : Return If(shift, "_"c, "-"c)
            Case Keys.OemPlus : Return If(shift, "+"c, "="c)
            Case Keys.OemPeriod : Return If(shift, ">"c, "."c)
            Case Keys.OemQuestion : Return If(shift, "?"c, "/"c)
            Case Keys.OemComma : Return If(shift, "<"c, ","c)
            Case Keys.OemSemicolon : Return If(shift, ":"c, ";"c)
            Case Keys.OemQuotes : Return If(shift, """"c, "'"c)
            Case Keys.OemOpenBrackets : Return If(shift, "{"c, "["c)
            Case Keys.OemCloseBrackets : Return If(shift, "}"c, "]"c)
            Case Keys.OemPipe, Keys.OemBackslash : Return If(shift, "|"c, "\"c)
            Case Keys.Divide : Return "/"c
            Case Keys.Oem8 : Return If(shift, "~"c, "`"c)
            Case Else : Return ChrW(0)
        End Select
    End Function

    Private Shared Function ShiftedDigit(digit As Char) As Char
        Select Case digit
            Case "1"c : Return "!"c
            Case "2"c : Return "@"c
            Case "3"c : Return "#"c
            Case "4"c : Return "$"c
            Case "5"c : Return "%"c
            Case "6"c : Return "^"c
            Case "7"c : Return "&"c
            Case "8"c : Return "*"c
            Case "9"c : Return "("c
            Case "0"c : Return ")"c
            Case Else : Return digit
        End Select
    End Function

    Private Shared Function MapToConsoleKey(key As Keys) As ConsoleKey?
        If key >= Keys.A AndAlso key <= Keys.Z Then
            Return ConsoleKey.A + (key - Keys.A)
        ElseIf key >= Keys.D0 AndAlso key <= Keys.D9 Then
            Return ConsoleKey.D0 + (key - Keys.D0)
        ElseIf key >= Keys.NumPad0 AndAlso key <= Keys.NumPad9 Then
            Return ConsoleKey.NumPad0 + (key - Keys.NumPad0)
        End If

        Select Case key
            Case Keys.Enter : Return ConsoleKey.Enter
            Case Keys.Space : Return ConsoleKey.Spacebar
            Case Keys.Back : Return ConsoleKey.Backspace
            Case Keys.Delete : Return ConsoleKey.Delete
            Case Keys.Left : Return ConsoleKey.LeftArrow
            Case Keys.Right : Return ConsoleKey.RightArrow
            Case Keys.Up : Return ConsoleKey.UpArrow
            Case Keys.Down : Return ConsoleKey.DownArrow
            Case Keys.Home : Return ConsoleKey.Home
            Case Keys.End : Return ConsoleKey.End
            Case Keys.Tab : Return ConsoleKey.Tab
            Case Keys.Escape : Return ConsoleKey.Escape
            Case Keys.OemTilde : Return ConsoleKey.Oem3
            Case Keys.OemMinus : Return ConsoleKey.OemMinus
            Case Keys.OemPlus : Return ConsoleKey.OemPlus
            Case Keys.OemPeriod : Return ConsoleKey.OemPeriod
            Case Keys.OemQuestion : Return ConsoleKey.Oem2
            Case Keys.OemComma : Return ConsoleKey.OemComma
            Case Keys.OemSemicolon : Return ConsoleKey.Oem1
            Case Keys.OemQuotes : Return ConsoleKey.Oem7
            Case Keys.OemOpenBrackets : Return ConsoleKey.Oem4
            Case Keys.OemCloseBrackets : Return ConsoleKey.Oem6
            Case Keys.OemPipe, Keys.OemBackslash : Return ConsoleKey.Oem5
            Case Keys.Divide : Return ConsoleKey.Divide
            Case Keys.Oem8 : Return ConsoleKey.Oem8
            Case Else : Return Nothing
        End Select
    End Function

    Protected Overrides Sub Draw(gameTime As GameTime)
        GraphicsDevice.Clear(_desktopColor)

        ' Render each window's widget tree into its offscreen render target first.
        ' This must happen outside an active SpriteBatch because it switches render targets.
        SyncLock _lock
            For Each w In _windows.Where(Function(win) win.IsVisible)
                w.RenderWidgetTree(DisplayServer.Instance.Graphics, ClientRectFor(w))
            Next
        End SyncLock

        _batch.Begin()

        SyncLock _lock
            For Each w In _windows.Where(Function(win) win.IsVisible).OrderBy(Function(win) win.ZOrder)
                DrawWindow(w)
            Next
        End SyncLock

        DrawTaskbar()

        _batch.End()

        MyBase.Draw(gameTime)
    End Sub

    Private Shared Function ClientRectFor(win As AppWindow) As Rectangle
        Return New Rectangle(win.ClientX, win.ClientY, win.ClientWidth, win.ClientHeight)
    End Function

    Private Sub DrawWindow(win As AppWindow)
        Dim titleBarColor = If(win Is _focusedWindow, _titleBarActiveColor, _titleBarInactiveColor)
        Dim borderColor = If(win Is _focusedWindow, _borderActiveColor, _borderInactiveColor)
        Dim closeBtnColor = _closeBtnColor

        DrawRect(New Rectangle(win.X - AppWindow.BorderWidth, win.Y - AppWindow.BorderWidth,
                               win.Width + AppWindow.BorderWidth * 2,
                               win.Height + AppWindow.BorderWidth * 2 + AppWindow.TitleBarHeight), borderColor)

        DrawRect(New Rectangle(win.X, win.Y, win.Width, AppWindow.TitleBarHeight), titleBarColor)

        _font.DrawString(_batch, win.Title, New Vector2(win.X + 6, win.Y + 6), Color.White)

        Dim closeX = win.X + win.Width - 22
        Dim closeY = win.Y + 4
        DrawRect(New Rectangle(closeX, closeY, 18, 16), closeBtnColor)
        _font.DrawString(_batch, "X", New Vector2(closeX + 5, closeY + 2), Color.White)

        Dim clientRect = New Rectangle(win.ClientX, win.ClientY, win.ClientWidth, win.ClientHeight)
        DrawRect(clientRect, _clientBgColor)

        If win.WidgetTexture IsNot Nothing Then
            _batch.Draw(win.WidgetTexture, New Rectangle(clientRect.X, clientRect.Y, win.ClientWidth, win.ClientHeight), Color.White)
        ElseIf win.ContentTexture IsNot Nothing AndAlso Not win.ContentTexture.IsDisposed Then
            _batch.Draw(win.ContentTexture, New Vector2(clientRect.X, clientRect.Y), Color.White)
        End If
    End Sub

    Private Sub DrawTaskbar()
        Dim barHeight = 28
        Dim barY = GraphicsDevice.Viewport.Height - barHeight
        DrawRect(New Rectangle(0, barY, GraphicsDevice.Viewport.Width, barHeight), New Color(20, 20, 32))

        Dim x = 8
        SyncLock _lock
            For Each w In _windows
                Dim btnWidth = Math.Min(140, Math.Max(60, _font.MeasureString(w.Title).X + 16))
                Dim btnColor = If(w Is _focusedWindow, New Color(66, 135, 245), New Color(50, 50, 65))
                DrawRect(New Rectangle(x, barY + 4, CInt(btnWidth), barHeight - 8), btnColor)
                _font.DrawString(_batch, w.Title, New Vector2(x + 8, barY + 7), Color.White)
                x += CInt(btnWidth) + 4
            Next
        End SyncLock
    End Sub

    Private Sub DrawRect(rect As Rectangle, color As Color)
        _batch.Draw(_whitePixel, New Rectangle(rect.X, rect.Y, rect.Width, 1), color)
        _batch.Draw(_whitePixel, New Rectangle(rect.X, rect.Y + rect.Height - 1, rect.Width, 1), color)
        _batch.Draw(_whitePixel, New Rectangle(rect.X, rect.Y, 1, rect.Height), color)
        _batch.Draw(_whitePixel, New Rectangle(rect.X + rect.Width - 1, rect.Y, 1, rect.Height), color)

        If rect.Width > 2 AndAlso rect.Height > 2 Then
            _batch.Draw(_whitePixel, New Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2), color)
        End If
    End Sub
End Class
