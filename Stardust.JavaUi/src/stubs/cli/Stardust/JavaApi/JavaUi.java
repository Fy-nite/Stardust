package cli.Stardust.JavaApi;

public class JavaUi {

    private JavaUi() { }

    public static JavaWindow CreateWindow(String title, int width, int height) {
        return new JavaWindow();
    }

    public static JavaWidget NewWidget() {
        return new JavaWidget();
    }

    public static cli.Stardust.Display.Label NewLabel(String text) {
        return new cli.Stardust.Display.Label(text);
    }

    public static cli.Stardust.Display.Panel NewPanel() {
        return new cli.Stardust.Display.Panel();
    }

    public static JavaButton NewButton(String text) {
        return new JavaButton(text);
    }

    public static JavaTextBox NewTextBox() {
        return new JavaTextBox();
    }

    public static void WidgetSetBackground(cli.Stardust.Display.Widget widget, int argb) { }
    public static void WidgetSetBorder(cli.Stardust.Display.Widget widget, int argb) { }
    public static void WidgetSetForeground(cli.Stardust.Display.Widget widget, int argb) { }
    public static void LabelSetTextColor(cli.Stardust.Display.Widget label, int argb) { }
    public static void LabelSetAlignment(cli.Stardust.Display.Widget label, int mode) { }
    public static void PanelSetLayout(cli.Stardust.Display.Widget panel, int mode) { }
    public static void PanelSetSpacing(cli.Stardust.Display.Widget panel, int spacing) { }
    public static void ButtonSetTextColor(cli.Stardust.Display.Widget button, int argb) { }
    public static void ButtonSetButtonColor(cli.Stardust.Display.Widget button, int argb) { }
    public static void ButtonSetHoverColor(cli.Stardust.Display.Widget button, int argb) { }
    public static void ButtonSetPressedColor(cli.Stardust.Display.Widget button, int argb) { }
    public static void TextBoxSetPlaceholder(cli.Stardust.Display.Widget textBox, String text) { }
    public static void TextBoxSetReadOnly(cli.Stardust.Display.Widget textBox, boolean readOnly) { }
    public static void TextBoxSetMaxLength(cli.Stardust.Display.Widget textBox, int maxLength) { }
    public static String TextBoxGetText(cli.Stardust.Display.Widget textBox) { return null; }
    public static void TextBoxFocus(cli.Stardust.Display.Widget textBox) { }
}
