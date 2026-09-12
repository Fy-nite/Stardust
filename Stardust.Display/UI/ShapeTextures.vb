Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

''' Prerendered, color-agnostic shapes for the display server: soft shadows,
''' rounded-corner fills (9-sliced so the corner radius stays fixed at any size),
''' gradient surfaces, and the baked window title-bar chrome. Everything is built
''' once against the graphics device and drawn with a straight-alpha SpriteBatch.
Public Class ShapeTextures

    Private _graphics As GraphicsDevice

    ' White-on-alpha rounded fills. {0,1}_{0,1} = which corners are rounded.
    Private _roundedAll As Texture2D
    Private _roundedBottom As Texture2D
    Private _roundedTop As Texture2D

    ' Control-radius (3px) rounded fill and 1px ring outline for widgets.
    Private _roundedControl As Texture2D
    Private _ringControl As Texture2D

    ' Baked vertical gradients (window chrome, desktop).
    Private _desktopGradient As Texture2D
    Private _chromeActive As Texture2D
    Private _chromeInactive As Texture2D

    ' Soft drop shadow (black, blurred).
    Private _shadow As Texture2D

    Public Sub New(graphics As GraphicsDevice)
        _graphics = graphics
        _roundedAll = BuildRoundedSlice(Theme.WindowRadius, True, True)
        _roundedBottom = BuildRoundedSlice(Theme.WindowRadius, False, True)
        _roundedTop = BuildRoundedSlice(Theme.WindowRadius, True, False)
        _roundedControl = BuildRoundedSlice(Theme.ControlRadius, True, True)
        _ringControl = BuildRoundedRing(Theme.ControlRadius)
        _desktopGradient = BuildGradient(128, Theme.DesktopTop, Theme.DesktopBottom)
        _chromeActive = BuildChrome(Theme.WindowRadius, Theme.TitleBarHeight,
                                    Theme.TitlebarActiveTop, Theme.TitlebarActiveBottom, Theme.LipActive)
        _chromeInactive = BuildChrome(Theme.WindowRadius, Theme.TitleBarHeight,
                                      Theme.TitlebarInactive, Theme.TitlebarInactive, Theme.LipInactive)
        _shadow = BuildShadow(128, Theme.WindowRadius * 2, 5)
    End Sub

    ''' Rebuild the size-dependent title-bar chrome after a UI-scale change.
    ''' Corner radius does not scale; title-bar height does.
    Public Sub ApplyScale()
        If _chromeActive IsNot Nothing Then _chromeActive.Dispose()
        If _chromeInactive IsNot Nothing Then _chromeInactive.Dispose()
        _chromeActive = BuildChrome(Theme.WindowRadius, Theme.TitleBarHeight,
                                    Theme.TitlebarActiveTop, Theme.TitlebarActiveBottom, Theme.LipActive)
        _chromeInactive = BuildChrome(Theme.WindowRadius, Theme.TitleBarHeight,
                                      Theme.TitlebarInactive, Theme.TitlebarInactive, Theme.LipInactive)
    End Sub

    ' ---------------------------------------------------------------- drawing

    Public Sub DrawDesktop(batch As SpriteBatch, viewport As Rectangle)
        batch.Draw(_desktopGradient, viewport, Color.White)
    End Sub

    Public Sub DrawWindowBase(batch As SpriteBatch, rect As Rectangle, active As Boolean, shadowAlpha As Single)
        Dim shadowRect = New Rectangle(rect.X - 10, rect.Y + 6, rect.Width + 20, rect.Height + 14)
        batch.Draw(_shadow, shadowRect, Color.Black * shadowAlpha)
        DrawRounded(batch, rect, Theme.Hairline)
    End Sub

    Public Sub DrawTitleBar(batch As SpriteBatch, rect As Rectangle, active As Boolean)
        DrawHSlice(batch, If(active, _chromeActive, _chromeInactive), rect)
    End Sub

    Public Sub DrawBody(batch As SpriteBatch, rect As Rectangle, color As Color)
        Draw9Slice(batch, _roundedBottom, rect, Color.White, color)
    End Sub

    Public Sub DrawRounded(batch As SpriteBatch, rect As Rectangle, color As Color)
        Draw9Slice(batch, _roundedAll, rect, Color.White, color)
    End Sub

    Public Sub DrawRoundedTop(batch As SpriteBatch, rect As Rectangle, color As Color)
        Draw9Slice(batch, _roundedTop, rect, Color.White, color)
    End Sub

    Public Sub DrawRoundedBottom(batch As SpriteBatch, rect As Rectangle, color As Color)
        Draw9Slice(batch, _roundedBottom, rect, Color.White, color)
    End Sub

    ''' Widget-sized rounded fill (small radius).
    Public Sub DrawRoundedControl(batch As SpriteBatch, rect As Rectangle, color As Color)
        Draw9Slice(batch, _roundedControl, rect, Color.White, color)
    End Sub

    ''' Widget-sized 1px rounded outline.
    Public Sub DrawRoundedOutlineControl(batch As SpriteBatch, rect As Rectangle, color As Color)
        Draw9Slice(batch, _ringControl, rect, Color.White, color)
    End Sub

    ' -------------------------------------------------- 9-slice machinery

    Private Sub DrawHSlice(batch As SpriteBatch, tex As Texture2D, dest As Rectangle)
        Dim r = (tex.Width - 1) \ 2
        Dim midW = Math.Max(0, dest.Width - r * 2)
        batch.Draw(tex, New Rectangle(dest.X, dest.Y, r, dest.Height),
                   New Rectangle(0, 0, r, dest.Height), Color.White)
        batch.Draw(tex, New Rectangle(dest.X + r, dest.Y, midW, dest.Height),
                   New Rectangle(r, 0, 1, dest.Height), Color.White)
        batch.Draw(tex, New Rectangle(dest.X + dest.Width - r, dest.Y, r, dest.Height),
                   New Rectangle(r + 1, 0, r, dest.Height), Color.White)
    End Sub

    Private Sub Draw9Slice(batch As SpriteBatch, tex As Texture2D, dest As Rectangle, srcColor As Color, tint As Color)
        ' Corner radius is the texture's own (2r+1-wide slice), never the window
        ' radius: _roundedControl/_ringControl are 7px (radius 3) textures, the
        ' chrome slices are 13px (radius 6). Slicing with the wrong radius reads
        ' center pixels from a transparent corner and hollows out the fill.
        Dim r = (tex.Width - 1) \ 2
        If dest.Width <= 0 OrElse dest.Height <= 0 Then Return

        ' Source blocks for a (2r+1) x (2r+1) slice: columns/rows are corner|edge|corner.
        Dim srcW = tex.Width
        Dim srcH = tex.Height

        Dim midW = dest.Width - r * 2
        Dim midH = dest.Height - r * 2
        If midW < 0 OrElse midH < 0 Then
            ' Too small to slice; stretch the single clear centre pixel.
            batch.Draw(tex, dest, New Rectangle(r, r, 1, 1), tint)
            Return
        End If

        Dim destMidW = midW
        Dim destMidH = midH

        ' Corners (fixed size, crisp).
        batch.Draw(tex, New Rectangle(dest.X, dest.Y, r, r), New Rectangle(0, 0, r, r), tint)
        batch.Draw(tex, New Rectangle(dest.X + dest.Width - r, dest.Y, r, r), New Rectangle(srcW - r, 0, r, r), tint)
        batch.Draw(tex, New Rectangle(dest.X, dest.Y + dest.Height - r, r, r), New Rectangle(0, srcH - r, r, r), tint)
        batch.Draw(tex, New Rectangle(dest.X + dest.Width - r, dest.Y + dest.Height - r, r, r),
                   New Rectangle(srcW - r, srcH - r, r, r), tint)

        ' Edges (stretched).
        batch.Draw(tex, New Rectangle(dest.X + r, dest.Y, destMidW, r), New Rectangle(r, 0, 1, r), tint)
        batch.Draw(tex, New Rectangle(dest.X + r, dest.Y + dest.Height - r, destMidW, r),
                   New Rectangle(r, srcH - r, 1, r), tint)
        batch.Draw(tex, New Rectangle(dest.X, dest.Y + r, r, destMidH), New Rectangle(0, r, r, 1), tint)
        batch.Draw(tex, New Rectangle(dest.X + dest.Width - r, dest.Y + r, r, destMidH),
                   New Rectangle(srcW - r, r, r, 1), tint)

        ' Centre.
        batch.Draw(tex, New Rectangle(dest.X + r, dest.Y + r, destMidW, destMidH), New Rectangle(r, r, 1, 1), tint)
    End Sub

    ' --------------------------------------------------------- bake helpers

    Private Function BuildRoundedSlice(r As Integer, roundedTop As Boolean, roundedBottom As Boolean) As Texture2D
        Dim size = r * 2 + 1
        Dim data(size * size - 1) As Color
        Dim half As Single = 0.5F
        For y = 0 To size - 1
            For x = 0 To size - 1
                Dim px = x + half
                Dim py = y + half
                Dim inShape As Boolean = True

                If roundedTop AndAlso IsCornerCut(px, py, r, True) Then inShape = False
                If roundedBottom AndAlso IsCornerCut(px, py, r, False) Then inShape = False

                data(y * size + x) = If(inShape, Color.White, Color.Transparent)
            Next
        Next
        Return MakeTexture(data, size, size)
    End Function

    Private Shared Function IsCornerCut(px As Single, py As Single, r As Integer, top As Boolean) As Boolean
        If px < r AndAlso ((top AndAlso py < r) OrElse (Not top AndAlso py > r)) Then
            Dim cy = If(top, CSng(r), CSng(r) + 1.0F)
            Dim dx = px - r
            Dim dy = py - cy
            Return dx * dx + dy * dy > r * r
        End If
        If px > r AndAlso ((top AndAlso py < r) OrElse (Not top AndAlso py > r)) Then
            Dim cy = If(top, CSng(r), CSng(r) + 1.0F)
            Dim dx = px - (r + 1)
            Dim dy = py - cy
            Return dx * dx + dy * dy > r * r
        End If
        Return False
    End Function

    ''' 1px outline ring of a rounded rectangle: white where the shape exists but
    ''' its 1px-inset interior does not. Same corner centers, inner radius r-1.
    Private Function BuildRoundedRing(r As Integer) As Texture2D
        Dim size = r * 2 + 1
        Dim data(size * size - 1) As Color
        Dim half As Single = 0.5F
        For y = 0 To size - 1
            For x = 0 To size - 1
                Dim px = x + half
                Dim py = y + half

                Dim inOuter = Not (IsCornerCut(px, py, r, True) OrElse IsCornerCut(px, py, r, False))
                Dim inHole As Boolean = False
                If px >= 1 AndAlso px <= size - 2 AndAlso py >= 1 AndAlso py <= size - 2 Then
                    Dim innerR = Math.Max(1, r - 1)
                    inHole = Not (IsCornerCut(px, py, innerR, True) OrElse IsCornerCut(px, py, innerR, False))
                End If

                data(y * size + x) = If(inOuter AndAlso Not inHole, Color.White, Color.Transparent)
            Next
        Next
        Return MakeTexture(data, size, size)
    End Function

    Private Function BuildChrome(r As Integer, height As Integer, topColor As Color, bottomColor As Color, lipColor As Color) As Texture2D
        Dim w = r * 2 + 1
        Dim data(w * height - 1) As Color
        Dim half As Single = 0.5F
        For y = 0 To height - 1
            Dim t = y / CSng(Math.Max(1, height - 1))
            Dim base = Theme.Lerp(topColor, bottomColor, t)
            Dim rowColor = If(y = 0, lipColor, base)
            For x = 0 To w - 1
                Dim a As Integer = 255
                If IsCornerCut(x + half, y + half, r, True) Then a = 0
                data(y * w + x) = New Color(rowColor.R, rowColor.G, rowColor.B, a)
            Next
        Next
        Return MakeTexture(data, w, height)
    End Function

    Private Function BuildGradient(height As Integer, topColor As Color, bottomColor As Color) As Texture2D
        Dim data(height - 1) As Color
        For y = 0 To height - 1
            Dim t = y / CSng(Math.Max(1, height - 1))
            data(y) = Theme.Lerp(topColor, bottomColor, t)
        Next
        Return MakeTexture(data, 1, height)
    End Function

    Private Function BuildShadow(size As Integer, r As Integer, blurPasses As Integer) As Texture2D
        Dim data(size * size - 1) As Color
        For y = 0 To size - 1
            For x = 0 To size - 1
                data(y * size + x) = Color.White
            Next
        Next

        ' Rounded-rect mask, then blur to a soft falloff.
        Dim core = r * 3
        For y = 0 To size - 1
            For x = 0 To size - 1
                Dim inside As Boolean
                Dim left = (size - core) \ 2
                Dim top = (size - core) \ 2
                inside = x >= left AndAlso x < left + core AndAlso y >= top AndAlso y < top + core
                data(y * size + x) = If(inside, Color.White, Color.Transparent)
            Next
        Next

        For p = 1 To blurPasses
            data = BoxBlur(data, size, size, 1)
        Next

        ' Soften the falloff curve so it fades out gently rather than in a band.
        For i = 0 To data.Length - 1
            If data(i).A > 0 Then
                Dim a = data(i).A / 255.0F
                a = a * a * (3 - 2 * a)
                data(i) = Color.Black * a
            End If
        Next
        Return MakeTexture(data, size, size)
    End Function

    Private Shared Function BoxBlur(data As Color(), w As Integer, h As Integer, radius As Integer) As Color()
        Dim tmp(data.Length - 1) As Color
        Dim out(data.Length - 1) As Color
        HorizontalBlur(data, tmp, w, h, radius)
        VerticalBlur(tmp, out, w, h, radius)
        Return out
    End Function

    Private Shared Sub HorizontalBlur(src As Color(), dst As Color(), w As Integer, h As Integer, r As Integer)
        For y = 0 To h - 1
            For x = 0 To w - 1
                Dim sumA As Integer = 0
                Dim n As Integer = 0
                For k = -r To r
                    Dim xx = x + k
                    If xx >= 0 AndAlso xx < w Then
                        sumA += src(y * w + xx).A
                        n += 1
                    End If
                Next
                dst(y * w + x) = If(n > 0, Color.White * (sumA / n / 255.0F), Color.Transparent)
            Next
        Next
    End Sub

    Private Shared Sub VerticalBlur(src As Color(), dst As Color(), w As Integer, h As Integer, r As Integer)
        For y = 0 To h - 1
            For x = 0 To w - 1
                Dim sumA As Integer = 0
                Dim n As Integer = 0
                For k = -r To r
                    Dim yy = y + k
                    If yy >= 0 AndAlso yy < h Then
                        sumA += src(yy * w + x).A
                        n += 1
                    End If
                Next
                dst(y * w + x) = If(n > 0, Color.White * (sumA / n / 255.0F), Color.Transparent)
            Next
        Next
    End Sub

    Private Function MakeTexture(data As Color(), w As Integer, h As Integer) As Texture2D
        Dim tex As New Texture2D(_graphics, w, h)
        tex.SetData(data)
        Return tex
    End Function

End Class