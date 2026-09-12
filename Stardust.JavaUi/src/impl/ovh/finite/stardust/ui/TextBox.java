package ovh.finite.stardust.ui;

public class TextBox extends Widget {

    private final cli.Stardust.JavaApi.JavaTextBox box;
    private Runnable submitHandler;
    private TextChangedHandler textHandler;

    public TextBox() {
        super(cli.Stardust.JavaApi.JavaUi.NewTextBox());
        this.box = (cli.Stardust.JavaApi.JavaTextBox) widget;
        box.SetOwner(this);
    }

    public TextBox(int width) {
        this();
        setWidth(width);
    }

    public String text() { return cli.Stardust.JavaApi.JavaUi.TextBoxGetText(box); }
    public void setText(String text) { box.set_Text(text); }

    public void setPlaceholder(String placeholder) {
        cli.Stardust.JavaApi.JavaUi.TextBoxSetPlaceholder(box, placeholder);
    }

    public void setReadOnly(boolean readOnly) {
        cli.Stardust.JavaApi.JavaUi.TextBoxSetReadOnly(box, readOnly);
    }

    public void setMaxLength(int maxLength) {
        cli.Stardust.JavaApi.JavaUi.TextBoxSetMaxLength(box, maxLength);
    }

    public void focus() { cli.Stardust.JavaApi.JavaUi.TextBoxFocus(box); }

    public void setOnSubmit(Runnable handler) { submitHandler = handler; }
    public void setOnTextChanged(TextChangedHandler handler) { textHandler = handler; }

    public void java_onSubmit() {
        if (submitHandler != null) submitHandler.run();
    }

    public void java_onTextChanged(String text) {
        if (textHandler != null) textHandler.onText(text);
    }
}
