Imports System, System.IO
Imports System.Reflection.Emit
Imports DiscUtils, DiscUtils.Ntfs, DiscUtils.Partitions
Imports DiscUtils.Complete
Imports Stardust.Core
Imports Stardust.Core.AppHost
Imports Stardust.Core.Drivers.VFS.FileSystem
Imports Stardust.FileSystem.BaseFS

Module Program
    Dim Driver As VHDXDriver
    Sub Main(args As String())
        Dim procs As New ProcessManager

        SetupHelper.SetupComplete()
        Console.WriteLine("Hello World!")

        'RecursePrintFileNodes(Driver) 
        If FS.FileExists("/sbin/init") Then
            procs.Start("/sbin/init")
        Else
            Console.WriteLine("/sbin/init does not exst, falling back to regular shell start")
            procs.Start("/bin/sh")
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
