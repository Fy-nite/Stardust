package ovh.finite.stardust.ui;

/** Compile-time API for a single-line text input. */
public class TextBox extends Widget {

    public TextBox() {
        super(null);
    }

    public TextBox(int width) {
        this();
    }

    public String text() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setText(String text) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setPlaceholder(String placeholder) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setReadOnly(boolean readOnly) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setMaxLength(int maxLength) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void focus() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setOnSubmit(Runnable handler) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setOnTextChanged(TextChangedHandler handler) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }
}