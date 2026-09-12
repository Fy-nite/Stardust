Imports Avalonia
Imports Avalonia.Controls
Imports Avalonia.Layout
Imports Avalonia.Media
Imports AvaloniaButton = Avalonia.Controls.Button
Imports AvaloniaThickness = Avalonia.Thickness

''' Built-in desktop Settings: a live UI-scale control for the whole desktop,
''' windows, chrome and their contents. Built with Avalonia controls and rendered
''' by the offscreen Avalonia bridge - the first app on the new UI path.
Public Class SettingsApp
    Inherits GuiProcessNode

    Private Shared ReadOnly Presets() As Single = {0.75F, 1.0F, 1.25F, 1.5F, 2.0F}

    Private Shared _valueLabel As TextBlock

    Public Sub New()
        MyBase.New("Settings", 520, 250)
    End Sub

    Public Overrides Sub init()
    End Sub

    Public Overrides Sub BuildUI()
        SetRootVisual(AddressOf CreateUi)
    End Sub

    ''' Build the settings control tree. Runs on the Avalonia UI thread (it is
    ''' invoked as the window's root factory), so every control is born where it
    ''' belongs. Shared so non-process hosts can open the same window.
    Public Shared Function CreateUi() As Control
Dim root As New StackPanel With {
            .Margin = New AvaloniaThickness(16),
            .Spacing = 8,
            .Background = New SolidColorBrush(AvaloniaColor(Theme.WindowSurface))
        }

        Dim heading As New TextBlock With {
            .Text = "Display Scale",
            .FontSize = 18,
            .FontWeight = FontWeight.SemiBold,
            .Foreground = New SolidColorBrush(AvaloniaColor(Theme.Accent))
        }
        root.Children.Add(heading)

        Dim subLbl As New TextBlock With {
            .Text = "Scales the whole desktop live: windows, chrome and everything in them.",
            .TextWrapping = TextWrapping.Wrap,
            .Foreground = New SolidColorBrush(AvaloniaColor(Theme.InkMuted))
        }
        root.Children.Add(subLbl)

        _valueLabel = New TextBlock With {
            .Text = CurrentScaleText(),
            .Foreground = New SolidColorBrush(AvaloniaColor(Theme.Ink))
        }
        root.Children.Add(_valueLabel)

        Dim row As New StackPanel With {
            .Orientation = Orientation.Horizontal,
            .Spacing = 8
        }
        For Each p In Presets
            Dim scale = p
            Dim btn = MakeButton(PercentLabel(scale))
            AddHandler btn.Click, Sub() ApplyScale(scale)
            row.Children.Add(btn)
        Next
        root.Children.Add(row)

        Dim hint As New TextBlock With {
            .Text = "Every window and its contents scale on click.",
            .Foreground = New SolidColorBrush(AvaloniaColor(Theme.InkFaint))
        }
        root.Children.Add(hint)

        Return root
    End Function

Private Shared Sub ApplyScale(scale As Single)
        If _valueLabel IsNot Nothing Then
            _valueLabel.Text = String.Format("Current: {0}%", CInt(scale * 100))
        End If
        ' Button clicks arrive on the Avalonia UI thread; the MonoGame side must
        ' only be touched on the game thread.
        DisplayServer.Instance.InvokeOnGameThread(Sub() DisplayServer.Instance.SetUiScale(scale))
    End Sub

    Private Shared Function MakeButton(text As String) As AvaloniaButton
        Return New AvaloniaButton With {
            .Content = text,
            .Background = New SolidColorBrush(AvaloniaColor(Theme.SurfaceRaised)),
            .Foreground = New SolidColorBrush(AvaloniaColor(Theme.Ink)),
            .BorderBrush = New SolidColorBrush(AvaloniaColor(Theme.Hairline)),
            .BorderThickness = New AvaloniaThickness(1),
            .CornerRadius = New CornerRadius(3),
            .Padding = New AvaloniaThickness(14, 5),
            .FontSize = 13
        }
    End Function

    Private Shared Function CurrentScaleText() As String
        Return String.Format("Current: {0}%", CInt(Theme.UiScale * 100))
    End Function

    Private Shared Function PercentLabel(scale As Single) As String
        Return String.Format("{0}%", CInt(scale * 100))
    End Function

    Private Shared Function AvaloniaColor(c As Microsoft.Xna.Framework.Color) As Color
        Return Color.FromArgb(c.A, c.R, c.G, c.B)
    End Function
End Class
