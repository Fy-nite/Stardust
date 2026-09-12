Imports Microsoft.Xna.Framework

''' The single source of truth for the Stardust desktop look: a warm graphite
''' palette with one brass accent. Static so widgets built before the graphics
''' device exists can rely on it.
Public NotInheritable Class Theme

    ' Desktop backdrop.
    Public Shared ReadOnly DesktopTop As Color = New Color(32, 29, 25)
    Public Shared ReadOnly DesktopBottom As Color = New Color(18, 16, 14)

    ' Windows and surfaces.
    Public Shared ReadOnly WindowSurface As Color = New Color(33, 30, 26)
    Public Shared ReadOnly SurfaceRaised As Color = New Color(45, 41, 36)
    Public Shared ReadOnly SurfaceHover As Color = New Color(54, 49, 43)
    Public Shared ReadOnly SurfacePressed As Color = New Color(29, 26, 22)
    Public Shared ReadOnly SurfaceDisabled As Color = New Color(40, 37, 33)
    Public Shared ReadOnly InputWell As Color = New Color(23, 21, 18)
    Public Shared ReadOnly TerminalWell As Color = New Color(24, 22, 19)

    ' Title bar (active = gradient, inactive = flat).
    Public Shared ReadOnly TitlebarActiveTop As Color = New Color(47, 42, 36)
    Public Shared ReadOnly TitlebarActiveBottom As Color = New Color(33, 29, 25)
    Public Shared ReadOnly TitlebarInactive As Color = New Color(38, 34, 30)
    Public Shared ReadOnly LipActive As Color = New Color(91, 82, 68)
    Public Shared ReadOnly LipInactive As Color = New Color(58, 53, 45)

    ' Ink hierarchy.
    Public Shared ReadOnly Ink As Color = New Color(238, 231, 220)
    Public Shared ReadOnly InkMuted As Color = New Color(156, 146, 132)
    Public Shared ReadOnly InkFaint As Color = New Color(106, 98, 88)

    ' Lines.
    Public Shared ReadOnly Hairline As Color = New Color(59, 54, 46)
    Public Shared ReadOnly HairlineFaint As Color = New Color(47, 43, 37)

    ' The one accent, used tonally (never as a glow).
    Public Shared ReadOnly Accent As Color = New Color(200, 155, 90)
    Public Shared ReadOnly AccentHover As Color = New Color(216, 171, 111)
    Public Shared ReadOnly AccentPressed As Color = New Color(168, 127, 68)

    ' Close caption button (desaturated, not neon).
    Public Shared ReadOnly Close As Color = New Color(185, 91, 79)
    Public Shared ReadOnly CloseHover As Color = New Color(203, 107, 94)
    Public Shared ReadOnly ClosePressed As Color = New Color(160, 74, 64)

    ' Geometry (base values at 100% scale; live-scaled by UiScale).
    Public Const WindowRadius As Integer = 6
    Public Const ControlRadius As Integer = 3
    Public Const BaseTitleBarHeight As Integer = 28
    Private Const BaseCaptionButtonSize As Integer = 22
    Private Const BaseCaptionGap As Integer = 3
    Private Const BaseCaptionRightPad As Integer = 5
    Private Const BaseTaskbarHeight As Integer = 36
    Private Const BaseTitleLeftX As Integer = 38
    Private Const BaseTitleIconX As Integer = 8
    Private Const BaseTitleIconSize As Integer = 14
    Private Const BaseTitleMargin As Integer = 8
    Private Const BaseCaptionIconSize As Integer = 12
    Private Const BaseTitleFontPx As Integer = 13
    Private Const BaseTaskFontPx As Integer = 12

    Public Const MinUiScale As Single = 0.75F
    Public Const MaxUiScale As Single = 2.0F

    ''' Live UI scale applied to window chrome, taskbar, caption buttons and chrome fonts.
    Public Shared Property UiScale As Single = 1.0F

    Public Shared Function SetUiScale(value As Single) As Single
        UiScale = MathHelper.Clamp(value, MinUiScale, MaxUiScale)
        Return UiScale
    End Function

    Public Shared ReadOnly Property TitleBarHeight As Integer
        Get
            Return CInt((BaseTitleBarHeight * UiScale) + 0.5F)
        End Get
    End Property

    Public Shared ReadOnly Property CaptionButtonSize As Integer
        Get
            Return CInt((BaseCaptionButtonSize * UiScale) + 0.5F)
        End Get
    End Property

    Public Shared ReadOnly Property CaptionGap As Integer
        Get
            Return Math.Max(2, CInt(BaseCaptionGap * UiScale))
        End Get
    End Property

    Public Shared ReadOnly Property CaptionRightPad As Integer
        Get
            Return CInt(BaseCaptionRightPad * UiScale)
        End Get
    End Property

    ''' Caption buttons are centred in the title bar.
    Public Shared ReadOnly Property CaptionButtonTop As Integer
        Get
            Return CInt((TitleBarHeight - CaptionButtonSize) / 2)
        End Get
    End Property

    Public Shared ReadOnly Property TaskbarHeight As Integer
        Get
            Return CInt((BaseTaskbarHeight * UiScale) + 0.5F)
        End Get
    End Property

    Public Shared ReadOnly Property TitleLeftX As Integer
        Get
            Return CInt(BaseTitleLeftX * UiScale)
        End Get
    End Property

    Public Shared ReadOnly Property TitleIconX As Integer
        Get
            Return CInt(BaseTitleIconX * UiScale)
        End Get
    End Property

    Public Shared ReadOnly Property TitleIconSize As Integer
        Get
            Return CInt(BaseTitleIconSize * UiScale)
        End Get
    End Property

    Public Shared ReadOnly Property TitleMargin As Integer
        Get
            Return CInt(BaseTitleMargin * UiScale)
        End Get
    End Property

    Public Shared ReadOnly Property TitleIconTop As Integer
        Get
            Return CInt((TitleBarHeight - TitleIconSize) / 2)
        End Get
    End Property

    Public Shared ReadOnly Property CaptionIconSize As Integer
        Get
            Return CInt((BaseCaptionIconSize * UiScale) + 0.5F)
        End Get
    End Property

    Public Shared ReadOnly Property TitleFontPx As Integer
        Get
            Return Math.Max(8, CInt(BaseTitleFontPx * UiScale))
        End Get
    End Property

    Public Shared ReadOnly Property TaskFontPx As Integer
        Get
            Return Math.Max(7, CInt(BaseTaskFontPx * UiScale))
        End Get
    End Property

    ''' Straight-alpha blend, matching MonoGame's default SpriteBatch blend.
    Public Shared Function WithAlpha(color As Color, alpha As Integer) As Color
        Return New Color(color.R, color.G, color.B, alpha)
    End Function

    Public Shared Function Lerp(a As Color, b As Color, t As Single) As Color
        Dim k = MathHelper.Clamp(t, 0.0F, 1.0F)
        ' Promote to Integer first: VB computes Byte - Byte as Byte (underflow hazard).
        Dim ar As Single = CInt(a.R) + (CInt(b.R) - CInt(a.R)) * k
        Dim ag As Single = CInt(a.G) + (CInt(b.G) - CInt(a.G)) * k
        Dim ab As Single = CInt(a.B) + (CInt(b.B) - CInt(a.B)) * k
        Dim aa As Single = CInt(a.A) + (CInt(b.A) - CInt(a.A)) * k
        Return New Color(
            CByte(Math.Min(255, Math.Max(0, CInt(ar)))),
            CByte(Math.Min(255, Math.Max(0, CInt(ag)))),
            CByte(Math.Min(255, Math.Max(0, CInt(ab)))),
            CByte(Math.Min(255, Math.Max(0, CInt(aa)))))
    End Function
End Class