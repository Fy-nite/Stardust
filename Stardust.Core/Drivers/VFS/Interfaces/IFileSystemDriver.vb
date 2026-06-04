Imports System.IO

Namespace Drivers.VFS.Interfaces
    Public Interface IFileSystemDriver
        Function FileExists(path As String) As Boolean
        Function DirectoryExists(path As String) As Boolean
        Function GetFiles(path As String) As String()
        Function GetDirectories(path As String) As String()
        Function OpenFile(path As String, mode As FileMode, access As FileAccess) As Stream
        Sub CreateDirectory(path As String)
        Sub DeleteFile(path As String)
        Sub WriteAllBytes(path As String, bytes As Byte())
        Function ReadAllBytes(path As String) As Byte()
    End Interface
End Namespace
