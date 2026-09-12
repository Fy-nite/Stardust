package ovh.finite.stardust.ui;

/** Compile-time API for a widget container. */
public class Panel extends Widget {

    public Panel() {
        super(null);
    }

    public void setVerticalLayout() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setHorizontalLayout() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setNoLayout() {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }

    public void setSpacing(int spacing) {
        throw new UnsupportedOperationException("stardust-ui is provided by the OS at runtime");
    }
}