package ovh.finite.stardust.ui;

/**
 * Compile-time API for the Stardust Java UI facade. Calling any of these at
 * runtime raises UnsupportedOperationException; the OS supplies the real
 * implementation through its class loader parent, so app jars must not bundle
 * the ui package.
 */
public abstract class Widget {

    final cli.Stardust.Display.Widget widget;

    Widget(cli.Stardust.Display.Widget widget) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    cli.Stardust.Display.Widget widget() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public int x() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setX(int x) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public int y() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setY(int y) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public int width() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setWidth(int width) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public int height() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setHeight(int height) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public boolean isVisible() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setVisible(boolean visible) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public boolean isEnabled() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setEnabled(boolean enabled) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setBackground(UiColor color) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setBorder(UiColor color) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setForeground(UiColor color) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void addChild(Widget child) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void removeChild(Widget child) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void clearChildren() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void bringToFront(Widget child) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setOnClick(ClickHandler handler) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setOnKey(KeyHandler handler) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setOnScroll(ScrollHandler handler) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setOnEnter(Runnable handler) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setOnLeave(Runnable handler) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void java_onMouseClick(int x, int y) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void java_onMouseDoubleClick(int x, int y) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void java_onMouseEnter() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void java_onMouseLeave() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void java_onMouseScroll(int delta) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void java_onKeyPress(char keyChar, int consoleKey, boolean shift) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }
}