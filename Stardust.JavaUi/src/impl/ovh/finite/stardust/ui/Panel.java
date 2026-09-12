package ovh.finite.stardust.ui;

public class Panel extends Widget {

    private final cli.Stardust.Display.Panel panel;

    public Panel() {
        super(cli.Stardust.JavaApi.JavaUi.NewPanel());
        this.panel = (cli.Stardust.Display.Panel) widget;
    }

    public void setVerticalLayout() { cli.Stardust.JavaApi.JavaUi.PanelSetLayout(panel, 1); }
    public void setHorizontalLayout() { cli.Stardust.JavaApi.JavaUi.PanelSetLayout(panel, 2); }
    public void setNoLayout() { cli.Stardust.JavaApi.JavaUi.PanelSetLayout(panel, 0); }
    public void setSpacing(int spacing) { cli.Stardust.JavaApi.JavaUi.PanelSetSpacing(panel, spacing); }
}
