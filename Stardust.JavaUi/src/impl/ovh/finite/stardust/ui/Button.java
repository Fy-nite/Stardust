package ovh.finite.stardust.ui;

public class Button extends Widget {

    private final cli.Stardust.JavaApi.JavaButton btn;
    private Runnable clickHandler;

    public Button(String text) {
        super(cli.Stardust.JavaApi.JavaUi.NewButton(text));
        this.btn = (cli.Stardust.JavaApi.JavaButton) widget;
        btn.SetOwner(this);
    }

    public void setText(String text) { btn.set_Text(text); }
    public String text() { return btn.get_Text(); }

    public void setTextColor(UiColor color) {
        cli.Stardust.JavaApi.JavaUi.ButtonSetTextColor(btn, color.argb());
    }

    public void setButtonColor(UiColor color) {
        cli.Stardust.JavaApi.JavaUi.ButtonSetButtonColor(btn, color.argb());
    }

    public void setHoverColor(UiColor color) {
        cli.Stardust.JavaApi.JavaUi.ButtonSetHoverColor(btn, color.argb());
    }

    public void setPressedColor(UiColor color) {
        cli.Stardust.JavaApi.JavaUi.ButtonSetPressedColor(btn, color.argb());
    }

    public void setOnClick(Runnable handler) { clickHandler = handler; }

    public void java_onClick() {
        if (clickHandler != null) clickHandler.run();
    }
}
