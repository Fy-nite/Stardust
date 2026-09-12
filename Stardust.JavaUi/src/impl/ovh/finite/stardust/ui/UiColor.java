package ovh.finite.stardust.ui;

/** ARGB colour value used across the Stardust Java UI facade. */
public final class UiColor {

    public static final UiColor WHITE = UiColor.of(255, 255, 255);
    public static final UiColor BLACK = UiColor.of(0, 0, 0);
    public static final UiColor RED = UiColor.of(255, 0, 0);
    public static final UiColor GREEN = UiColor.of(0, 255, 0);
    public static final UiColor BLUE = UiColor.of(0, 0, 255);
    public static final UiColor YELLOW = UiColor.of(255, 255, 0);
    public static final UiColor CYAN = UiColor.of(0, 255, 255);
    public static final UiColor MAGENTA = UiColor.of(255, 0, 255);
    public static final UiColor GRAY = UiColor.of(128, 128, 128);
    public static final UiColor LIGHT_GRAY = UiColor.of(192, 192, 192);
    public static final UiColor DARK_GRAY = UiColor.of(64, 64, 64);
    public static final UiColor ORANGE = UiColor.of(255, 165, 0);
    public static final UiColor PINK = UiColor.of(255, 192, 203);
    public static final UiColor BROWN = UiColor.of(165, 42, 42);
    public static final UiColor TRANSPARENT = UiColor.of(0, 0, 0, 0);

    private final int argb;

    private UiColor(int argb) {
        this.argb = argb;
    }

    public static UiColor of(int r, int g, int b) {
        return of(r, g, b, 255);
    }

    public static UiColor of(int r, int g, int b, int a) {
        int argb = ((a & 255) << 24) | ((r & 255) << 16) | ((g & 255) << 8) | (b & 255);
        return new UiColor(argb);
    }

    public int argb() {
        return argb;
    }

    public int alpha() {
        return (argb >> 24) & 255;
    }

    public int red() {
        return (argb >> 16) & 255;
    }

    public int green() {
        return (argb >> 8) & 255;
    }

    public int blue() {
        return argb & 255;
    }

    @Override
    public boolean equals(Object o) {
        return o instanceof UiColor && ((UiColor) o).argb == argb;
    }

    @Override
    public int hashCode() {
        return argb;
    }

    @Override
    public String toString() {
        return "UiColor(#" + Integer.toHexString(argb) + ")";
    }
}