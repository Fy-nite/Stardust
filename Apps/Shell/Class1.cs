using Stardust.Core;
using javax.swing;

namespace Shell
{
    public class Shell : ProcessNode
    {
        public override void init()
        {
            Console.WriteLine("hello from shell");
        }

        public override void run()
        {
            var shellFrame = DisplayServer.Instance.CreateInternalFrame("Shell Terminal", 600, 400);
            var textArea = new JTextArea();
            textArea.setEditable(false);
            shellFrame.getContentPane().add(new JScrollPane(textArea));
            DisplayServer.Instance.AddFrame(shellFrame);

            while (true)
            {
                var CMD = Console.ReadLine();
                if (CMD == null)
                {
                    Console.WriteLine("you need to enter a command");
                }
                textArea.append($"> {CMD}\n");
                Console.WriteLine(CMD);
                switch (CMD)
                {
                    case "ls":
                        break;
                    case "desktop":
                        break;
                    case "exit":
                        Console.WriteLine("shell exiting...");
                        ExitProcess();
                        return;
                    default: Console.WriteLine($"invalid command {CMD}"); break;
                }
            }
        }
    }
}
