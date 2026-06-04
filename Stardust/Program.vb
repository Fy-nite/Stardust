Imports System, System.IO
Imports System.Reflection.Emit
Imports DiscUtils, DiscUtils.Ntfs, DiscUtils.Partitions
Imports DiscUtils.Complete
Imports Stardust.Core
Imports Stardust.Core.AppHost
Imports Stardust.Core.Drivers.VFS.FileSystem
Imports Stardust.FileSystem.BaseFS

Module Program
    Sub Main(args As String())
        Dim procs As New ProcessManager

        ' 1. Initialize the VFS
        ' Mount the host-side "Root" folder as our system root
        ' This is where /bin, /etc, etc. will live during dev
        Dim rootDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RootFS")
        FS.Mount("/", New FolderDriver(rootDirPath))

        '' 2. (Optional) Mount a VHD for user data
        'Dim vhdPath = "Stardust.vhdx"
        'If File.Exists(vhdPath) Then
        '    Dim vhdLoader As New VHDXDriver()
        '    Dim fatFs = vhdLoader.OpenDisk(vhdPath)
        '    FS.Mount("/mnt/hdd0", New DiskDriver(fatFs))
        '    Console.WriteLine("Mounted VHDX to /mnt/hdd0")
        'End If

        SetupHelper.SetupComplete()
        Console.WriteLine("Stardust OS Booting...")

        ' Check for init or shell
        If FS.FileExists("/sbin/init") Then
            procs.Start("/sbin/init")
        Else
            Console.WriteLine("/sbin/init does not exist, falling back to regular shell start")
            ' Note: RunDll expects a path. If sh is in our VFS, we might need to extract it first
            ' as your ProcessManager currently does for .dll files.
            procs.Start("/bin/Shell.dll")
        End If
    End Sub
    Public Sub RecursePrintFileNodes(disk As VHDXDriver)
        PrintFolder(disk.Root, "", 0)
    End Sub

    Private Sub PrintFolder(fs As Object, path As String, level As Integer)

        Dim indent As String = New String(" "c, level * 2)

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
