package ovh.finite.stardust.ui;

/** Compile-time API for creating and managing Stardust desktop windows. */
public final class Display {

    private Display() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public static Window createWindow(String title) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public static Window createWindow(String title, int width, int height) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public static void show(Window window) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public static void close(Window window) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public static void focus(Window window) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public static void block(Window window) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }
}