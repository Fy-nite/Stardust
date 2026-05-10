Imports System.Drawing
Imports System.IO
Imports DiscUtils
Imports DiscUtils.Fat
Imports DiscUtils.Partitions
Imports DiscUtils.Raw
Imports DiscUtils.Streams
Imports Stardust.Core.Drivers.VFS.Interfaces

Namespace Drivers.VFS.FileSystem
    Public Class VHDXDriver
        Private VHDStream As Stream
        Public Root As FatFileSystem
        Private RootPartition As SparseStream
        Public Property Disk As Vhdx.Disk
        Dim vdisk As VirtualDisk
        Public Property RootFileSystem As FatFileSystem

        Sub CreateDisk(DiskName As String, diskSize As Long)
            Dim Thing As String = "Creating disk with name: " & DiskName & " and size: " & (diskSize - 1).ToString() & " bytes."
            Try
                VHDStream = File.Create(DiskName)

                Disk = Vhdx.Disk.InitializeDynamic(VHDStream, Ownership.Dispose, diskSize)
                BiosPartitionTable.Initialize(Disk, WellKnownPartitionType.WindowsNtfs)

                If Disk Is Nothing Then
                    Console.WriteLine("Disk not created successfully.")
                Else
                    Console.WriteLine("Disk created successfully.")
                End If
                Root = FatFileSystem.FormatPartition(Disk, 0, "Root")

            Catch ex As Exception
                Console.Error.WriteLine()
            End Try
        End Sub
        Sub OpenDisk()
            Dim Diskname = "Stardust.vhdx"
            If File.Exists(DiskName) Then
                Dim Thing As String = "Opening disk with name: " & DiskName
                Console.WriteLine(Thing) ' TODO: Replace this with proper logging
                Disk = Vhdx.Disk.OpenDisk(Diskname, FileAccess.ReadWrite)
                For Each part In Disk.Partitions.Partitions
                    Console.WriteLine("Partition: " & part.ToString())
                Next
                Root = New FatFileSystem(Disk.Partitions.Partitions(0).Open())
            Else
                Throw New FileNotFoundException("Disk not found", DiskName)
            End If
        End Sub
        Public Function OpenDisk(DiskName As String) As FatFileSystem

            If File.Exists(DiskName) Then
                Dim Thing As String = "Opening disk with name: " & DiskName
                Console.WriteLine(Thing) ' TODO: Replace this with proper logging
                Disk = Vhdx.Disk.OpenDisk(DiskName, FileAccess.ReadWrite)
                For Each part In Disk.Partitions.Partitions
                    Console.WriteLine("Partition: " & part.ToString())
                Next
                Root = New FatFileSystem(Disk.Partitions.Partitions(0).Open())
            Else
                Throw New FileNotFoundException("Disk not found", DiskName)
            End If
            Return Root
        End Function
    End Class

End Namespace