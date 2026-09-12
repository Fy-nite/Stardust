Imports Microsoft.Xna.Framework
Imports Stardust.Display

''' <summary>
''' A window handle for Java apps. Wraps a real AppWindow; the Java facade
''' operates on this object through its methods instead of reaching into the
''' Stardust.Display assembly directly.
''' </summary>
Public Class JavaWindow

    Private ReadOnly _win As AppWindow

    Friend Sub New(win As AppWindow)
        _win = win
    End Sub

    Friend ReadOnly Property Native As AppWindow
        Get
            Return _win
        End Get
    End Property

    Public Sub SetTitle(title As String)
        _win.Title = title
    End Sub

    Public Function GetTitle() As String
        Return _win.Title
    End Function

    Public Sub SetSize(width As Integer, height As Integer)
        _win.Width = width
        _win.Height = height
    End Sub

    Public Function GetClientWidth() As Integer
        Return _win.ClientWidth
    End Function

    Public Function GetClientHeight() As Integer
        Return _win.ClientHeight
    End Function

    Public Sub SetRootWidget(widget As Widget)
        _win.RootWidget = widget
    End Sub

    Public Sub Show()
        DisplayServer.Instance.AddWindowDirect(_win)
    End Sub

    Public Sub Block()
        _win.WaitForClose()
    End Sub

    Public Sub Close()
        DisplayServer.Instance.RemoveWindow(_win)
    End Sub

    Public Sub Focus()
        DisplayServer.Instance.FocusWindow(_win)
    End Sub

    Public Function IsClosed() As Boolean
        Return Not _win.IsOpen
    End Function

    ''' <summary>
    ''' Fills the entire client area with one colour. Buffered via SetPixels so
    ''' it is safe to call from the Java app thread.
    ''' </summary>
    Public Sub Fill(argb As Integer)
        Dim w = _win.ClientWidth
        Dim h = _win.ClientHeight
        Dim pixels(w * h - 1) As Color
        Dim c = JavaUi.ColorFromArgb(argb)
        For i = 0 To pixels.Length - 1
            pixels(i) = c
        Next
        _win.SetPixels(pixels, w, h)
    End Sub

    ''' <summary>
    ''' Sets a single client-area pixel. Like SetPixelDirect this touches the
    ''' live texture directly, so it is only safe from a handler running on the
    ''' display thread.
    ''' </summary>
    Public Sub SetPixel(x As Integer, y As Integer, argb As Integer)
        _win.SetPixelDirect(x, y, JavaUi.ColorFromArgb(argb))
    End Sub

End Class