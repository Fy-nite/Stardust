Imports System.Reflection
Imports System.Collections.Concurrent
Imports System.IO
Imports FontStashSharp
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input

Public Class DisplayServer
    Inherits Game

    Private Shared _instance As DisplayServer
    Private _graphics As GraphicsDeviceManager
    Private _batch As SpriteBatch
    Private _font As StardustFont
    Private _fontSystem As FontSystem
    Private _uiFont As SpriteFontBase
    Private _taskFont As SpriteFontBase
    Private _windows As New List(Of AppWindow)
    Private _lock As New Object()
    Private _focusedWindow As AppWindow = Nothing
    Private _nextZOrder As Integer = 1

    Private _shapes As ShapeTextures

    Private _lastMouseState As MouseState
    Private _lastKeyState As KeyboardState
    Private _dragging As Boolean = False
    Private _dragWindow As AppWindow = Nothing
    Private _dragOffsetX As Integer = 0
    Private _dragOffsetY As Integer = 0
    Private _resizing As Boolean = False
    Private _resizeWindow As AppWindow = Nothing
    Private _resizeZone As ResizeZone = ResizeZone.None
    Private _resizeCursor As MouseCursor = Nothing
    ' The AV window that currently holds the pointer press, so the matching
    ' release (PostPointerUp) always returns to the same surface even if the
    ' pointer is over a different window / empty desktop when it comes up.
    Private _pressOwner As AppWindow = Nothing
    Private _startupAction As Action = Nothing

    Private _whitePixel As Texture2D

    ' Callbacks queued from other threads (notably the Avalonia UI thread) to be
    ' run on the game thread at the next Update. The only safe way for Avalonia
    ' controls to reach back into OS/MonoGame state.
    Private ReadOnly _gameThreadQueue As New ConcurrentQueue(Of Action)

    ''' Any thread. Run the action on the game thread at the next Update.
    Public Sub InvokeOnGameThread(action As Action)
        If action Is Nothing Then Return
        _gameThreadQueue.Enqueue(action)
    End Sub

    ' Hover tracking (set in Update, read in Draw).
    Private _hoveredCaptionWin As AppWindow = Nothing
    Private _hoveredCaption As AppWindow.CaptionHit = AppWindow.CaptionHit.None
    Private _hoveredResizeWin As AppWindow = Nothing
    Private _hoveredResizeZone As ResizeZone = ResizeZone.None
    Private _hoveredTaskIdx As Integer = -1

    ' Last cursor we told the OS about, so we only update on change.
    Private _cursorSet As MouseCursor = Nothing

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
        _shapes = New ShapeTextures(GraphicsDevice)

        ' Real font (Fira Code) for chrome text (window titles, taskbar).
        _fontSystem = New FontSystem()
        Dim fontData = LoadEmbeddedResource("FiraCode-Regular.ttf")
        If fontData IsNot Nothing Then
            _fontSystem.AddFont(fontData)
            _uiFont = _fontSystem.GetFont(Theme.TitleFontPx)
            _taskFont = _fontSystem.GetFont(Theme.TaskFontPx)
        End If

        ' Restore the persisted UI scale now that chrome textures and fonts exist.
        Dim saved = LoadUiScale()
        If saved.HasValue Then SetUiScale(saved.Value)
    End Sub

    Public Shared Function LoadEmbeddedResource(suffix As String) As Byte()
        Dim asm = GetType(DisplayServer).Assembly
        For Each name In asm.GetManifestResourceNames()
            If name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) Then
                Using s = asm.GetManifestResourceStream(name)
                    If s IsNot Nothing Then
                        Using ms = New MemoryStream()
                            s.CopyTo(ms)
                            Return ms.ToArray()
                        End Using
                    End If
                End Using
            End If
        Next
        Return Nothing
    End Function

    ''' Change the desktop UI scale live: clamps, rebuilds the title-bar chrome
    ''' textures, re-creates the chrome fonts, and persists the value. Must be
    ''' called on the game thread (widget click handlers run there), so it is
    ''' safe to allocate textures and touch state read by Draw.
    Public Sub SetUiScale(value As Single)
        Theme.SetUiScale(value)
        _shapes?.ApplyScale()
        If _fontSystem IsNot Nothing Then
            _uiFont = _fontSystem.GetFont(Theme.TitleFontPx)
            _taskFont = _fontSystem.GetFont(Theme.TaskFontPx)
        End If
        ' Windows grow/shrink with the scale so their scaled content stays fully visible.
        SyncLock _lock
            For Each w In _windows
                w.SizeToScale()
            Next
        End SyncLock
        SaveUiScale(Theme.UiScale)
    End Sub

    Private Shared Function SettingsPath() As String
        Dim dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Stardust")
        Return Path.Combine(dir, "ui.settings")
    End Function

    Private Shared Function LoadUiScale() As Single?
        Try
            Dim txt = File.ReadAllText(SettingsPath()).Trim()
            Dim s As Single
            If Single.TryParse(txt, System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, s) Then
                Return s
            End If
        Catch
        End Try
        Return Nothing
    End Function

    Private Shared Sub SaveUiScale(scale As Single)
        Try
            Dim path = SettingsPath()
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path))
            File.WriteAllText(path, scale.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
        Catch
        End Try
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

    Public ReadOnly Property Shapes As ShapeTextures
        Get
            Return _shapes
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
        Dim hadFocus As AppWindow = _focusedWindow
        If hadFocus IsNot Nothing AndAlso hadFocus IsNot win Then
            hadFocus.IsFocused = False
        End If
        _focusedWindow = win
        win.IsFocused = True
        If win.IsMinimized Then win.IsMinimized = False
        win.ZOrder = System.Threading.Interlocked.Increment(_nextZOrder)
    End Sub

    Public ReadOnly Property WorkArea As Rectangle
        Get
            Return New Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height - Theme.TaskbarHeight)
        End Get
    End Property

    ' ---------------------------------------------------------------- update

    Protected Overrides Sub Update(gameTime As GameTime)
        Dim curMouse As MouseState = Mouse.GetState()
        Dim curKeybd As KeyboardState = Keyboard.GetState()

        ' Game-thread callbacks posted from other threads (e.g. Avalonia UI).
        Dim action As Action = Nothing
        While _gameThreadQueue.TryDequeue(action)
            Try
                action()
            Catch ex As Exception
                Console.Error.WriteLine($"[DisplayServer] queued action failed: {ex.Message}")
            End Try
        End While

        HandleMouse(curMouse)
        HandleKeyboard(curKeybd)
        UpdateHoverTracking(curMouse)

        SyncLock _lock
            For Each w In _windows
                w.UpdateWidgets(gameTime)
                If w.IsVisible AndAlso Not w.IsMinimized AndAlso w.ContentTexture IsNot Nothing Then
                    w.ApplyPendingContent()
                End If
            Next
        End SyncLock

        _lastMouseState = curMouse
        _lastKeyState = curKeybd

        MyBase.Update(gameTime)
    End Sub

    ' ----------------------------------------------------------- hover tracking

    Private Sub UpdateHoverTracking(mouse As MouseState)
        Dim mx = mouse.X
        Dim my = mouse.Y
        _hoveredCaptionWin = Nothing
        _hoveredCaption = AppWindow.CaptionHit.None
        _hoveredResizeWin = Nothing
        _hoveredResizeZone = ResizeZone.None
        _hoveredTaskIdx = -1

        ' Cursor feedback: an active resize keeps its anchor cursor, otherwise the
        ' top window's caption buttons and frame zones drive it.
        If _resizing AndAlso _resizeWindow IsNot Nothing Then
            SetOSCursor(_resizeCursor)
        Else
            Dim cursor As MouseCursor = MouseCursor.Arrow
            SyncLock _lock
                Dim topWin = _windows.Where(Function(w) w.IsVisible AndAlso Not w.IsMinimized AndAlso w.ContainsPoint(mx, my)).
                                      OrderByDescending(Function(w) w.ZOrder).FirstOrDefault()
                If topWin IsNot Nothing Then
                    If topWin.InTitleBar(mx, my) Then
                        _hoveredCaptionWin = topWin
                        _hoveredCaption = topWin.HitCaptionButton(mx, my)
                        If _hoveredCaption <> AppWindow.CaptionHit.None Then
                            cursor = MouseCursor.Hand
                        End If
                    Else
                        Dim zone = HitResizeZone(topWin, mx, my)
                        If zone <> ResizeZone.None Then
                            _hoveredResizeWin = topWin
                            _hoveredResizeZone = zone
                            cursor = CursorForZone(zone)
                        End If
                    End If
                End If
            End SyncLock
            SetOSCursor(cursor)
        End If

        ' Taskbar hover.
        Dim barY = GraphicsDevice.Viewport.Height - Theme.TaskbarHeight
        If my >= barY Then
            Dim idx = HitTestTaskbar(mx, my)
            _hoveredTaskIdx = idx
        End If
    End Sub

    Private Function HitTestTaskbar(mx As Integer, my As Integer) As Integer
        Dim barY = GraphicsDevice.Viewport.Height - Theme.TaskbarHeight
        Dim leftPad = CInt(6 * Theme.UiScale)
        Dim x = leftPad
        SyncLock _lock
            Dim visibleWins = _windows.Where(Function(w) w.IsOpen).ToList()
            For i = 0 To visibleWins.Count - 1
                Dim w = visibleWins(i)
                Dim btnW = TaskbarButtonWidth(w.Title)
                Dim btnRect = New Rectangle(x, barY + 5, btnW, Theme.TaskbarHeight - 10)
                If btnRect.Contains(mx, my) Then Return i
                x += btnW + CInt(3 * Theme.UiScale)
            Next
        End SyncLock
        Return -1
    End Function

    ''' Shared between hit-testing and drawing so buttons track the mouse and
    ''' render at the same size. Uses the Fira Code taskbar font when available.
    Private Function TaskbarButtonWidth(title As String) As Integer
        Dim titleW As Single
        If _taskFont IsNot Nothing Then
            titleW = _taskFont.MeasureString(title).X
        Else
            titleW = title.Length * _font.CharWidth
        End If
        Dim minW = CInt(100 * Theme.UiScale)
        Dim maxW = CInt(190 * Theme.UiScale)
        Return Math.Max(minW, Math.Min(maxW, CInt(titleW + 38 * Theme.UiScale)))
    End Function

    ' ------------------------------------------------------------- mouse

    Private Sub HandleMouse(mouse As MouseState)
        Dim mx = mouse.X
        Dim my = mouse.Y
        Dim leftClicked = mouse.LeftButton = ButtonState.Pressed AndAlso _lastMouseState.LeftButton = ButtonState.Released
        Dim leftDown = mouse.LeftButton = ButtonState.Pressed
        Dim leftReleased = mouse.LeftButton = ButtonState.Released AndAlso _lastMouseState.LeftButton = ButtonState.Pressed
        Dim rightDown = mouse.RightButton = ButtonState.Pressed
        Dim middleDown = mouse.MiddleButton = ButtonState.Pressed

        If _dragging AndAlso _dragWindow IsNot Nothing Then
            If leftDown Then
                _dragWindow.X = mx - _dragOffsetX
                _dragWindow.Y = my - _dragOffsetY
            Else
                _dragging = False
                _dragWindow = Nothing
                _pressOwner = Nothing
            End If
            Return
        End If

        If _resizing AndAlso _resizeWindow IsNot Nothing Then
            If leftDown Then
                ApplyResize(_resizeWindow, mx, my)
                _resizeWindow.ApplyContentSize()
                SetOSCursor(_resizeCursor)
            Else
                _resizing = False
                _resizeWindow = Nothing
                _pressOwner = Nothing
            End If
            Return
        End If

        If leftClicked Then
            ' Taskbar first.
            Dim barY = GraphicsDevice.Viewport.Height - Theme.TaskbarHeight
            If my >= barY Then
                Dim idx = HitTestTaskbar(mx, my)
                If idx >= 0 Then
                    Dim visibleWins As List(Of AppWindow)
                    SyncLock _lock
                        visibleWins = _windows.Where(Function(w) w.IsOpen).ToList()
                    End SyncLock
                    If idx < visibleWins.Count Then
                        FocusWindow(visibleWins(idx))
                    End If
                End If
                Return
            End If

            Dim topWindow As AppWindow = Nothing
            SyncLock _lock
                topWindow = _windows.Where(Function(w) w.IsVisible AndAlso w.ContainsPoint(mx, my)).
                    OrderByDescending(Function(w) w.ZOrder).FirstOrDefault()
            End SyncLock

            If topWindow IsNot Nothing Then
                FocusWindow(topWindow)

                ' Corner / edge resize wins over the title bar (so the top edge
                ' of the frame is grabbable), chosen once at press time so a fast
                ' drag cannot flip the anchor mid-gesture. Maximized windows have
                ' no zone and fall through to the title bar.
                Dim zone = HitResizeZone(topWindow, mx, my)
                If zone <> ResizeZone.None Then
                    _resizing = True
                    _resizeWindow = topWindow
                    _resizeZone = zone
                    _resizeCursor = CursorForZone(zone)
                    SetOSCursor(_resizeCursor)
                    Return
                End If

                If topWindow.InTitleBar(mx, my) Then
                    Dim hit = topWindow.HitCaptionButton(mx, my)
                    Select Case hit
                        Case AppWindow.CaptionHit.CaptionClose
                            RemoveWindow(topWindow)
                            Return
                        Case AppWindow.CaptionHit.CaptionMinimize
                            topWindow.Minimize()
                            Return
                        Case AppWindow.CaptionHit.CaptionMaximize
                            topWindow.ToggleMaximize(WorkArea)
                            Return
                    End Select
                    _dragging = True
                    _dragWindow = topWindow
                    _dragOffsetX = mx - topWindow.X
                    _dragOffsetY = my - topWindow.Y
                Else
If topWindow.AvaloniaSurface IsNot Nothing Then
                    Dim s = Theme.UiScale
                    Dim px = CInt((mx - topWindow.ClientX) / s)
                    Dim py = CInt((my - topWindow.ClientY) / s)
                    _pressOwner = topWindow
                    ' A fresh MouseMove right before the down establishes the
                    ' hover/hit state; without it headless MouseDown is inert.
                    topWindow.AvaloniaSurface.PostPointer(px, py, leftDown, rightDown, middleDown)
                    topWindow.AvaloniaSurface.PostPointerDown(
                        px, py, AvaloniaInput.MapButton(
                            mouse.LeftButton = ButtonState.Pressed,
                            mouse.RightButton = ButtonState.Pressed,
                            mouse.MiddleButton = ButtonState.Pressed),
                        leftDown, rightDown, middleDown)
                Else
                    Dim s = Theme.UiScale
                    Dim localX = CInt((mx - topWindow.ClientX) / s)
                    Dim localY = CInt((my - topWindow.ClientY) / s)
                    topWindow.TrackMousePosition(localX, localY)
                    topWindow.HandleWidgetClick(localX, localY)
                End If
            End If
        Else
            If _focusedWindow IsNot Nothing Then
                _focusedWindow.IsFocused = False
                _focusedWindow = Nothing
            End If
        End If
        End If

        ' Release: the matching mouse-up for the window that owns the press. Must
        ' reach the same surface even when the pointer comes up over empty
        ' desktop or another window; without it the Avalonia button never sees a
        ' MouseUp and stays latched pressed forever (clicks stop working).
        If leftReleased AndAlso _pressOwner IsNot Nothing Then
            Dim owner = _pressOwner
            _pressOwner = Nothing
            If owner.AvaloniaSurface IsNot Nothing Then
                Dim s = Theme.UiScale
                Dim px = CInt((mx - owner.ClientX) / s)
                Dim py = CInt((my - owner.ClientY) / s)
                owner.AvaloniaSurface.PostPointer(px, py, False, False, False)
                owner.AvaloniaSurface.PostPointerUp(px, py, Avalonia.Input.MouseButton.Left, False, False, False)
            End If
        End If

        If _focusedWindow IsNot Nothing AndAlso Not _focusedWindow.IsMinimized Then
            If _focusedWindow.AvaloniaSurface IsNot Nothing Then
                Dim s = Theme.UiScale
                Dim px = CInt((mx - _focusedWindow.ClientX) / s)
                Dim py = CInt((my - _focusedWindow.ClientY) / s)
                _focusedWindow.AvaloniaSurface.PostPointer(
                    px, py,
                    mouse.LeftButton = ButtonState.Pressed,
                    mouse.RightButton = ButtonState.Pressed,
                    mouse.MiddleButton = ButtonState.Pressed)
            Else
                Dim s = Theme.UiScale
                Dim localX = CInt((mx - _focusedWindow.ClientX) / s)
                Dim localY = CInt((my - _focusedWindow.ClientY) / s)
                _focusedWindow.TrackMousePosition(localX, localY)
                _focusedWindow.HandleWidgetMouseMove(localX, localY)
            End If
        End If

        Dim scrollDelta = 0
        If mouse.ScrollWheelValue > _lastMouseState.ScrollWheelValue Then
            scrollDelta = 1
        ElseIf mouse.ScrollWheelValue < _lastMouseState.ScrollWheelValue Then
            scrollDelta = -1
        End If
        If scrollDelta <> 0 AndAlso _focusedWindow IsNot Nothing AndAlso Not _focusedWindow.IsMinimized Then
            If _focusedWindow.AvaloniaSurface IsNot Nothing Then
                Dim s = Theme.UiScale
                _focusedWindow.AvaloniaSurface.PostScroll(
                    CInt((mx - _focusedWindow.ClientX) / s), CInt((my - _focusedWindow.ClientY) / s),
                    scrollDelta * 120,
                    mouse.LeftButton = ButtonState.Pressed,
                    mouse.RightButton = ButtonState.Pressed,
                    mouse.MiddleButton = ButtonState.Pressed)
            Else
                _focusedWindow.HandleWidgetScroll(scrollDelta)
            End If
        End If
    End Sub

    Private Enum ResizeZone
    None
    North
    South
    West
    East
    NorthWest
    NorthEast
    SouthWest
    SouthEast
End Enum

''' The resize hot-zone thickness in physical pixels, scaling with the UI so the
''' grab area stays comfortable at every scale.
Private Const EdgeZone As Integer = 7
Private Function ResizeZoneSize() As Integer
    Return Math.Max(EdgeZone, CInt(EdgeZone * Theme.UiScale))
End Function

''' Which resize zone a point on a window's frame sits in, with corners winning
''' over single edges. Maximized windows never expose a zone.
Private Function HitResizeZone(win As AppWindow, mx As Integer, my As Integer) As ResizeZone
    If win Is Nothing OrElse win.IsMaximized Then Return ResizeZone.None
    Dim z = ResizeZoneSize()
    Dim onLeft = mx >= win.X - z AndAlso mx <= win.X + z
    Dim onRight = mx >= win.X + win.Width - z AndAlso mx <= win.X + win.Width + z
    Dim onTop = my >= win.Y - z AndAlso my <= win.Y + z
    Dim onBottom = my >= win.Y + win.Height - z AndAlso my <= win.Y + win.Height + z
    If onTop AndAlso onLeft Then Return ResizeZone.NorthWest
    If onTop AndAlso onRight Then Return ResizeZone.NorthEast
    If onBottom AndAlso onLeft Then Return ResizeZone.SouthWest
    If onBottom AndAlso onRight Then Return ResizeZone.SouthEast
    If onTop Then Return ResizeZone.North
    If onBottom Then Return ResizeZone.South
    If onLeft Then Return ResizeZone.West
    If onRight Then Return ResizeZone.East
    Return ResizeZone.None
End Function

Private Shared Function CursorForZone(z As ResizeZone) As MouseCursor
    Select Case z
        Case ResizeZone.North, ResizeZone.South : Return MouseCursor.SizeNS
        Case ResizeZone.West, ResizeZone.East : Return MouseCursor.SizeWE
        Case ResizeZone.NorthWest, ResizeZone.SouthEast : Return MouseCursor.SizeNWSE
        Case ResizeZone.NorthEast, ResizeZone.SouthWest : Return MouseCursor.SizeNESW
        Case Else : Return MouseCursor.Arrow
    End Select
End Function

Private Sub SetOSCursor(c As MouseCursor)
    If c Is Nothing Then c = MouseCursor.Arrow
    If c IsNot _cursorSet Then
        _cursorSet = c
        Try
            Mouse.SetCursor(c)
        Catch
        End Try
    End If
End Sub

Private Sub ApplyResize(win As AppWindow, mx As Integer, my As Integer)
    Dim minW = 220
    Dim minH = 120
    Select Case _resizeZone
        Case ResizeZone.East
            win.Width = Math.Max(minW, mx - win.X)
        Case ResizeZone.South
            win.Height = Math.Max(minH, my - win.Y)
        Case ResizeZone.West, ResizeZone.NorthWest, ResizeZone.SouthWest
            Dim newW = Math.Max(minW, win.X + win.Width - mx)
            win.X = win.X + win.Width - newW
            win.Width = newW
        Case ResizeZone.North, ResizeZone.NorthEast
            Dim newH = Math.Max(minH, win.Y + win.Height - my)
            win.Y = win.Y + win.Height - newH
            win.Height = newH
        Case ResizeZone.NorthWest
            Dim newW = Math.Max(minW, win.X + win.Width - mx)
            win.X = win.X + win.Width - newW
            win.Width = newW
            Dim newH = Math.Max(minH, win.Y + win.Height - my)
            win.Y = win.Y + win.Height - newH
            win.Height = newH
        Case ResizeZone.NorthEast
            win.Width = Math.Max(minW, mx - win.X)
            Dim newH = Math.Max(minH, win.Y + win.Height - my)
            win.Y = win.Y + win.Height - newH
            win.Height = newH
        Case ResizeZone.SouthEast
            win.Width = Math.Max(minW, mx - win.X)
            win.Height = Math.Max(minH, my - win.Y)
        Case ResizeZone.SouthWest
            Dim newW = Math.Max(minW, win.X + win.Width - mx)
            win.X = win.X + win.Width - newW
            win.Width = newW
            win.Height = Math.Max(minH, my - win.Y)
        Case Else
    End Select
End Sub

    ' ------------------------------------------------------------- keyboard

    Private Sub HandleKeyboard(keyboard As KeyboardState)
        If _focusedWindow Is Nothing OrElse _focusedWindow.IsMinimized Then Return

        Dim surface = _focusedWindow.AvaloniaSurface
        Dim shift = keyboard.IsKeyDown(Keys.LeftShift) OrElse keyboard.IsKeyDown(Keys.RightShift)
        Dim ctrl = keyboard.IsKeyDown(Keys.LeftControl) OrElse keyboard.IsKeyDown(Keys.RightControl)
        Dim alt = keyboard.IsKeyDown(Keys.LeftAlt) OrElse keyboard.IsKeyDown(Keys.RightAlt)
        Dim capsLock = keyboard.CapsLock

        For Each key In keyboard.GetPressedKeys()
            If Not _lastKeyState.IsKeyDown(key) Then
                Dim ch = ComputeChar(key, shift, capsLock)
                If surface IsNot Nothing Then
                    surface.PostKeyDown(AvaloniaInput.MapKey(key), AvaloniaInput.MapPhysicalKey(key),
                                        ctrl, shift, alt)
                    ' KeyPress does not synthesize text input; only KeyTextInput
                    ' lands characters in a focused control. Editing and navigation
                    ' keys (Enter, Back, Tab) are handled by control commands on the
                    ' key event and must not be re-typed as text.
                    If ch <> ChrW(0) AndAlso ch > ChrW(31) Then
                        surface.PostText(ch.ToString())
                    End If
                Else
                    Dim consoleKey = MapToConsoleKey(key)
                    If consoleKey.HasValue Then
                        Dim keyInfo As New ConsoleKeyInfo(
                            ch,
                            consoleKey.Value,
                            shift, alt, ctrl)
                        _focusedWindow.HandleWidgetKey(keyInfo)
                    End If
                End If
            End If
        Next

        ' Key releases (the legacy widget path never needed them; Avalonia does).
        If surface IsNot Nothing Then
            For Each key In _lastKeyState.GetPressedKeys()
                If Not keyboard.IsKeyDown(key) Then
                    surface.PostKeyUp(AvaloniaInput.MapKey(key), AvaloniaInput.MapPhysicalKey(key),
                                      ctrl, shift, alt)
                End If
            Next
        End If
    End Sub

    Private Shared Function ComputeChar(key As Keys, shift As Boolean, capsLock As Boolean) As Char
        Dim shiftChars = shift Xor capsLock
        If key >= Keys.A AndAlso key <= Keys.Z Then
            Return ChrW(AscW(If(shiftChars, "A"c, "a"c)) + (key - Keys.A))
        ElseIf key >= Keys.D0 AndAlso key <= Keys.D9 Then
            Dim digitCh = ChrW(AscW("0"c) + (key - Keys.D0))
            Return If(shift, ShiftedDigit(digitCh), digitCh)
        ElseIf key >= Keys.NumPad0 AndAlso key <= Keys.NumPad9 Then
            Dim digitCh = ChrW(AscW("0"c) + (key - Keys.NumPad0))
            Return If(shift, ShiftedDigit(digitCh), digitCh)
        ElseIf key = Keys.Space Then
            Return " "c
        ElseIf key = Keys.Enter Then
            Return CChar(vbCr)
        ElseIf key = Keys.Back Then
            Return ChrW(8)
        Else
            Return ShiftedOemChar(key, shift)
        End If
    End Function

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
            Case Keys.Add : Return ConsoleKey.Add
            Case Keys.Subtract : Return ConsoleKey.Subtract
            Case Keys.Oem8 : Return ConsoleKey.Oem8
            Case Else : Return Nothing
        End Select
    End Function

    ' -------------------------------------------------------------- draw

    Protected Overrides Sub Draw(gameTime As GameTime)
        GraphicsDevice.Clear(Theme.DesktopBottom)

        ' Render widget trees (skip minimized windows).
        SyncLock _lock
            For Each w In _windows.Where(Function(win) win.IsVisible AndAlso Not win.IsMinimized)
                w.RenderWidgetTree(DisplayServer.Instance.Graphics, ClientRectFor(w))
                w.RenderAvaloniaScene(DisplayServer.Instance.Graphics)
            Next
        End SyncLock

        _batch.Begin()

        _shapes.DrawDesktop(_batch, GraphicsDevice.Viewport.Bounds)

        SyncLock _lock
            For Each w In _windows.Where(Function(win) win.IsVisible AndAlso Not win.IsMinimized).OrderBy(Function(win) win.ZOrder)
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
        Dim active = (win Is _focusedWindow)
        Dim tbH = Theme.TitleBarHeight
        Dim fullRect = New Rectangle(win.X, win.Y, win.Width, win.Height)
        Dim titleRect = New Rectangle(win.X, win.Y, win.Width, tbH)
        Dim bodyRect = New Rectangle(win.X, win.Y + tbH, win.Width, win.Height - tbH)

        ' Shadow.
        Dim shadowAlpha = If(active, 0.42F, 0.18F)
        _shapes.DrawWindowBase(_batch, fullRect, active, shadowAlpha)

        ' Title bar chrome (gradient + lip + rounded top corners).
        _shapes.DrawTitleBar(_batch, New Rectangle(win.X + 1, win.Y + 1, win.Width - 2, tbH), active)

        ' Body surface.
        _shapes.DrawBody(_batch, New Rectangle(win.X + 1, win.Y + tbH, win.Width - 2, win.Height - tbH - 1), Theme.WindowSurface)

        ' Title (Fira Code, tracked).
        Dim titleColor = If(active, Theme.Ink, Theme.InkMuted)
        Dim titleX = win.X + Theme.TitleLeftX
        Dim maxTitleW = (win.MinimizeRect.X - Theme.TitleMargin) - titleX
        If _uiFont IsNot Nothing Then
            Dim titleH = CInt(Math.Max(8, _uiFont.MeasureString("Ag").Y))
            Dim titleY = win.Y + CInt((tbH - titleH) / 2)
            Dim titleText = win.Title
            If _uiFont.MeasureString(titleText).X > maxTitleW Then
                Dim suffix = "..."
                While titleText.Length > 0 AndAlso _uiFont.MeasureString(titleText).X + _uiFont.MeasureString(suffix).X > maxTitleW
                    titleText = titleText.Substring(0, titleText.Length - 1)
                End While
                titleText &= suffix
            End If
            _batch.DrawString(_uiFont, titleText, New Vector2(titleX, titleY), titleColor)
        Else
            _font.DrawString(_batch, win.Title, New Vector2(titleX, win.Y + CInt((tbH - _font.CharHeight) / 2)), titleColor)
        End If

        ' Window icon.
        IconRenderer.Draw(_batch, IconRenderer.IconType.Window,
                          win.X + Theme.TitleIconX, win.Y + Theme.TitleIconTop, Theme.TitleIconSize, titleColor)

        ' Caption buttons (minimize, maximize, restore, close).
        Dim closeHov = (active AndAlso _hoveredCaptionWin Is win AndAlso _hoveredCaption = AppWindow.CaptionHit.CaptionClose)
        Dim maxHov = (active AndAlso _hoveredCaptionWin Is win AndAlso _hoveredCaption = AppWindow.CaptionHit.CaptionMaximize)
        Dim minHov = (active AndAlso _hoveredCaptionWin Is win AndAlso _hoveredCaption = AppWindow.CaptionHit.CaptionMinimize)

        DrawCaptionButton(_batch, win.CloseRect, IconRenderer.IconType.Close, closeHov, True)
        Dim maxIcon = If(win.IsMaximized, IconRenderer.IconType.Restore, IconRenderer.IconType.Maximize)
        DrawCaptionButton(_batch, win.MaximizeRect, maxIcon, maxHov, False)
        DrawCaptionButton(_batch, win.MinimizeRect, IconRenderer.IconType.Minimize, minHov, False)

        ' Client content (Avalonia surface, widget texture, or raw pixels).
        Dim clientRect = New Rectangle(win.ClientX, win.ClientY, win.ClientWidth, win.ClientHeight)
        If win.AvaloniaTexture IsNot Nothing Then
            _batch.Draw(win.AvaloniaTexture, clientRect, Color.White)
        ElseIf win.WidgetTexture IsNot Nothing Then
            _batch.Draw(win.WidgetTexture, clientRect, Color.White)
        ElseIf win.ContentTexture IsNot Nothing AndAlso Not win.ContentTexture.IsDisposed Then
            _batch.Draw(win.ContentTexture, New Vector2(clientRect.X, clientRect.Y), Color.White)
        End If

        DrawResizeGrip(win)
    End Sub

    ''' Three diagonal ticks in the bottom-right frame that mark the resize grab.
    ''' Quiet at rest, brightens while the corner is hovered.
    Private Sub DrawResizeGrip(win As AppWindow)
        If win.IsMaximized Then Return
        Dim s = Theme.UiScale
        Dim hovered = (_hoveredResizeWin Is win AndAlso
                       (_hoveredResizeZone = ResizeZone.SouthEast OrElse _hoveredResizeZone = ResizeZone.South OrElse _hoveredResizeZone = ResizeZone.East))
        Dim col = If(hovered, Theme.Hairline, Theme.HairlineFaint)
        Dim inset = CInt(3 * s)
        Dim cornerX = win.X + win.Width - inset
        Dim cornerY = win.Y + win.Height - inset
        Dim tickLen = Math.Max(3, CInt(4 * s))
        Dim spacing = Math.Max(2, CInt(3 * s))
        For i = 0 To 2
            Dim tickX = cornerX - i * spacing
            For j = 0 To tickLen - 1
                batchFill(_batch, New Rectangle(tickX - j, cornerY - j, 1, 1), col)
            Next
        Next
    End Sub

    Private Sub DrawCaptionButton(batch As SpriteBatch, rect As Rectangle, icon As IconRenderer.IconType, hovered As Boolean, isClose As Boolean)
        Dim iconSize = Theme.CaptionIconSize
        Dim iconX = rect.X + (rect.Width - iconSize) \ 2
        Dim iconY = rect.Y + (rect.Height - iconSize) \ 2
        If hovered Then
            Dim bgColor = If(isClose, Theme.CloseHover, Theme.SurfaceHover)
            _shapes.DrawRoundedControl(batch, rect, bgColor)
            IconRenderer.Draw(batch, icon, iconX, iconY, iconSize, Color.White)
        Else
            Dim iconColor = If(isClose, Theme.Close, Theme.InkMuted)
            IconRenderer.Draw(batch, icon, iconX, iconY, iconSize, iconColor)
        End If
    End Sub

    ' -------------------------------------------------------------- taskbar

    Private Sub DrawTaskbar()
        Dim barY = GraphicsDevice.Viewport.Height - Theme.TaskbarHeight

        ' Bar surface.
        batchFill(_batch, New Rectangle(0, barY, GraphicsDevice.Viewport.Width, Theme.TaskbarHeight),
                  New Color(20, 18, 15, 232))
        ' Hairline top edge.
        batchFill(_batch, New Rectangle(0, barY, GraphicsDevice.Viewport.Width, 1), Theme.HairlineFaint)

        Dim s = Theme.UiScale
        Dim x = CInt(6 * s)
        Dim idx = 0
        SyncLock _lock
            Dim visibleWins = _windows.Where(Function(w) w.IsOpen).ToList()
            For i = 0 To visibleWins.Count - 1
                Dim w = visibleWins(i)
                Dim btnW = TaskbarButtonWidth(w.Title)
                Dim btnRect = New Rectangle(x, barY + 5, btnW, Theme.TaskbarHeight - 10)
                Dim active = (w Is _focusedWindow)
                Dim hovered = (_hoveredTaskIdx = i)

                ' Button surface.
                Dim bg As Color
                If active Then
                    bg = Theme.SurfaceRaised
                ElseIf hovered Then
                    bg = Theme.SurfaceHover
                Else
                    bg = Color.Transparent
                End If
                If bg.A > 0 Then _shapes.DrawRoundedControl(_batch, btnRect, bg)

                ' Left accent bar (active only).
                If active Then
                    Dim accentPad = CInt(4 * s)
                    batchFill(_batch, New Rectangle(btnRect.X, btnRect.Y + accentPad, 3, btnRect.Height - accentPad * 2), Theme.Accent)
                End If

                ' Icon + title.
                Dim textColor = If(w.IsMinimized, Theme.InkFaint, If(active, Theme.Ink, Theme.InkMuted))
                Dim iconSize = Theme.CaptionIconSize
                Dim iconY = btnRect.Y + (btnRect.Height - iconSize) \ 2
                Dim iconPad = CInt(10 * s)
                Dim iconX = btnRect.X + iconPad
                If active Then iconX += CInt(4 * s)
                IconRenderer.Draw(_batch, IconRenderer.IconType.Window, iconX, iconY, iconSize, textColor)

                If _taskFont IsNot Nothing Then
                    Dim txtH = CInt(Math.Max(8, _taskFont.MeasureString("Ag").Y))
                    Dim textY = btnRect.Y + CInt((btnRect.Height - txtH) / 2)
                    _batch.DrawString(_taskFont, w.Title, New Vector2(iconX + iconSize + CInt(6 * s), textY), textColor)
                Else
                    _font.DrawString(_batch, w.Title, New Vector2(iconX + iconSize + CInt(6 * s), btnRect.Y + CInt(10 * s)), textColor)
                End If

                x += btnW + CInt(3 * s)
                idx += 1
            Next
        End SyncLock
    End Sub

    Private Shared Sub batchFill(batch As SpriteBatch, rect As Rectangle, color As Color)
        batch.Draw(DisplayServer.Instance.WhitePixel, rect, color)
    End Sub
End Class
