package ovh.finite.stardust.ui;

/** Callback for key presses on a widget. */
public interface KeyHandler {
    void onKey(char keyChar, int consoleKey, boolean shift);
}