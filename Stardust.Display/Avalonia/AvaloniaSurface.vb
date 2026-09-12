Imports System.Runtime.InteropServices
Imports Avalonia
Imports Avalonia.Controls
Imports Avalonia.Headless
Imports Avalonia.Input
Imports Avalonia.Media
Imports Avalonia.Platform
Imports Avalonia.Threading
Imports Microsoft.Xna.Framework.Graphics
Imports AvaloniaPanel = Avalonia.Controls.Panel

''' One window's offscreen Avalonia content surface. Lives on the Avalonia
''' thread (its Window and layout state are UI-thread-bound); the MonoGame game
''' thread only pulls finished pixel buffers via GetTexture. Pixel buffers are
''' double-buffered so the two threads never race on a half-written frame.
Public NotInheritable Class AvaloniaSurface

    Private _window As Window
    Private _root As AvaloniaPanel

    Private _widthPx As Integer
    Private _heightPx As Integer

    ' Double buffer: the UI thread fills _frames(_writeIndex) then flips; the
    ' game thread always reads the other (last completed) buffer, so a copy and
    ' an upload never race on the same array.
    Private _frames As Byte()()
    Private _writeIndex As Integer = 0

    Private _dirty As Boolean = False
    Private _hasFrame As Boolean = False
    Private _disposed As Boolean = False

    Private _texture As Texture2D = Nothing

    ''' UI thread only. Creates the hidden offscreen window and shows it.
    Friend Sub New(widthPx As Integer, heightPx As Integer)
        _widthPx = Math.Max(1, widthPx)
        _heightPx = Math.Max(1, heightPx)
        _frames = New Byte(1)() {}
        _frames(0) = New Byte(0) {}
        _frames(1) = New Byte(0) {}
        AllocateBuffers()

        _window = New Window With {
            .WindowDecorations = WindowDecorations.None,
            .ShowInTaskbar = False,
            .ShowActivated = False,
            .CanResize = False,
            .Background = New SolidColorBrush(ThemeWindow)
        }
        ApplyWindowLogicalSize()
        _window.Position = New PixelPoint(-32000, -32000)

        _root = New AvaloniaPanel With {
            .Background = New SolidColorBrush(ThemeWindow)
        }
        _window.Content = _root
        _window.Show()
    End Sub

    ' The offscreen window lives in design points; render scaling maps them to
    ' the physical client pixels, so Avalonia content scales with the desktop
    ' (UiScale) exactly like the chrome does, while the framebuffer stays
    ' _widthPx x _heightPx physical.
    Private Sub ApplyWindowLogicalSize()
        Dim scale = Math.Max(0.1F, Theme.UiScale)
        _window.Width = CInt(CDbl(_widthPx) / scale + 0.5)
        _window.Height = CInt(CDbl(_heightPx) / scale + 0.5)
        _window.SetRenderScaling(scale)
    End Sub

    Private Shared Function ThemeWindow As Color
        Return Color.FromArgb(255, CByte(33), CByte(30), CByte(26))
    End Function

    Public ReadOnly Property WidthPx As Integer
        Get
            Return _widthPx
        End Get
    End Property

    Public ReadOnly Property HeightPx As Integer
        Get
            Return _heightPx
        End Get
    End Property

    Public ReadOnly Property HasFrame As Boolean
        Get
            Return _hasFrame
        End Get
    End Property

    ' -------------------------------------------------------------- content

    ''' Any thread. Swap this surface's app content; the factory runs on the
    ''' Avalonia UI thread so controls are created where they belong.
    Friend Sub PostSetContent(factory As Func(Of Control))
        If _disposed OrElse factory Is Nothing Then Return
        Dispatcher.UIThread.Post(Sub()
                                     Try
                                         Dim content = factory()
                                         _root.Children.Clear()
                                         If content IsNot Nothing Then
                                             _root.Children.Add(content)
                                         End If
                                         _dirty = True
                                     Catch ex As Exception
                                         Console.Error.WriteLine($"[AvaloniaSurface] SetContent failed: {ex.Message}")
                                     End Try
                                 End Sub)
    End Sub

    ''' Any thread. Ask for a re-render on the next host pump.
    Public Sub Invalidate()
        _dirty = True
    End Sub

    Friend ReadOnly Property IsDirty As Boolean
        Get
            Return _dirty
        End Get
    End Property

    ''' UI thread only, called from the host pump after a render tick.
    Friend Sub RenderFrame()
        If _disposed OrElse _window Is Nothing Then Return
        Try
            Dim bmp = _window.CaptureRenderedFrame()
            If bmp Is Nothing Then Return

            Using fb = bmp.Lock()
                Dim w = fb.Size.Width
                Dim h = fb.Size.Height
                If w <= 0 OrElse h <= 0 Then Return

                If w <> _widthPx OrElse h <> _heightPx Then
                    _widthPx = w
                    _heightPx = h
                    AllocateBuffers()
                End If

                Dim target = _frames(_writeIndex)
                Dim rowBytes = fb.RowBytes
                If rowBytes = w * 4 Then
                    Marshal.Copy(fb.Address, target, 0, target.Length)
                Else
                    ' Row-padded framebuffer: copy row by row into a packed array.
                    Const bpp As Integer = 4
                    For y = 0 To h - 1
                        Marshal.Copy(IntPtr.Add(fb.Address, y * rowBytes),
                                     target, y * w * bpp, w * bpp)
                    Next
                End If

                _writeIndex = If(_writeIndex = 0, 1, 0)
                _hasFrame = True
            End Using
        Catch ex As Exception
            Console.Error.WriteLine($"[AvaloniaSurface] render failed: {ex.Message}")
        End Try
        _dirty = False
    End Sub

    Private Sub AllocateBuffers()
        Dim len = _widthPx * _heightPx * 4
        _frames(0) = New Byte(If(len < 1, 4, len) - 1) {}
        _frames(1) = New Byte(If(len < 1, 4, len) - 1) {}
    End Sub

    ''' UI thread only. Resize the offscreen window (physical pixels).
    Friend Sub Resize(widthPx As Integer, heightPx As Integer)
        widthPx = Math.Max(1, widthPx)
        heightPx = Math.Max(1, heightPx)
        If widthPx = _widthPx AndAlso heightPx = _heightPx Then Return
        _widthPx = widthPx
        _heightPx = heightPx
        AllocateBuffers()
        _hasFrame = False
        ApplyWindowLogicalSize()
        _dirty = True
    End Sub

    ''' Any thread. Ask to resize on the Avalonia thread (used from the game
    ''' thread whenever the MonoGame window changes size).
    Public Sub PostResize(widthPx As Integer, heightPx As Integer)
        If _disposed Then Return
        Dispatcher.UIThread.Post(Sub() Resize(widthPx, heightPx))
    End Sub

    ' --------------------------------------------------------------- pixels

    ''' Game thread. Pulls the latest finished frame into a texture ready for
    ''' the SpriteBatch. Texture is created/updated on the graphics thread. The
    ''' framebuffer is Rgba8888, which matches MonoGame's SurfaceFormat.Color
    ''' (GL_RGBA / unsigbyte); Bgra32 is not implemented by the DesktopGL
    ''' backend's GetGLFormat.
    Public Function GetTexture(graphics As GraphicsDevice) As Texture2D
        If Not _hasFrame OrElse _disposed Then Return Nothing

        Dim w = _widthPx
        Dim h = _heightPx
        Dim data = _frames(1 - _writeIndex)
        If data Is Nothing OrElse data.Length <> w * h * 4 Then Return Nothing

        If _texture Is Nothing OrElse _texture.IsDisposed OrElse
           _texture.Width <> w OrElse _texture.Height <> h Then
            DisposeTexture()
            _texture = New Texture2D(graphics, w, h, False, SurfaceFormat.Color)
        End If

        _texture.SetData(data)
        Return _texture
    End Function

    Public ReadOnly Property Texture As Texture2D
        Get
            Return _texture
        End Get
    End Property

    ' ---------------------------------------------------------------- input

    Public Sub PostPointer(x As Integer, y As Integer,
                           leftDown As Boolean, rightDown As Boolean, middleDown As Boolean)
        If _disposed Then Return
        Dim mods = BuildMouseModifiers(leftDown, rightDown, middleDown)
        Dispatcher.UIThread.Post(Sub()
                                     Try
                                         _window.MouseMove(New Point(x, y), mods)
                                     Catch
                                     End Try
                                 End Sub)
    End Sub

    Public Sub PostPointerDown(x As Integer, y As Integer, button As MouseButton,
                               leftDown As Boolean, rightDown As Boolean, middleDown As Boolean)
        If _disposed Then Return
        Dim mods = BuildMouseModifiers(leftDown, rightDown, middleDown)
        Dispatcher.UIThread.Post(Sub()
                                     Try
                                         _window.MouseDown(New Point(x, y), button, mods)
                                     Catch
                                     End Try
                                 End Sub)
        _dirty = True
    End Sub

    Public Sub PostPointerUp(x As Integer, y As Integer, button As MouseButton,
                             leftDown As Boolean, rightDown As Boolean, middleDown As Boolean)
        If _disposed Then Return
        Dim mods = BuildMouseModifiers(leftDown, rightDown, middleDown)
        Dispatcher.UIThread.Post(Sub()
                                     Try
                                         _window.MouseUp(New Point(x, y), button, mods)
                                     Catch
                                     End Try
                                 End Sub)
        _dirty = True
    End Sub

    Public Sub PostScroll(x As Integer, y As Integer, deltaY As Integer,
                          leftDown As Boolean, rightDown As Boolean, middleDown As Boolean)
        If _disposed Then Return
        Dim mods = BuildMouseModifiers(leftDown, rightDown, middleDown)
        Dispatcher.UIThread.Post(Sub()
                                     Try
                                         _window.MouseWheel(New Point(x, y), New Vector(0, deltaY), mods)
                                     Catch
                                     End Try
                                 End Sub)
        _dirty = True
    End Sub

    Public Sub PostKeyDown(key As Key, physicalKey As PhysicalKey,
                           ctrl As Boolean, shift As Boolean, alt As Boolean,
                           Optional symbol As String = Nothing)
        If _disposed Then Return
        Dim mods = BuildKeyModifiers(ctrl, shift, alt)
        Dim text = If(symbol, "")
        Dispatcher.UIThread.Post(Sub()
                                     Try
                                         _window.KeyPress(key, mods, physicalKey, text)
                                     Catch
                                     End Try
                                 End Sub)
        _dirty = True
    End Sub

    Public Sub PostKeyUp(key As Key, physicalKey As PhysicalKey,
                         ctrl As Boolean, shift As Boolean, alt As Boolean)
        If _disposed Then Return
        Dim mods = BuildKeyModifiers(ctrl, shift, alt)
        Dispatcher.UIThread.Post(Sub()
                                     Try
                                         _window.KeyRelease(key, mods, physicalKey, "")
                                     Catch
                                     End Try
                                 End Sub)
    End Sub

    Public Sub PostText(text As String)
        If _disposed OrElse String.IsNullOrEmpty(text) Then Return
        Dispatcher.UIThread.Post(Sub()
                                     Try
                                         _window.KeyTextInput(text)
                                     Catch
                                     End Try
                                 End Sub)
        _dirty = True
    End Sub

    Public Sub RequestFocus()
        If _disposed Then Return
        Dispatcher.UIThread.Post(Sub() _window.Focus())
    End Sub

    Private Shared Function BuildMouseModifiers(leftDown As Boolean, rightDown As Boolean, middleDown As Boolean) As RawInputModifiers
        Dim m = RawInputModifiers.None
        If leftDown Then m = m Or RawInputModifiers.LeftMouseButton
        If rightDown Then m = m Or RawInputModifiers.RightMouseButton
        If middleDown Then m = m Or RawInputModifiers.MiddleMouseButton
        Return m
    End Function

    Private Shared Function BuildKeyModifiers(ctrl As Boolean, shift As Boolean, alt As Boolean) As RawInputModifiers
        Dim m = RawInputModifiers.None
        If ctrl Then m = m Or RawInputModifiers.Control
        If shift Then m = m Or RawInputModifiers.Shift
        If alt Then m = m Or RawInputModifiers.Alt
        Return m
    End Function

    ' ---------------------------------------------------------------- teardown

    Friend Sub DetachFromHost()
        ' Runs on the UI thread. Closes the window; the texture is disposed on
        ' the game thread when the owning AppWindow is disposed.
        _disposed = True
        Try
            _window?.Close()
        Catch
        End Try
        _window = Nothing
    End Sub

    Public Sub DisposeTexture()
        _texture?.Dispose()
        _texture = Nothing
    End Sub
End Class