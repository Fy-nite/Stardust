Imports System.Threading
Imports Avalonia
Imports Avalonia.Controls
Imports Avalonia.Headless
Imports Avalonia.Media
Imports Avalonia.Platform
Imports Avalonia.Styling
Imports Avalonia.Threading

''' Owns the Avalonia runtime for the Stardust display server. Runs on its own
''' dedicated background thread: the Avalonia dispatcher is pumped there, and
''' every offscreen window surface is created, sized, rendered and destroyed on
''' that same thread. The MonoGame game thread only exchanges finished pixel
''' buffers with the surfaces.
Public NotInheritable Class AvaloniaHost

    Private Shared ReadOnly _initLock As New Object()
    Private Shared _instance As AvaloniaHost = Nothing

    Private ReadOnly _surfaceLock As New Object()
    Private ReadOnly _surfaces As New List(Of AvaloniaSurface)

    Private _thread As Thread = Nothing
    Private _shutdown As Boolean = False
    Private _ready As New ManualResetEventSlim(False)
    Private _started As Boolean = False

    ' Diagnostic: pump heartbeat + watchdog. _pumpTick advances on every host
    ' loop; a watchdog dumps the UI-thread stack trace to the console if the
    ' pump stops moving, to pin down freezes like the UiScale click hang.
    Private _pumpTick As Long = 0
    Private _phase As String = "idle"

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property Instance As AvaloniaHost
        Get
            SyncLock _initLock
                If _instance Is Nothing Then
                    _instance = New AvaloniaHost()
                    _instance.Start()
                End If
                Return _instance
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property IsRunning As Boolean
        Get
            Return _started AndAlso Not _shutdown
        End Get
    End Property

    Private Sub Start()
        If _started Then Return
        _started = True
        _thread = New Thread(AddressOf ThreadMain)
        _thread.IsBackground = True
        _thread.Name = "AvaloniaHost"
        _thread.SetApartmentState(ApartmentState.STA)
        _thread.Start()
        _ready.Wait()
    End Sub

    Friend Sub Shutdown()
        _shutdown = True
        _ready.Dispose()
    End Sub

    ''' Diagnostic only. If the host pump stops advancing, dump the UI-thread
    ''' stack so a freeze is pinpointed instead of guessed at.
    Private Sub StartWatchdog()
        Dim w As New Thread(Sub()
                                Dim lastTick = _pumpTick
                                While Not _shutdown
                                    Thread.Sleep(2000)
                                    If _shutdown Then Exit While
                                    If _pumpTick = lastTick Then
                                        Console.Error.WriteLine("[AVH watchdog] pump stalled (phase=" & _phase &
                                                                ", shutdown=" & _shutdown &
                                                                ", surfaces=" & _surfaces.Count & ")")
                                    Else
                                        lastTick = _pumpTick
                                    End If
                                End While
                            End Sub)
        w.IsBackground = True
        w.Name = "AvaloniaWatchdog"
        w.Start()
    End Sub

    ' -------------------------------------------------------------- thread main

    Private Sub ThreadMain()
        Try
            AppBuilder.Configure(Of StardustAvaloniaApp)().
                UseSkia().
                UseHeadless(New AvaloniaHeadlessPlatformOptions With {
                    .UseHeadlessDrawing = False,
                    .Fps = 60,
                    .FrameBufferFormat = PixelFormats.Rgba8888}).
                ConfigureFonts(AddressOf ConfigureFonts).
                SetupWithoutStarting()

            ConfigureTheme()
        Catch ex As Exception
            Console.Error.WriteLine($"[AvaloniaHost] setup failed: {ex.Message}")
        End Try
        _ready.Set()

        StartWatchdog()

        While Not _shutdown
            _phase = "RunJobs"
            Try
                Dispatcher.UIThread.RunJobs()
            Catch
            End Try

            If Not _shutdown Then
                _phase = "PumpSurfaces"
                PumpSurfaces()
                _phase = "sleep"
                Thread.Sleep(2)
            End If
            _pumpTick += 1
        End While

        SyncLock _surfaceLock
            For Each s In _surfaces.ToArray()
                s.DetachFromHost()
            Next
            _surfaces.Clear()
        End SyncLock
    End Sub

    Private Sub ConfigureFonts(fontManager As Avalonia.Media.FontManager)
        Try
            Dim bytes = DisplayServer.LoadEmbeddedResource("FiraCode-Regular.ttf")
            If bytes IsNot Nothing Then
                Dim collection As New StardustFontCollection()
                If collection.AddFontFromBytes(bytes) Then
                    fontManager.AddFontCollection(collection)
                End If
            End If
        Catch ex As Exception
            Console.Error.WriteLine($"[AvaloniaHost] font setup failed: {ex.Message}")
        End Try
    End Sub

    Private Sub ConfigureTheme()
        Dim fluent = New Avalonia.Themes.Fluent.FluentTheme()

        Dim dark = New Avalonia.Themes.Fluent.ColorPaletteResources()
        dark.RegionColor = Color.Parse("#211E1A")
        dark.Accent = Color.Parse("#C89B5A")
        dark.BaseLow = Color.Parse("#EEDCE6") ' ink
        dark.BaseMediumLow = Color.Parse("#9C9284") ' ink muted
        dark.BaseMedium = Color.Parse("#6A6258") ' ink faint
        dark.BaseMediumHigh = Color.Parse("#3B362E") ' hairline
        dark.BaseHigh = Color.Parse("#2F2B25") ' hairline faint
        dark.AltHigh = Color.Parse("#2D2923") ' surface raised
        dark.AltMediumHigh = Color.Parse("#363129") ' surface hover
        dark.AltMedium = Color.Parse("#1D1A16") ' surface pressed
        dark.AltLow = Color.Parse("#171512") ' input well
        dark.ChromeMediumLow = Color.Parse("#211E1A")
        dark.ChromeLow = Color.Parse("#272420")
        dark.ChromeMedium = Color.Parse("#1D1A16")
        dark.ListLow = Color.Parse("#2D2923")
        dark.ListMedium = Color.Parse("#363129")
        dark.ErrorText = Color.Parse("#B95B4F")

        fluent.Palettes(Avalonia.Styling.ThemeVariant.Dark) = dark

        Application.Current.Styles.Add(fluent)
        Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark
    End Sub

    ' ------------------------------------------------------------- surfaces

    ''' Create an offscreen Avalonia surface for one window, synchronously, on
    ''' the Avalonia thread. Safe to call from the MonoGame (game) thread.
    Friend Function CreateSurface(widthPx As Integer, heightPx As Integer) As AvaloniaSurface
        If Not IsRunning Then Return Nothing
        Dim result As AvaloniaSurface = Nothing
        Dim done As New ManualResetEventSlim(False)
        Try
            Dispatcher.UIThread.Post(Sub()
                                         Try
                                             result = New AvaloniaSurface(widthPx, heightPx)
                                         Catch ex As Exception
                                             Console.Error.WriteLine(
                                                 $"[AvaloniaHost] surface create failed: {ex.Message}")
                                         End Try
                                         done.Set()
                                     End Sub)
            done.Wait()
        Catch
        Finally
            done.Dispose()
        End Try

        If result IsNot Nothing Then
            SyncLock _surfaceLock
                _surfaces.Add(result)
            End SyncLock
        End If
        Return result
    End Function

    ''' Run on the UI thread every host loop iteration: renders whatever surfaces
    ''' were invalidated and published a new pixel buffer.
    Private Sub PumpSurfaces()
        Dim anyDirty = False
        SyncLock _surfaceLock
            For Each s In _surfaces
                If s.IsDirty Then anyDirty = True
            Next
        End SyncLock

        If Not anyDirty Then Return

        AvaloniaHeadlessPlatform.ForceRenderTimerTick(1)

        SyncLock _surfaceLock
            For Each s In _surfaces.ToArray()
                If s.IsDirty Then s.RenderFrame()
            Next
        End SyncLock
    End Sub

    Friend Sub RemoveSurface(surface As AvaloniaSurface)
        SyncLock _surfaceLock
            _surfaces.Remove(surface)
        End SyncLock
    End Sub

    ''' Game thread. Detach and close a surface on the Avalonia thread. Returns
    ''' False when the host is not running (e.g. teardown), in which case the
    ''' caller keeps the reference so nothing is touched after host death.
    Friend Function TryDisposeSurface(surface As AvaloniaSurface) As Boolean
        If surface Is Nothing Then Return False
        If Not IsRunning Then Return False
        RemoveSurface(surface)
        Try
            Dispatcher.UIThread.Post(Sub() surface.DetachFromHost())
        Catch
            Return False
        End Try
        Return True
    End Function
End Class

''' The Avalonia Application subclass used by the embedded runtime. Minimal: all
''' real setup happens in AvaloniaHost.ConfigureTheme.
Public Class StardustAvaloniaApp
    Inherits Application

    Public Sub New()
        Name = "Stardust"
    End Sub
End Class

''' Registers the embedded Fira Code TTF as an Avalonia font collection under
''' the "fonts:stardust" key, so controls can opt in with
''' FontFamily = "fonts:stardust#Fira Code".
Friend NotInheritable Class StardustFontCollection
    Inherits Avalonia.Media.Fonts.FontCollectionBase

    Public Overrides ReadOnly Property Key As Uri
        Get
            Return New Uri("fonts:stardust")
        End Get
    End Property

    Public Function AddFontFromBytes(bytes As Byte()) As Boolean
        Using stream = New System.IO.MemoryStream(bytes)
            Dim glyph As Avalonia.Media.GlyphTypeface = Nothing
            Return TryAddGlyphTypeface(stream, glyph)
        End Using
    End Function
End Class