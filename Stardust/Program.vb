Imports System, System.IO
Imports System.Reflection.Emit
Imports DiscUtils, DiscUtils.Ntfs, DiscUtils.Partitions
Imports DiscUtils.Complete
Imports Stardust.Core
Imports Stardust.Core.Drivers.VFS.FileSystem
Imports Stardust.Kernel.AppHost
Imports Stardust.Display
Imports Stardust.FileSystem.BaseFS

Module Program
    Sub Main(args As String())

        Dim procs As ProcessManager = ProcessManager.Instance

        ' The FS module auto-mounts "/" to the nearest RootFS folder found by walking
        ' up the directory tree (a checked-in RootFS with /bin bundles), so no
        ' explicit mount is needed here. Mounting from BaseDirectory would otherwise
        ' pick up a stale empty build-output RootFS.

        SetupHelper.SetupComplete()
        Console.WriteLine("Stardust OS Booting...")

        ' Register each app by its launch path so the shell can launch them via ./name
        ' or by their .app bundle path (resolved from /bin/<Name>.app).
        procs.RegisterApp (Of DriverViewer)("/bin/DriverViewer.sda")
        procs.RegisterApp (Of DriverViewer)("/bin/DriverViewer.app")
        procs.RegisterApp (Of Browser)("/bin/Browser.sda")
        procs.RegisterApp (Of Browser)("/bin/Browser.app")
        procs.RegisterApp (Of SettingsApp)("/bin/Settings.sda")
        procs.RegisterApp (Of SettingsApp)("/bin/Settings.app")
        procs.RegisterApp (Of Shell.Shell)("/bin/sh")
        procs.RegisterApp (Of Shell.Shell)("/bin/Shell.app")


        DisplayServer.Instance.RunServer(Sub()
            If FS.FileExists("/sbin/init") Then
                procs.Start("/sbin/init")
            Else
                Console.WriteLine("/sbin/init does not exist, falling back to regular shell start")
                procs.Start("/bin/sh")
                procs.Start("/bin/DriverViewer.sda")
                procs.Start("/bin/Browser.sda")
                procs.Start("/bin/Settings.sda")
            End If
        End Sub)
    End Sub

    Public Sub RecursePrintFileNodes(disk As VHDXDriver)
        PrintFolder(disk.Root, "", 0)
    End Sub

    Private Sub PrintFolder(fs As Object, path As String, level As Integer)

        Dim indent As String = New String(" "c, level*2)

        For Each file In fs.GetFiles(path)
            Console.WriteLine(indent & fs.GetFileInfo(file).Name)
        Next
        For Each Dizr In fs.GetDirectories(path)

            Dim info = fs.GetDirectoryInfo(Dizr)

            Console.WriteLine(indent & "[" & info.Name & "]")

            PrintFolder(fs, info.FullName, level + 1)

        Next
    End Sub
End Module
