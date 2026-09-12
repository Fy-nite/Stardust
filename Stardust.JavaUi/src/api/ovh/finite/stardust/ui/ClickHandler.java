package ovh.finite.stardust.ui;

/** Callback for mouse clicks on a widget. Receives widget-local coordinates. */
public interface ClickHandler {
    void onClick(int x, int y);
}