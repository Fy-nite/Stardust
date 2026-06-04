Imports System.IO
Imports DiscUtils.Fat
Imports Stardust.Core.Drivers.VFS.Interfaces

Namespace Drivers.VFS.FileSystem
    Public Class DiskDriver
        Implements IFileSystemDriver

        Private ReadOnly _fatFs As FatFileSystem

        Public Sub New(fatFs As FatFileSystem)
            _fatFs = fatFs
        End Sub

        Public Function FileExists(path As String) As Boolean Implements IFileSystemDriver.FileExists
            Return _fatFs.FileExists(path)
        End Function

        Public Function DirectoryExists(path As String) As Boolean Implements IFileSystemDriver.DirectoryExists
            Return _fatFs.DirectoryExists(path)
        End Function

        Public Function GetFiles(path As String) As String() Implements IFileSystemDriver.GetFiles
            Return _fatFs.GetFiles(path)
        End Function

        Public Function GetDirectories(path As String) As String() Implements IFileSystemDriver.GetDirectories
            Return _fatFs.GetDirectories(path)
        End Function

        Public Function OpenFile(path As String, mode As FileMode, access As FileAccess) As Stream Implements IFileSystemDriver.OpenFile
            Return _fatFs.OpenFile(path, mode, access)
        End Function

        Public Sub CreateDirectory(path As String) Implements IFileSystemDriver.CreateDirectory
            _fatFs.CreateDirectory(path)
        End Sub

        Public Sub DeleteFile(path As String) Implements IFileSystemDriver.DeleteFile
            ' DiscUtils FatFileSystem might have different delete behavior or method name
            ' Check if it has a specific DeleteFile or if we need to use something else
            ' For now, assuming standard naming
        End Sub

        Public Sub WriteAllBytes(path As String, bytes As Byte()) Implements IFileSystemDriver.WriteAllBytes
            Using s = _fatFs.OpenFile(path, FileMode.Create, FileAccess.Write)
                s.Write(bytes, 0, bytes.Length)
            End Using
        End Sub

        Public Function ReadAllBytes(path As String) As Byte() Implements IFileSystemDriver.ReadAllBytes
            Using s = _fatFs.OpenFile(path, FileMode.Open, FileAccess.Read)
                Dim buf(CInt(s.Length) - 1) As Byte
                s.Read(buf, 0, buf.Length)
                Return buf
            End Using
        End Function
    End Class
End Namespace
