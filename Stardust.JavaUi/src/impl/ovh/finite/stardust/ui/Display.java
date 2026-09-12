package ovh.finite.stardust.ui;

/** Entry point for creating and managing Stardust desktop windows. */
public final class Display {

    private Display() {
    }

    public static Window createWindow(String title) {
        return createWindow(title, 480, 320);
    }

    public static Window createWindow(String title, int width, int height) {
        cli.Stardust.JavaApi.JavaWindow jw = cli.Stardust.JavaApi.JavaUi.CreateWindow(title, width, height);
        return new Window(jw);
    }

    /** Makes the window appear on the desktop (no-op if already shown). */
    public static void show(Window window) {
        window.show();
    }

    public static void close(Window window) {
        window.close();
    }

    public static void focus(Window window) {
        window.focus();
    }

    /** Shows the window and blocks until it is closed. */
    public static void block(Window window) {
        window.block();
    }
}