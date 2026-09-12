Imports System.Collections.Concurrent
Imports Avalonia.Controls
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class AppWindow
    Public Property Title As String
    Public Property X As Integer
    Public Property Y As Integer
    Public Property Width As Integer
    Public Property Height As Integer
    Public Property IsVisible As Boolean = True
    Public Property IsFocused As Boolean = False
    Public Property ZOrder As Integer = 0
    Public Property IsOpen As Boolean = True
    Public Property IsMinimized As Boolean = False
    Public Property IsMaximized As Boolean = False

    Private _restoreRect As Rectangle
    Private _restoreWidth As Integer
    Private _restoreHeight As Integer

    Public Const BorderWidth As Integer = 1

    Private _content As Texture2D
    Private _pendingContent As Color() = Nothing
    Private _pendingLock As New Object()

    Private _rootWidget As Widget = Nothing
    Private _widgetRenderTarget As RenderTarget2D = Nothing
    Private _focusedWidget As Widget = Nothing

    Public Enum CaptionHit
        None
        CaptionClose
        CaptionMaximize
        CaptionMinimize
    End Enum

    Public Property RootWidget As Widget
        Get
            Return _rootWidget
        End Get
        Set(value As Widget)
            _rootWidget = value
            _focusedWidget = Nothing
            If value IsNot Nothing Then
                value.X = 0
                value.Y = 0
                ApplyContentSize()
            End If
            FocusFirstWidget()
        End Set
    End Property

    ' ------------------------------------------------------ Avalonia content

    Private _rootVisualFactory As Func(Of Avalonia.Controls.Control) = Nothing
    Private _avSurface As AvaloniaSurface = Nothing
    Private _avTexture As Texture2D = Nothing

    ''' Optional Avalonia root content for this window. When set, the window's
    ''' client area is rendered by the offscreen Avalonia runtime (on its own
    ''' thread) instead of the legacy widget tree, and composited by MonoGame.
    ''' The factory is invoked on the Avalonia UI thread, so controls are born
    ''' and attached on their own thread.
    Public Property RootVisualFactory As Func(Of Avalonia.Controls.Control)
        Get
            Return _rootVisualFactory
        End Get
        Set(value As Func(Of Avalonia.Controls.Control))
            _rootVisualFactory = value
            If value Is Nothing AndAlso _avSurface Is Nothing Then Return
            EnsureAvaloniaSurface()
            If _avSurface IsNot Nothing Then
                _avSurface.PostSetContent(value)
            End If
        End Set
    End Property

    ''' The offscreen surfaces backing this window, if any.
    Friend ReadOnly Property AvaloniaSurface As AvaloniaSurface
        Get
            Return _avSurface
        End Get
    End Property

    Public ReadOnly Property AvaloniaTexture As Texture2D
        Get
            Return _avTexture
        End Get
    End Property

    Private Sub EnsureAvaloniaSurface()
        If _avSurface IsNot Nothing Then Return
        If Not AvaloniaHost.Instance.IsRunning Then Return
        _avSurface = AvaloniaHost.Instance.CreateSurface(ClientWidth, ClientHeight)
    End Sub

    ''' Keep the offscreen Avalonia window sized to the client region; called
    ''' from the game thread after any size change.
    Public Sub ResizeAvaloniaSurface()
        If _avSurface Is Nothing Then Return
        _avSurface.PostResize(ClientWidth, ClientHeight)
    End Sub

    ''' Game thread: pull the latest rendered frame from the surface into a
    ''' MonoGame texture ready for Draw.
    Public Sub RenderAvaloniaScene(graphics As GraphicsDevice)
        If _avSurface Is Nothing Then Return
        _avTexture = _avSurface.GetTexture(graphics)
    End Sub

    Public Sub FocusFirstWidget()
        If _rootWidget Is Nothing Then Return
        Dim focusable = FindFocusable(_rootWidget)
        If focusable IsNot Nothing Then
            _focusedWidget = focusable
            If TypeOf focusable Is TextBox Then
                DirectCast(focusable, TextBox).Focus()
            ElseIf TypeOf focusable Is Terminal Then
                DirectCast(focusable, Terminal).Focus()
            End If
        End If
    End Sub

    Private Shared Function FindFocusable(widget As Widget) As Widget
        If TypeOf widget Is TextBox OrElse TypeOf widget Is Terminal Then
            Return widget
        End If
        For Each child In widget.Children
            Dim found = FindFocusable(child)
            If found IsNot Nothing Then Return found
        Next
        Return Nothing
    End Function

    Public ReadOnly Property FocusedWidget As Widget
        Get
            Return _focusedWidget
        End Get
    End Property

    Public Sub New(title As String, x As Integer, y As Integer, width As Integer, height As Integer)
        Me.Title = title
        Me.X = x
        Me.Y = y
        ' Width/Height are the DESIGN size (scale 1.0). Client content is laid
        ' out in design units and scaled by Theme.UiScale at render time.
        _designW = Math.Max(1, width - BorderWidth * 2)
        _designH = Math.Max(1, height - Theme.BaseTitleBarHeight - BorderWidth * 2)
        SizeToScale()
    End Sub

    ''' Design-space client content size (scale 1.0), the coordinate space the
    ''' widget tree is laid out in.
    Public ReadOnly Property DesignClientWidth As Integer
        Get
            Return _designW
        End Get
    End Property

    Public ReadOnly Property DesignClientHeight As Integer
        Get
            Return _designH
        End Get
    End Property

    Public ReadOnly Property ContentScale As Single
        Get
            Return Theme.UiScale
        End Get
    End Property

    Private _designW As Integer
    Private _designH As Integer

    ''' Recompute the physical window size from the design size at the current
    ''' UI scale, keeping maximized windows on the wall. Call after Theme.UiScale
    ''' changes. The widget render target and root widget resize with it.
    Public Sub SizeToScale()
        If Not IsMaximized Then
            Dim s = Theme.UiScale
            Width = CInt((_designW * s) + 0.5F) + BorderWidth * 2
            Height = Theme.TitleBarHeight + CInt((_designH * s) + 0.5F) + BorderWidth * 2
        End If
        ApplyContentSize()
    End Sub

    ''' Keep the root widget sized to the window's client region expressed in
    ''' design units (physical client / scale).
    Public Sub ApplyContentSize()
        If _rootWidget Is Nothing AndAlso _avSurface Is Nothing Then Return
        Dim s = Theme.UiScale
        If _rootWidget IsNot Nothing Then
            _rootWidget.Width = Math.Max(1, CInt((Width - BorderWidth * 2) / s + 0.5F))
            _rootWidget.Height = Math.Max(1, CInt((Height - Theme.TitleBarHeight - BorderWidth * 2) / s + 0.5F))
        End If
        ResizeAvaloniaSurface()
    End Sub

    Public ReadOnly Property ClientX As Integer
        Get
            Return X + BorderWidth
        End Get
    End Property

    Public ReadOnly Property ClientY As Integer
        Get
            Return Y + Theme.TitleBarHeight + BorderWidth
        End Get
    End Property

    Public ReadOnly Property ClientWidth As Integer
        Get
            Return Math.Max(1, Width - BorderWidth * 2)
        End Get
    End Property

    Public ReadOnly Property ClientHeight As Integer
        Get
            Return Math.Max(1, Height - Theme.TitleBarHeight - BorderWidth * 2)
        End Get
    End Property

    Public Sub SetPixels(pixels As Color(), width As Integer, height As Integer)
        SyncLock _pendingLock
            _pendingContent = pixels
        End SyncLock
    End Sub

    Public Sub ApplyPendingContent()
        SyncLock _pendingLock
            If _pendingContent IsNot Nothing Then
                If _content Is Nothing OrElse _content.IsDisposed OrElse
                   _content.Width <> ClientWidth OrElse _content.Height <> ClientHeight Then
                    _content?.Dispose()
                    _content = New Texture2D(_graphicsRef, ClientWidth, ClientHeight)
                End If
                _content.SetData(_pendingContent)
                _pendingContent = Nothing
            End If
        End SyncLock
    End Sub

    Private _graphicsRef As GraphicsDevice = Nothing

    Public Sub InitGraphics(graphics As GraphicsDevice)
        _graphicsRef = graphics
        If _content Is Nothing OrElse _content.IsDisposed Then
            If ClientWidth > 0 AndAlso ClientHeight > 0 Then
                _content = New Texture2D(graphics, ClientWidth, ClientHeight)
                Dim clear(ClientWidth * ClientHeight - 1) As Color
                For i = 0 To clear.Length - 1
                    clear(i) = Theme.WindowSurface
                Next
                _content.SetData(clear)
            End If
        End If
    End Sub

    Public Sub ClearContent(color As Color)
        If _content IsNot Nothing AndAlso Not _content.IsDisposed Then
            Dim data(ClientWidth * ClientHeight - 1) As Color
            For i = 0 To data.Length - 1
                data(i) = color
            Next
            _content.SetData(data)
        End If
    End Sub

    Public Sub SetPixelDirect(x As Integer, y As Integer, color As Color)
        If _content Is Nothing OrElse _content.IsDisposed Then Return
        If x < 0 OrElse x >= ClientWidth OrElse y < 0 OrElse y >= ClientHeight Then Return
        Dim data(0) As Color
        data(0) = color
        _content.SetData(0, New Rectangle(x, y, 1, 1), data, 0, 1)
    End Sub

    Public ReadOnly Property ContentTexture As Texture2D
        Get
            Return _content
        End Get
    End Property

    Public Function ContainsPoint(px As Integer, py As Integer) As Boolean
        Return px >= X AndAlso px < X + Width AndAlso py >= Y AndAlso py < Y + Height
    End Function

    Public Function InTitleBar(px As Integer, py As Integer) As Boolean
        Return px >= X AndAlso px < X + Width AndAlso py >= Y AndAlso py < Y + Theme.TitleBarHeight
    End Function

    Public ReadOnly Property CloseRect As Rectangle
        Get
            Return New Rectangle(X + Width - Theme.CaptionRightPad - Theme.CaptionButtonSize,
                                 Y + Theme.CaptionButtonTop, Theme.CaptionButtonSize, Theme.CaptionButtonSize)
        End Get
    End Property

    Public ReadOnly Property MaximizeRect As Rectangle
        Get
            Return New Rectangle(CloseRect.X - Theme.CaptionGap - Theme.CaptionButtonSize,
                                 Y + Theme.CaptionButtonTop, Theme.CaptionButtonSize, Theme.CaptionButtonSize)
        End Get
    End Property

    Public ReadOnly Property MinimizeRect As Rectangle
        Get
            Return New Rectangle(MaximizeRect.X - Theme.CaptionGap - Theme.CaptionButtonSize,
                                 Y + Theme.CaptionButtonTop, Theme.CaptionButtonSize, Theme.CaptionButtonSize)
        End Get
    End Property

    Public Function HitCaptionButton(px As Integer, py As Integer) As CaptionHit
        If CloseRect.Contains(px, py) Then Return CaptionHit.CaptionClose
        If MaximizeRect.Contains(px, py) Then Return CaptionHit.CaptionMaximize
        If MinimizeRect.Contains(px, py) Then Return CaptionHit.CaptionMinimize
        Return CaptionHit.None
    End Function

    Public Sub Minimize()
        If IsMinimized Then Return
        IsMinimized = True
    End Sub

    Public Sub ToggleMaximize(workArea As Rectangle)
        If IsMaximized Then
            IsMaximized = False
            X = _restoreRect.X
            Y = _restoreRect.Y
            Width = _restoreRect.Width
            Height = _restoreRect.Height
            ApplyContentSize()
        Else
            _restoreRect = New Rectangle(X, Y, Width, Height)
            IsMaximized = True
            X = workArea.X
            Y = workArea.Y
            Width = workArea.Width
            Height = workArea.Height
            ApplyContentSize()
        End If
    End Sub

    Public Sub WaitForClose()
        While IsOpen
            Threading.Thread.Sleep(50)
        End While
    End Sub

    Public Sub Dispose()
        _content?.Dispose()
        _widgetRenderTarget?.Dispose()
        _avTexture?.Dispose()
        _avTexture = Nothing
        If _avSurface IsNot Nothing Then
            _avSurface.DisposeTexture()
            If AvaloniaHost.Instance.TryDisposeSurface(_avSurface) Then
                _avSurface = Nothing
            End If
        End If
    End Sub

    Public Sub UpdateWidgets(gameTime As GameTime)
        _rootWidget?.Update(gameTime)
    End Sub

    Public ReadOnly Property WidgetTexture As Texture2D
        Get
            Return _widgetRenderTarget
        End Get
    End Property

    Public Sub RenderWidgetTree(graphics As GraphicsDevice, clientRect As Rectangle)
        If _rootWidget Is Nothing Then
            _widgetRenderTarget?.Dispose()
            _widgetRenderTarget = Nothing
            Return
        End If

        If _widgetRenderTarget Is Nothing OrElse _widgetRenderTarget.IsDisposed OrElse
           _widgetRenderTarget.Width <> ClientWidth OrElse _widgetRenderTarget.Height <> ClientHeight Then
            _widgetRenderTarget?.Dispose()
            _widgetRenderTarget = New RenderTarget2D(graphics, ClientWidth, ClientHeight)
        End If

        graphics.SetRenderTarget(_widgetRenderTarget)
        graphics.Clear(Theme.WindowSurface)

        Dim sprites As New SpriteBatch(graphics)
        sprites.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                      SamplerState.PointClamp, Nothing, Nothing, Nothing,
                      Matrix.CreateScale(Theme.UiScale))
        _rootWidget.Draw(sprites, DisplayServer.Instance.Font)
        sprites.End()

        graphics.SetRenderTarget(Nothing)
    End Sub

    Public Sub HandleWidgetClick(localX As Integer, localY As Integer)
        If _rootWidget Is Nothing Then Return
        Dim hit = _rootWidget.HitTest(localX, localY)
        If hit IsNot Nothing Then
            If _focusedWidget IsNot Nothing AndAlso _focusedWidget IsNot hit Then
                If TypeOf _focusedWidget Is TextBox Then
                    DirectCast(_focusedWidget, TextBox).Unfocus()
                ElseIf TypeOf _focusedWidget Is Terminal Then
                    DirectCast(_focusedWidget, Terminal).Unfocus()
                End If
            End If
            _focusedWidget = hit
            If TypeOf hit Is TextBox Then
                DirectCast(hit, TextBox).Focus()
            ElseIf TypeOf hit Is Terminal Then
                DirectCast(hit, Terminal).Focus()
            End If
            hit.OnMouseClick(localX - hit.X, localY - hit.Y)
        End If
    End Sub

    Public Sub HandleWidgetKey(keyInfo As ConsoleKeyInfo)
        _focusedWidget?.OnKeyPress(keyInfo)
    End Sub

    Public Sub HandleWidgetMouseMove(localX As Integer, localY As Integer)
        If _rootWidget Is Nothing Then Return
        Dim hit = _rootWidget.HitTest(localX, localY)
        If hit IsNot _lastHoveredWidget Then
            _lastHoveredWidget?.OnMouseLeave()
            hit?.OnMouseEnter()
            _lastHoveredWidget = hit
        End If
    End Sub

    Public Sub HandleWidgetScroll(delta As Integer)
        _focusedWidget?.OnMouseScroll(delta)
        If _focusedWidget Is Nothing AndAlso _rootWidget IsNot Nothing Then
            Dim hit = _rootWidget.HitTest(_lastMouseLocalX, _lastMouseLocalY)
            hit?.OnMouseScroll(delta)
        End If
    End Sub

    Private _lastHoveredWidget As Widget = Nothing
    Private _lastMouseLocalX As Integer = 0
    Private _lastMouseLocalY As Integer = 0

    Public Sub TrackMousePosition(localX As Integer, localY As Integer)
        _lastMouseLocalX = localX
        _lastMouseLocalY = localY
    End Sub
End Class