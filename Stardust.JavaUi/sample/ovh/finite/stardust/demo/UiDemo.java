package ovh.finite.stardust.demo;

import ovh.finite.stardust.ui.Button;
import ovh.finite.stardust.ui.Display;
import ovh.finite.stardust.ui.Label;
import ovh.finite.stardust.ui.Panel;
import ovh.finite.stardust.ui.TextBox;
import ovh.finite.stardust.ui.UiColor;
import ovh.finite.stardust.ui.Window;

/** Small demo of the Java UI facade: window, panel layout, label, button, text box. */
public class UiDemo {

    public static void main(String[] args) {
        System.out.println("[UiDemo] hello from Java on Stardust");

        Window window = Display.createWindow("Stardust Demo", 420, 300);

        Panel root = new Panel();
        root.setVerticalLayout();
        root.setSpacing(8);

        Label title = new Label("Java on Stardust");
        title.setTextColor(UiColor.ORANGE);

        final Label clicks = new Label("clicks: 0");
        clicks.setTextColor(UiColor.LIGHT_GRAY);

        final int[] count = {0};

        Button button = new Button("Click me");
        button.setButtonColor(UiColor.of(66, 135, 245));
        button.setOnClick(new Runnable() {
            public void run() {
                count[0] += 1;
                clicks.setText("clicks: " + count[0]);
            }
        });

        final Label echo = new Label("type a name and press Enter");
        echo.setTextColor(UiColor.CYAN);

        final TextBox input = new TextBox(220);
        input.setPlaceholder("your name");
        input.setOnSubmit(new Runnable() {
            public void run() {
                echo.setText("hello, " + input.text());
            }
        });

        root.addChild(title);
        root.addChild(clicks);
        root.addChild(button);
        root.addChild(input);
        root.addChild(echo);

        window.setContent(root);
        window.block();

        System.out.println("[UiDemo] window closed, bye");
    }
}