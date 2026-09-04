using Stardust.Display;
using Stardust.Core;
using Stardust.Kernel.AppHost;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;

namespace Shell
{
    public class Shell : GuiProcessNode
    {
        private Terminal _terminal = null!;
        private string _cwd = "\\";

        public override void init()
        {
        }

        public override void BuildUI()
        {
            _terminal = new Terminal(Window.ClientWidth, Window.ClientHeight)
            {
                X = 0,
                Y = 0,
                Enabled = true,
                Visible = true
            };
            Window.RootWidget = _terminal;

            _terminal.Print("Stardust Shell v1.1");
            _terminal.Print("Type 'help' for commands.");
            _terminal.Print("");

            _terminal.OnCommandSubmitted += (sender, e) =>
            {
                string cmd = e.Command.Trim();
                if (cmd.Length > 0)
                {
                    _terminal.PrintCommand(cmd);
                    ProcessCommand(cmd);
                }
            };
        }

        private void ProcessCommand(string line)
        {
            var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return;

            string cmd = tokens[0].ToLower();
            string[] args = tokens.Skip(1).ToArray();

            switch (cmd)
            {
                case "help":
                    PrintHelp();
                    break;
                case "about":
                    _terminal.Print("Stardust Shell - a widget-based terminal for the Stardust OS.");
                    break;
                case "clear":
                    _terminal.Clear();
                    break;
                case "exit":
                case "quit":
                    _terminal.Print("Shell exiting...");
                    RequestExit();
                    Window.IsOpen = false;
                    break;

                case "echo":
                    Echo(args);
                    break;
                case "ls":
                case "dir":
                    Ls(args);
                    break;
                case "cat":
                case "type":
                    Cat(args);
                    break;
                case "pwd":
                    _terminal.Print(_cwd);
                    break;
                case "cd":
                    Cd(args);
                    break;
                case "mkdir":
                    Mkdir(args);
                    break;
                case "touch":
                    Touch(args);
                    break;
                case "rm":
                case "del":
                    Rm(args);
                    break;
                case "date":
                    _terminal.Print(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
                case "whoami":
                    _terminal.Print("root");
                    break;
                case "lsapp":
                    LsApps();
                    break;
                case "run":
                    Run(args);
                    break;
                case "ps":
                    Ps();
                    break;

                default:
                    // Support './app' style launching, e.g. "./Shell.app" or "./sh".
                    if (cmd.StartsWith("./"))
                    {
                        Run(new[] { cmd.Substring(2) });
                    }
                    else if (cmd.StartsWith("/") || cmd.StartsWith("\\"))
                    {
                        Run(new[] { cmd });
                    }
                    else
                    {
                        _terminal.Print($"Unknown command: {cmd}. Type 'help' for a list.");
                    }
                    break;
            }
        }

        private void PrintHelp()
        {
            _terminal.Print("Available commands:");
            _terminal.Print("  help                show this help");
            _terminal.Print("  about               about the shell");
            _terminal.Print("  clear               clear the screen");
            _terminal.Print("  exit, quit          exit the shell");
            _terminal.Print("");
            _terminal.Print("  echo <text>         print text");
            _terminal.Print("  pwd                 print working directory");
            _terminal.Print("  cd <dir>            change directory");
            _terminal.Print("  ls [path]           list directory contents");
            _terminal.Print("  cat <file>          print a file");
            _terminal.Print("  mkdir <dir>         create a directory");
            _terminal.Print("  touch <file>        create an empty file");
            _terminal.Print("  rm <file>           delete a file");
            _terminal.Print("  date                show current date/time");
            _terminal.Print("  whoami              current user");
            _terminal.Print("  lsapp               list installed apps (.app bundles)");
            _terminal.Print("  run <app>           launch an app (or use ./name)");
            _terminal.Print("  ps                  list running processes");
        }

        private void Echo(string[] args)
        {
            if (args.Length == 0) { _terminal.Print(""); return; }
            var text = string.Join(" ", args);
            int arrow = text.IndexOf(">>");
            if (arrow >= 0 && arrow < text.Length - 2)
            {
                string content = text.Substring(0, arrow).Trim();
                string file = text.Substring(arrow + 2).Trim().Replace('/', '\\');
                try
                {
                    var path = Resolve(file);
                    string existing = FS.FileExists(path) ? FS.ReadAllText(path) : "";
                    FS.WriteAllText(path, existing + content + "\n");
                    _terminal.Print($"Appended to {file}");
                }
                catch (Exception ex)
                {
                    _terminal.Print($"Error: {ex.Message}");
                }
                return;
            }
            arrow = text.IndexOf(">");
            if (arrow >= 0 && arrow < text.Length - 1)
            {
                string content = text.Substring(0, arrow).Trim();
                string file = text.Substring(arrow + 1).Trim().Replace('/', '\\');
                try
                {
                    FS.WriteAllText(Resolve(file), content + "\n");
                    _terminal.Print($"Wrote to {file}");
                }
                catch (Exception ex)
                {
                    _terminal.Print($"Error: {ex.Message}");
                }
                return;
            }
            _terminal.Print(text);
        }

        private void Ls(string[] args)
        {
            string target = args.Length > 0 ? args[0] : ".";
            string dir = Resolve(target);
            try
            {
                if (!FS.DirectoryExists(dir))
                {
                    _terminal.Print($"ls: {target}: No such directory");
                    return;
                }
                var dirs = FS.GetDirectories(dir).Select(Path.GetFileName).ToArray();
                var files = FS.GetFiles(dir).Select(Path.GetFileName).ToArray();
                foreach (var d in dirs) _terminal.Print(d + "/");
                foreach (var f in files) _terminal.Print(f);
                if (dirs.Length == 0 && files.Length == 0)
                    _terminal.Print($"(empty)  [{dir}]");
                else
                    _terminal.Print($"{files.Length} file(s), {dirs.Length} dir(s)");
            }
            catch (Exception ex)
            {
                _terminal.Print($"Error: {ex.Message}");
            }
        }

        private void Cat(string[] args)
        {
            if (args.Length == 0) { _terminal.Print("Usage: cat <file>"); return; }
            string path = Resolve(args[0]);
            try
            {
                if (!FS.FileExists(path)) { _terminal.Print($"cat: {args[0]}: No such file"); return; }
                var content = FS.ReadAllText(path);
                foreach (var ln in content.Replace("\r\n", "\n").Split('\n'))
                    _terminal.Print(ln);
            }
            catch (Exception ex)
            {
                _terminal.Print($"Error: {ex.Message}");
            }
        }

        private void Cd(string[] args)
        {
            if (args.Length == 0) { _cwd = "\\"; return; }
            string path = Resolve(args[0]);
            if (!FS.DirectoryExists(path)) { _terminal.Print($"cd: {args[0]}: No such directory"); return; }
            _cwd = path;
        }

        private void Mkdir(string[] args)
        {
            if (args.Length == 0) { _terminal.Print("Usage: mkdir <dir>"); return; }
            foreach (var a in args)
            {
                try
                {
                    FS.CreateDirectory(Resolve(a));
                }
                catch (Exception ex) { _terminal.Print($"mkdir: {a}: {ex.Message}"); }
            }
        }

        private void Touch(string[] args)
        {
            if (args.Length == 0) { _terminal.Print("Usage: touch <file>"); return; }
            foreach (var a in args)
            {
                try
                {
                    var path = Resolve(a);
                    if (!FS.FileExists(path)) FS.WriteAllText(path, "");
                }
                catch (Exception ex) { _terminal.Print($"touch: {a}: {ex.Message}"); }
            }
        }

        private void Rm(string[] args)
        {
            if (args.Length == 0) { _terminal.Print("Usage: rm <file>"); return; }
            foreach (var a in args)
            {
                var path = Resolve(a);
                if (!FS.FileExists(path)) { _terminal.Print($"rm: {a}: No such file"); continue; }
                try
                {
                    FS.DeleteFile(path);
                }
                catch (Exception ex) { _terminal.Print($"rm: {a}: {ex.Message}"); }
            }
        }

        private void LsApps()
        {
            // Discover apps from .app bundles on the file system (macOS-style metadata).
            var apps = Stardust.Core.Apps.AppBundleLoader.ScanApps("/bin");
            apps = apps.Where(a => !a.Hidden).ToList();
            if (apps.Count == 0)
            {
                _terminal.Print("No app bundles found in /bin.");
                return;
            }
            _terminal.Print($"{"NAME",-20} {"VERSION",-10} CATEGORY");
            _terminal.Print(new string('-', 44));
            foreach (var app in apps.OrderBy(a => a.Name))
            {
                string name = app.IsValid ? app.Name : Path.GetFileName(app.Path);
                string extra = app.IsValid && app.Description.Length > 0 ? $" - {app.Description}" : string.Empty;
                _terminal.Print($"{(app.IsValid ? app.Name : app.Path + " (no manifest)"),-20} {(app.Version ?? "-"),-10} {(app.Category ?? "-")}{extra}");
            }
        }

        private void Run(string[] args)
        {
            if (args.Length == 0) { _terminal.Print("Usage: run <app>    e.g. run Shell.app"); return; }
            string target = args[0];
            string? launchable = ResolveAppPath(target);
            if (launchable == null)
            {
                _terminal.Print($"run: {target}: no such app");
                return;
            }
            try
            {
                var node = ProcessManager.Instance.Start(launchable);
                _terminal.Print(node != null
                    ? $"Started {launchable} (pid {node.PID})"
                    : $"run: {target}: failed to start");
            }
            catch (Exception ex)
            {
                _terminal.Print($"run: {target}: {ex.Message}");
            }
        }

        private void Ps()
        {
            var procs = ProcessManager.Instance.Processes;
            if (procs.Count == 0) { _terminal.Print("No running processes."); return; }
            _terminal.Print($"{"PID",-6} {"PPID",-6} TYPE");
            _terminal.Print(new string('-', 30));
            foreach (var p in procs)
            {
                _terminal.Print($"{p.PID,-6} {p.PPID,-6} {p.GetType().Name}");
            }
        }

        /// <summary>
        /// Resolves an app name (e.g. "Shell", "Shell.app", "./sh", "/bin/Browser.sda")
        /// to a launchable registered path.
        /// </summary>
        private string? ResolveAppPath(string target)
        {
            if (target.StartsWith("/") || target.StartsWith("\\"))
            {
                return target.Replace('/', '\\');
            }

            string t = target.Replace('/', '\\');
            if (t.StartsWith(".\\")) t = t.Substring(2);

            var candidates = new[]
            {
                "\\bin\\" + t,
                "\\bin\\" + t + ".app",
                "\\bin\\" + t + ".sda",
            };
            foreach (var c in candidates)
            {
                if (ProcessManager.Instance.IsRegistered(c)) return c;
                if (c.EndsWith(".app") && FS.DirectoryExists(c)) return c;
            }
            return null;
        }

        private string Resolve(string target)
        {
            string t = target.Replace('/', '\\');
            if (string.IsNullOrEmpty(t) || t == "\\") return "\\";

            // absolute path
            if (t.StartsWith("\\"))
                return Normalize(t);

            // relative path resolved against the current directory
            var combined = (_cwd == "\\" ? "" : _cwd.TrimEnd('\\')) + "\\" + t.TrimStart('\\');
            return Normalize(combined);
        }

        /// <summary>
        /// Normalises a virtual path (backslash form), collapsing ".", ".." and
        /// duplicated separators, e.g. "\bin\Shell.app\..\sh" -> "\bin\sh".
        /// Traversal above the root is clamped to root.
        /// </summary>
        private static string Normalize(string path)
        {
            var parts = path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var stack = new System.Collections.Generic.List<string>();
            bool absolute = path.StartsWith("\\");

            foreach (var part in parts)
            {
                if (part == ".") continue;
                if (part == "..")
                {
                    if (stack.Count > 0) stack.RemoveAt(stack.Count - 1);
                    // remain at root if already there
                    continue;
                }
                stack.Add(part);
            }

            string joined = string.Join("\\", stack);
            if (absolute) joined = "\\" + joined;
            return joined == "\\" ? "\\" : joined;
        }

    }
}
