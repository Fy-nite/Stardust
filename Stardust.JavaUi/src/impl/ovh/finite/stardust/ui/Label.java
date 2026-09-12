package ovh.finite.stardust.ui;

public class Label extends Widget {

    private final cli.Stardust.Display.Label label;

    public Label(String text) {
        super(cli.Stardust.JavaApi.JavaUi.NewLabel(text));
        this.label = (cli.Stardust.Display.Label) widget;
    }

    public void setText(String text) { label.set_Text(text); }
    public String text() { return label.get_Text(); }

    public void setTextColor(UiColor color) {
        cli.Stardust.JavaApi.JavaUi.LabelSetTextColor(label, color.argb());
    }

    public void setAlignment(int alignment) {
        cli.Stardust.JavaApi.JavaUi.LabelSetAlignment(label, alignment);
    }
}
