package ovh.finite.stardust.ui;

/** Compile-time API for a single line of text. */
public class Label extends Widget {

    public Label(String text) {
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

    public void setAlignment(int alignment) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }
}