Imports Microsoft.Xna.Framework
Imports Stardust.Display

''' <summary>
''' Static entry point for the Java UI facade. Everything a Java app can do
''' with the windowing system goes through here (or through the widget
''' adapter classes), so the Java side never touches Stardust.Display or
''' MonoGame types directly.
''' </summary>
Public Module JavaUi

        ' ---------------- colour helpers ----------------

        ''' <summary>
        ''' Turns an ARGB int (0xAARRGGBB) into a MonoGame Color.
        ''' </summary>
        Public Function ColorFromArgb(argb As Integer) As Color
            Dim a = CByte((argb >> 24) And &HFF)
            Dim r = CByte((argb >> 16) And &HFF)
            Dim g = CByte((argb >> 8) And &HFF)
            Dim b = CByte(argb And &HFF)
            Return New Color(r, g, b, a)
        End Function

        ' ---------------- window ----------------

        Public Function CreateWindow(title As String, width As Integer, height As Integer) As JavaWindow
            Dim win = DisplayServer.Instance.CreateWindow(title, width, height)
            Return New JavaWindow(win)
        End Function

        ' ---------------- widget factories ----------------

        Public Function NewWidget() As JavaWidget
            Return New JavaWidget()
        End Function

        Public Function NewLabel(text As String) As Label
            Return New Label(text)
        End Function

        Public Function NewPanel() As Panel
            Return New Panel()
        End Function

        Public Function NewButton(text As String) As JavaButton
            Return New JavaButton(text)
        End Function

        Public Function NewTextBox() As JavaTextBox
            Return New JavaTextBox()
        End Function

        ' ---------------- generic widget styling ----------------

        Public Sub WidgetSetBackground(widget As Widget, argb As Integer)
            widget.BackgroundColor = ColorFromArgb(argb)
        End Sub

        Public Sub WidgetSetBorder(widget As Widget, argb As Integer)
            widget.BorderColor = ColorFromArgb(argb)
        End Sub

        Public Sub WidgetSetForeground(widget As Widget, argb As Integer)
            widget.ForegroundColor = ColorFromArgb(argb)
        End Sub

        ' ---------------- label ----------------

        Public Sub LabelSetTextColor(label As Widget, argb As Integer)
            DirectCast(label, Label).TextColor = ColorFromArgb(argb)
        End Sub

        Public Sub LabelSetAlignment(label As Widget, mode As Integer)
            DirectCast(label, Label).Alignment = CType(mode, Label.TextAlignment)
        End Sub

        ' ---------------- panel ----------------

        Public Sub PanelSetLayout(panel As Widget, mode As Integer)
            DirectCast(panel, Panel).LayoutDirection = CType(mode, Panel.LayoutMode)
        End Sub

        Public Sub PanelSetSpacing(panel As Widget, spacing As Integer)
            DirectCast(panel, Panel).ItemSpacing = spacing
        End Sub

        ' ---------------- button ----------------

        Public Sub ButtonSetTextColor(button As Widget, argb As Integer)
            DirectCast(button, Button).TextColor = ColorFromArgb(argb)
        End Sub

        Public Sub ButtonSetButtonColor(button As Widget, argb As Integer)
            DirectCast(button, Button).ButtonColor = ColorFromArgb(argb)
        End Sub

        Public Sub ButtonSetHoverColor(button As Widget, argb As Integer)
            DirectCast(button, Button).HoverColor = ColorFromArgb(argb)
        End Sub

        Public Sub ButtonSetPressedColor(button As Widget, argb As Integer)
            DirectCast(button, Button).PressedColor = ColorFromArgb(argb)
        End Sub

        ' ---------------- text box ----------------

        Public Sub TextBoxSetPlaceholder(textBox As Widget, text As String)
            DirectCast(textBox, TextBox).Placeholder = text
        End Sub

        Public Sub TextBoxSetReadOnly(textBox As Widget, isReadOnly As Boolean)
            DirectCast(textBox, TextBox).IsReadOnly = isReadOnly
        End Sub

        Public Sub TextBoxSetMaxLength(textBox As Widget, maxLength As Integer)
            DirectCast(textBox, TextBox).MaxLength = maxLength
        End Sub

        Public Function TextBoxGetText(textBox As Widget) As String
            Return DirectCast(textBox, TextBox).Text
        End Function

        Public Sub TextBoxFocus(textBox As Widget)
            DirectCast(textBox, TextBox).Focus()
        End Sub

End Module