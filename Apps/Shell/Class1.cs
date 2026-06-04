using Stardust.Core;

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
            while (true)
            {
                var CMD = Console.ReadLine();
                switch (CMD)
                {
                    case "ls":
                        break;
                    default: Console.WriteLine($"invalid command {CMD}"); break;
                }
            }
        }
    }
}
