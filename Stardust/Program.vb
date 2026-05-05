Imports System, System.IO
Imports System.Reflection.Emit
Imports DiscUtils, DiscUtils.Ntfs, DiscUtils.Partitions
Imports DiscUtils.Complete
Imports Stardust.Core
Imports Stardust.Core.Drivers.VFS.FileSystem
Imports Stardust.FileSystem.BaseFS

Module Program
    Dim Driver As VHDXDriver
    Sub Main(args As String())
        Dim procs As ProcessManager = ProcessManager.Init()

        SetupHelper.SetupComplete()
        Console.WriteLine("Hello World!")
        Dim DiskSize As Long = 2048L * 1024 * 1024 ' Create a 2GB VHDX file
        Driver = New VHDXDriver()
        If Not File.Exists("root.vhdx") Then
            Console.WriteLine("root.vhdx not found, creating new disk.")
            Driver.CreateDisk("root.vhdx", DiskSize)
            Dim FSConnect As New FSConnector()
            FSConnect.InitDrive(Driver.Root)
        Else
            Driver.OpenDisk("root.vhdx")
        End If
        'RecursePrintFileNodes(Driver) 

        procs.Start("/sbin/init")
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
