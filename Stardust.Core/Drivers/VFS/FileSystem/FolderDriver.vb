Imports System.IO
Imports Stardust.Core.Drivers.VFS.Interfaces

Namespace Drivers.VFS.FileSystem
    Public Class FolderDriver
        Implements IFileSystemDriver

        Private ReadOnly _rootPath As String

        Public Sub New(rootPath As String)
            _rootPath = Path.GetFullPath(rootPath)
            If Not Directory.Exists(_rootPath) Then
                Directory.CreateDirectory(_rootPath)
            End If
        End Sub

        Private Function MapPath(path As String) As String
            ' Remove leading slash/backslash
            Dim cleanPath = path.TrimStart("/"c, "\"c)
            ' Virtual paths use backslashes; convert to the OS separator
            cleanPath = cleanPath.Replace("/"c, IO.Path.DirectorySeparatorChar).
                                   Replace("\"c, IO.Path.DirectorySeparatorChar)
            Return IO.Path.Combine(_rootPath, cleanPath)
        End Function

        Public Function FileExists(path As String) As Boolean Implements IFileSystemDriver.FileExists
            Return File.Exists(MapPath(path))
        End Function

        Public Function DirectoryExists(path As String) As Boolean Implements IFileSystemDriver.DirectoryExists
            Return Directory.Exists(MapPath(path))
        End Function

        Public Function GetFiles(path As String) As String() Implements IFileSystemDriver.GetFiles
            Return Directory.GetFiles(MapPath(path))
        End Function

        Public Function GetDirectories(path As String) As String() Implements IFileSystemDriver.GetDirectories
            Return Directory.GetDirectories(MapPath(path))
        End Function

        Public Function OpenFile(path As String, mode As FileMode, access As FileAccess) As Stream Implements IFileSystemDriver.OpenFile
            Return New FileStream(MapPath(path), mode, access)
        End Function

        Public Sub CreateDirectory(path As String) Implements IFileSystemDriver.CreateDirectory
            Directory.CreateDirectory(MapPath(path))
        End Sub

        Public Sub DeleteFile(path As String) Implements IFileSystemDriver.DeleteFile
            File.Delete(MapPath(path))
        End Sub

        Public Sub WriteAllBytes(path As String, bytes As Byte()) Implements IFileSystemDriver.WriteAllBytes
            File.WriteAllBytes(MapPath(path), bytes)
        End Sub

        Public Function ReadAllBytes(path As String) As Byte() Implements IFileSystemDriver.ReadAllBytes
            Return File.ReadAllBytes(MapPath(path))
        End Function
    End Class
End Namespace
