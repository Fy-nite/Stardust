# Stardust OS — Architecture

A hobby operating system built on .NET 10 with a Java Swing (IKVM) graphical desktop,
virtual filesystem, and a process/app model.

---

## Project Structure

```
Stardust.slnx
|
|-- Stardust/                          # Boot loader (EXE)
|   `-- Program.vb                     #   Entry point: mounts VFS, starts DisplayServer, launches apps
|
|-- Stardust.Core/                     # Kernel library
|   |-- Kernel/
|   |   |-- DisplayServer.vb           #   Swing MDI desktop (JFrame + JDesktopPane)
|   |   |-- AppHost/                   #   Process/application model
|   |   |   |-- IApplication.vb        #     PID, PPID, Init/run/tick/ExitProcess
|   |   |   |-- ProcessNode.vb         #     Abstract base: inherits IApplication
|   |   |   |-- GuiProcessNode.vb      #     ProcessNode + owning JFrame
|   |   |   |-- NativeAppProcess.vb    #     Wraps a static Main() as a ProcessNode
|   |   |   |-- ProcessManager.vb      #     Loads DLLs / registers apps, manages lifecycle
|   |   |   |-- ProcessManager_Registry.vb  #   Register classes at virtual paths
|   |   |   `-- Contexts/
|   |   |       `-- StarDustAssemblyContext.vb  #  Isolated AssemblyLoadContext
|   |   |-- Registry/                  #   Stub registry service
|   |   `-- UI/                        #   PicoTron pixel-art theme
|   |       |-- PicoTronTheme.vb       #     Installs custom UI delegates
|   |       |-- PicoTronResources.vb   #     Image cache + nine-patch slicer
|   |       |-- PicoTronButtonUI.vb    #     Custom button painting
|   |       |-- PicoTronPanelUI.vb     #     Custom panel background
|   |       |-- PicoTronInternalFrameUI.vb  #  Custom window chrome
|   |       `-- PicoTronTitlePane.vb   #     Custom title bar
|   |-- Drivers/VFS/                   #   Virtual filesystem drivers
|   |   |-- Interfaces/
|   |   |   |-- IFileSystemDriver.vb   #     FileExists, OpenFile, etc.
|   |   |   `-- IVFSNode.vb
|   |   `-- FileSystem/
|   |       |-- FolderDriver.vb        #     Maps VFS path to host folder
|   |       |-- DiskDriver.vb          #     Wraps DiscUtils FatFileSystem
|   |       |-- ISODriver.vb           #     Stub
|   |       `-- VHDDriver.vb           #     VHDX creation/opening
|   `-- Sys/globals/FileSystems.vb     #   FS module: Mount, ReadAllText, ExtractToTemp
|
|-- Stardust.FileSystem.BaseFS/        # Base filesystem utilities
|   |-- Class1.vb                      #   FSConnector — creates default dirs on FAT
|   `-- programs/sh.vb                 #   Stub
|
|-- Stardust.Hdd/                      # VHD image generator (C#)
|   `-- Class1.cs                      #   Genhdd()
|
|-- Apps/Shell/                        # User-space app
|   |-- Shell.csproj                   #   C# project, targets net10.0
|   `-- Class1.cs                      #   Shell : ProcessNode — terminal in JInternalFrame
|
`-- RootFS/bin/                        # Deployed VFS root
    |-- Shell.dll
    |-- Stardust.Core.dll
    `-- Stardust.FileSystem.BaseFS.dll
```

---

## Boot Sequence

1. `Program.Main()` — creates `ProcessManager`, mounts `RootFS/` folder as VFS root `/`
2. `DisplayServer.Instance.Run()` — shows the Swing desktop window (JFrame + JDesktopPane)
3. Calls `procs.Start("/bin/Shell.dll")` — loads and runs the Shell app

---

## Application Model

### IApplication — `Kernel/AppHost/IApplication.vb`
```vb
Interface IApplication
    PID, PPID, CurrentThread
    Init()        — called before run
    run()         — main logic
    tick()        — periodic callback (no-op by default)
    ExitProcess() — triggers OnExit
End Interface
```

### ProcessNode — `Kernel/AppHost/ProcessNode.vb`
Abstract base class implementing `IApplication`. Provides `PID`/`PPID` properties,
an `OnExit` event, and an `Overridable ExitProcess()`.

### GuiProcessNode — `Kernel/AppHost/GuiProcessNode.vb`
Extension of `ProcessNode` that owns a Swing `JFrame`. Subclasses override
`BuildUI()` to populate the window. The frame shows on the EDT via
`SwingUtilities.invokeAndWait`. The process thread blocks until the window
is closed, then `ExitProcess()` disposes the frame.

```vb
Public Class MyApp
    Inherits GuiProcessNode

    Public Sub New()
        MyBase.New("My App", 640, 480)
    End Sub

    Public Overrides Sub BuildUI()
        Window.getContentPane().add(New JButton("Click"))
    End Sub
End Class
```

### ProcessManager — `Kernel/AppHost/ProcessManager.vb`

The `ProcessManager` starts applications. `Start(path)` checks these in order:

| # | Mechanism | Description |
|---|-----------|-------------|
| 1 | **Registered apps** | `RegisterApp()` maps a path to a `ProcessNode` subclass |
| 2 | **DLL load** | `RunDll()` extracts a `.dll` from VFS, loads it in an isolated `AssemblyLoadContext`, finds a `ProcessNode` subclass or `Main()` method |

### App Registry — `Kernel/AppHost/ProcessManager_Registry.vb`

Register a .NET class at a virtual path, then launch it as if it were a file:

```vb
' Register
procs.RegisterApp("/bin/sh", GetType(Shell.Shell))
' Generic version
procs.RegisterApp(Of Shell.Shell)("/bin/sh")

' Launch
procs.Start("/bin/sh")

' Remove
procs.UnregisterApp("/bin/sh")
```

The type must inherit `ProcessNode` (or `GuiProcessNode`) and not be abstract.
The path is case-insensitive. Registered apps are checked **before** file-based
loading, so they can shadow VFS files.

Registered apps run on their own thread with PID tracking. They are NOT wrapped
in an `AssemblyLoadContext` (they live in the main AppDomain), so the app class
must be a direct project reference.

### NativeAppProcess — `Kernel/AppHost/NativeAppProcess.vb`
Wraps a static `Main()` method found via reflection in a loaded DLL — used as
fallback when no `ProcessNode` subclass is found.

---

## Display Server

### DisplayServer — `Kernel/DisplayServer.vb`
Singleton Swing desktop manager. Owns a `JFrame` containing a `JDesktopPane`
(for MDI-style internal frames). Provides:

- `CreateInternalFrame(title)` — factory for `JInternalFrame`
- `AddFrame(frame)` — adds it to the desktop on the EDT
- `Run()` — shows the main frame (also installs PicoTron theme)

### SwingRunner — `Kernel/DisplayServer.vb`
Helper implementing `java.lang.Runnable` so that VB lambdas can be passed to
`SwingUtilities.invokeLater()` / `invokeAndWait()`.

---

## Virtual Filesystem

### FS Module — `Sys/globals/FileSystems.vb`
Global `FS` module providing:

| Method | Purpose |
|--------|---------|
| `Mount(point, driver)` | Mount a driver at a VFS path |
| `FileExists(path)` | Check file existence |
| `ReadAllText(path)` | Read text file |
| `WriteAllText(path, content)` | Write text file |
| `ReadAllBytes(path)` | Read binary file |
| `ExtractToTemp(path)` | Copy VFS file to temp, return physical path |
| `GetFiles(path)` | List files in directory |

Mounts resolve by longest-prefix match. The boot loader mounts `RootFS/` at `/`.

### Drivers

| Driver | File | Backing |
|--------|------|---------|
| `FolderDriver` | `Drivers/VFS/FileSystem/FolderDriver.vb` | Host OS folder |
| `DiskDriver` | `Drivers/VFS/FileSystem/DiskDriver.vb` | DiscUtils FatFileSystem |
| `VHDXDriver` | `Drivers/VFS/FileSystem/VHDDriver.vb` | .vhdx disk images |
| `ISODriver` | `Drivers/VFS/FileSystem/ISODriver.vb` | Stub |

### Interface: `IFileSystemDriver` — `Drivers/VFS/Interfaces/IFileSystemDriver.vb`
```vb
Interface IFileSystemDriver
    FileExists(path), DirectoryExists(path)
    GetFiles(path), GetDirectories(path)
    OpenFile(path, mode, access) As Stream
    CreateDirectory(path), DeleteFile(path)
    ReadAllBytes(path), WriteAllBytes(path, bytes)
End Interface
```

---

## PicoTron Theme (`Kernel/UI/`)

A custom Swing Look and Feel for pixel-art window chrome, installed at startup
by `DisplayServer.Run()`.

### PicoTronTheme.vb
Calls `UIManager.put()` to register custom UI delegates for Button, Panel, and
InternalFrame. Runs on the EDT.

### PicoTronResources.vb
Image cache that loads PNGs from `RootFS/ui/{key}.png` as `ImageIcon` objects.
Provides `SliceNine()` — a 3×3 nine-patch scaler that preserves corner pixels
while tiling edges and center (ideal for retro/pixel-art borders).

### Custom UI Delegates
| Class | Base | Override |
|-------|------|----------|
| `PicoTronButtonUI` | `BasicButtonUI` | `paint()` — nearest-neighbor interpolation, ready for image slices |
| `PicoTronPanelUI` | `BasicPanelUI` | `paint()` — ready for tiled backgrounds |
| `PicoTronInternalFrameUI` | `BasicInternalFrameUI` | `installUI()` — swaps in `PicoTronTitlePane` |

### PicoTronTitlePane.vb
Custom `JComponent` for the internal frame title bar. Paints from
`RootFS/ui/title_bar.png` via nine-patch, falls back to dark gray bar with
white text. Title text is drawn in bold 11pt SansSerif.

### Adding images
Drop 3×3 nine-patch PNGs into `RootFS/ui/`:
- `title_bar.png` — window title bars
- `btn_idle.png` — button up state
- `btn_hover.png` — button hover state
- `btn_pressed.png` — button down state

---

## VFS ↔ Host Path Mapping

In `Program.vb`, `RootFS/` is mounted at `/`:

```
Host path                          VFS path
RootFS/bin/Shell.dll              /bin/Shell.dll
RootFS/ui/title_bar.png           /ui/title_bar.png
```

The `FS` module's `NormalisePath` converts `/` to `\` internally. The
`FolderDriver` maps `\bin\Shell.dll` to `RootFS\bin\Shell.dll` on the host.

---

## IKVM Notes

The project uses **IKVM 8.15.0** to bridge Java Swing/AWT into .NET. This makes
`javax.swing.*`, `java.awt.*`, `java.lang.*` etc. available as .NET types.

### VB.NET Name Clashes

Java's case-sensitive package/class naming clashes with VB's case-insensitivity:

| Java | VB Issue |
|------|----------|
| `java.awt.Event` + `java.awt.event` | `event` is ambiguous |
| `java.awt.Image` + `java.awt.image` | `image` is ambiguous |
| `java.awt.Color` + `java.awt.color` | `color` is ambiguous |
| `java.awt.Font` + `java.awt.font` | `font` is ambiguous |

**Workarounds used:**
- Avoid `Imports java.awt.event` — use `Imports` aliases with `[event]`
- Use `javax.swing.plaf.ColorUIResource` / `FontUIResource` instead of `java.awt.Color` / `Font`
- Use `javax.swing.ImageIcon` instead of `java.awt.Image` / `BufferedImage`

---

## Building

```bash
dotnet build Stardust.slnx
dotnet build Apps/Shell/Shell.csproj
```

The boot project is `Stardust\Stardust.vbproj` (output: `Stardust.exe`).
Apps compile to `RootFS/bin/` for deployment.

---

## Future Directions (stubs)

- `Kernel/Registry/` — basic service registry (`RegistrySystem(Of T)`)
- `Sys/Interfaces/IProcessNode.vb` — unused interface
- `Sys/Abstractions/ComputerProcess.vb` — empty class
- `Drivers/VFS/FileSystem/Nodes/` — file/symlink node stubs
- `Drivers/VFS/FileSystem/ISODriver.vb` — not implemented
- `StartIRApp()` in `ProcessManager` — not completed (`.oir`/`.bir` format)
