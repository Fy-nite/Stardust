package ovh.finite.stardust.ui;

/** A Stardust desktop window. Obtained from {@link Display#createWindow}. */
public class Window {

    private final cli.Stardust.JavaApi.JavaWindow jw;
    private boolean shown = false;

    Window(cli.Stardust.JavaApi.JavaWindow jw) {
        this.jw = jw;
    }

    public void setTitle(String title) {
        jw.SetTitle(title);
    }

    public String title() {
        return jw.GetTitle();
    }

    public void setSize(int width, int height) {
        jw.SetSize(width, height);
    }

    public int clientWidth() {
        return jw.GetClientWidth();
    }

    public int clientHeight() {
        return jw.GetClientHeight();
    }

    public void setContent(Widget root) {
        jw.SetRootWidget(root == null ? null : root.widget());
    }

    public void fill(UiColor color) {
        jw.Fill(color.argb());
    }

    public boolean isClosed() {
        return jw.IsClosed();
    }

    /** Same effect as {@link Display#show} - makes the window appear once. */
    public void show() {
        if (!shown) {
            jw.Show();
            shown = true;
        }
    }

    public void close() {
        jw.Close();
        shown = false;
    }

    public void focus() {
        jw.Focus();
    }

    /** Shows the window (once) and blocks the current thread until it is closed. */
    public void block() {
        show();
        jw.Block();
    }
}