package ovh.finite.stardust.ui;

/** Compile-time API for a clickable button. */
public class Button extends Widget {

    public Button(String text) {
        super(null);
    }

    public void setText(String text) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public String text() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setTextColor(UiColor color) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setButtonColor(UiColor color) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setHoverColor(UiColor color) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setPressedColor(UiColor color) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setOnClick(Runnable handler) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }
}