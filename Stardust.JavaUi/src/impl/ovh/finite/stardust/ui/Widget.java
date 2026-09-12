package ovh.finite.stardust.ui;

public abstract class Widget {

    final cli.Stardust.Display.Widget widget;

    Widget(cli.Stardust.Display.Widget widget) {
        this.widget = widget;
    }

    cli.Stardust.Display.Widget widget() {
        return widget;
    }

    public int x() { return widget.get_X(); }
    public void setX(int x) { widget.set_X(x); }
    public int y() { return widget.get_Y(); }
    public void setY(int y) { widget.set_Y(y); }
    public int width() { return widget.get_Width(); }
    public void setWidth(int width) { widget.set_Width(width); }
    public int height() { return widget.get_Height(); }
    public void setHeight(int height) { widget.set_Height(height); }
    public boolean isVisible() { return widget.get_Visible(); }
    public void setVisible(boolean visible) { widget.set_Visible(visible); }
    public boolean isEnabled() { return widget.get_Enabled(); }
    public void setEnabled(boolean enabled) { widget.set_Enabled(enabled); }

    public void setBackground(UiColor color) {
        cli.Stardust.JavaApi.JavaUi.WidgetSetBackground(widget, color.argb());
    }

    public void setBorder(UiColor color) {
        cli.Stardust.JavaApi.JavaUi.WidgetSetBorder(widget, color.argb());
    }

    public void setForeground(UiColor color) {
        cli.Stardust.JavaApi.JavaUi.WidgetSetForeground(widget, color.argb());
    }

    public void addChild(Widget child) {
        widget.AddChild(child.widget());
    }

    public void removeChild(Widget child) {
        widget.RemoveChild(child.widget());
    }

    public void clearChildren() {
        widget.ClearChildren();
    }

    public void bringToFront(Widget child) {
        widget.BringToFront(child.widget());
    }

    private ClickHandler clickHandler;
    private KeyHandler keyHandler;
    private ScrollHandler scrollHandler;
    private Runnable enterHandler;
    private Runnable leaveHandler;

    public void setOnClick(ClickHandler handler) { clickHandler = handler; }
    public void setOnKey(KeyHandler handler) { keyHandler = handler; }
    public void setOnScroll(ScrollHandler handler) { scrollHandler = handler; }
    public void setOnEnter(Runnable handler) { enterHandler = handler; }
    public void setOnLeave(Runnable handler) { leaveHandler = handler; }

    public void java_onMouseClick(int x, int y) {
        if (clickHandler != null) clickHandler.onClick(x, y);
    }

    public void java_onMouseDoubleClick(int x, int y) { }

    public void java_onMouseEnter() {
        if (enterHandler != null) enterHandler.run();
    }

    public void java_onMouseLeave() {
        if (leaveHandler != null) leaveHandler.run();
    }

    public void java_onMouseScroll(int delta) {
        if (scrollHandler != null) scrollHandler.onScroll(delta);
    }

    public void java_onKeyPress(char keyChar, int consoleKey, boolean shift) {
        if (keyHandler != null) keyHandler.onKey(keyChar, consoleKey, shift);
    }
}
