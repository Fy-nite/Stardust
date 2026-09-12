package cli.Stardust.Display;

public abstract class Widget {

    public int get_X() { return 0; }
    public void set_X(int x) { }
    public int get_Y() { return 0; }
    public void set_Y(int y) { }
    public int get_Width() { return 0; }
    public void set_Width(int width) { }
    public int get_Height() { return 0; }
    public void set_Height(int height) { }
    public boolean get_Visible() { return true; }
    public void set_Visible(boolean visible) { }
    public boolean get_Enabled() { return true; }
    public void set_Enabled(boolean enabled) { }

    public void AddChild(cli.Stardust.Display.Widget child) { }
    public void RemoveChild(cli.Stardust.Display.Widget child) { }
    public void ClearChildren() { }
    public void BringToFront(cli.Stardust.Display.Widget child) { }
}
