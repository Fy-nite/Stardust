Imports System.Collections.Concurrent
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

    Public Const TitleBarHeight As Integer = 24
    Public Const BorderWidth As Integer = 2

    Private _content As Texture2D
    Private _pendingContent As Color() = Nothing
    Private _pendingLock As New Object()

    Private _rootWidget As Widget = Nothing
    Private _widgetRenderTarget As RenderTarget2D = Nothing
    Private _focusedWidget As Widget = Nothing

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
                value.Width = ClientWidth
                value.Height = ClientHeight
            End If
            FocusFirstWidget()
        End Set
    End Property

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
        Me.Width = width
        Me.Height = height
    End Sub

    Public ReadOnly Property ClientX As Integer
        Get
            Return X + BorderWidth
        End Get
    End Property

    Public ReadOnly Property ClientY As Integer
        Get
            Return Y + TitleBarHeight + BorderWidth
        End Get
    End Property

    Public ReadOnly Property ClientWidth As Integer
        Get
            Return Math.Max(1, Width - BorderWidth * 2)
        End Get
    End Property

    Public ReadOnly Property ClientHeight As Integer
        Get
            Return Math.Max(1, Height - TitleBarHeight - BorderWidth * 2)
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
                    clear(i) = New Color(24, 24, 37)
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
        Return px >= X AndAlso px < X + Width AndAlso py >= Y AndAlso py < Y + TitleBarHeight
    End Function

    Public Function InCloseButton(px As Integer, py As Integer) As Boolean
        Dim closeX = X + Width - 22
        Dim closeY = Y + 4
        Return px >= closeX AndAlso px < closeX + 18 AndAlso py >= closeY AndAlso py < closeY + 16
    End Function

    Public Sub WaitForClose()
        While IsOpen
            Threading.Thread.Sleep(50)
        End While
    End Sub

    Public Sub Dispose()
        _content?.Dispose()
        _widgetRenderTarget?.Dispose()
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
        graphics.Clear(New Color(24, 24, 37))

        Dim sprites As New SpriteBatch(graphics)
        sprites.Begin()
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
