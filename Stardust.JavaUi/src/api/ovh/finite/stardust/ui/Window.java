package ovh.finite.stardust.ui;

/** Compile-time API for a Stardust desktop window. See {@link Display}. */
public class Window {

    Window(cli.Stardust.JavaApi.JavaWindow jw) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setTitle(String title) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public String title() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setSize(int width, int height) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public int clientWidth() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public int clientHeight() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setContent(Widget root) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void fill(UiColor color) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public boolean isClosed() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void show() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void close() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void focus() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void block() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }
}