Imports System, System.IO
Imports System.Reflection.Emit
Imports DiscUtils, DiscUtils.Ntfs, DiscUtils.Partitions
Imports DiscUtils.Complete
Imports Stardust.Core.Drivers.VFS.FileSystem
Imports Stardust.FileSystem.BaseFS

Module Program
    Dim Driver As VHDXDriver
    Sub Main(args As String())

        SetupHelper.SetupComplete()
        Console.WriteLine("Hello World!")

        Dim DiskSize As Long = 30L * 1024 * 1024
        'Using VhdStream As Stream = File.Create("Install.vhdx")
        '    Using Disk As Vhd.Disk = Vhd.Disk.InitializeDynamic(VhdStream, Ownership.Dispose, DiskSize)
        '        BiosPartitionTable.Initialize(Disk, WellKnownPartitionType.WindowsFat)
        '        Using Root As FatFileSystem = FatFileSystem.FormatPartition(Disk, 0, Nothing)
        '            Root.CreateDirectory("TestDir\CHILD")
        '        End Using
        '    End Using
        'End Using
        Driver = New VHDXDriver()
        If Not File.Exists("root.vhd") Then
            Console.WriteLine("root.vhd not found, creating new disk.")
            Driver.CreateDisk("root.vhd", 1024L * 1024 * 1024) ' Create a 1GB VHDX file
            Dim FSConnect As New FSConnector()
            FSConnect.InitDrive(Driver.Root)
        Else
            Driver.OpenDisk("root.vhd")
        End If
        RecursePrintFileNodes(Driver)

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
