Imports System.Data
Imports System.IO
Imports DiscUtils.Fat
Imports DiscUtils.Vhdx
Imports Stardust.Core.Drivers.VFS.FileSystem
Imports Stardust.Core
Imports Stardust.Core.AppHost
Imports Stardust.FileSystem.BaseFS

Public Module FS
    Public Property Disk As VHDXDriver
    Public Property Root As FatFileSystem

    Sub New()
        Dim DiskSize As Long = 2048L * 1024 * 1024
        Disk = New VHDXDriver()
        If Not File.Exists("Stardust.vhdx") Then
            Console.WriteLine("Stardust.vhdx not found, creating new disk.")
            Disk.CreateDisk("Stardust.vhdx", DiskSize)
            Dim FSConnect As New FSConnector()
            FSConnect.InitDrive(Disk.Root)
        Else
            Disk.OpenDisk("Stardust.vhdx")
        End If
        Root = Disk.Root
    End Sub

    Private Function NormalisePath(path As String) As String
        If path.StartsWith("/") OrElse path.StartsWith("\") Then
            path = path.Substring(1)
        End If
        Return path.Replace("/", "\")
    End Function

    Public Function ReadAllText(path As String) As String
        path = NormalisePath(path)
        Dim f = Root.OpenFile(path, IO.FileMode.OpenOrCreate)
        Dim buffer(CInt(f.Length) - 1) As Byte
        f.Read(buffer, 0, buffer.Length)
        Return System.Text.Encoding.UTF8.GetString(buffer)
    End Function
    Public Function ExtractToTemp(virtualPath As String) As String
        Dim bytes = ReadAllBytes(virtualPath)
        Dim tempPath = IO.Path.Combine(IO.Path.GetTempPath(), "stardust", IO.Path.GetFileName(virtualPath))
        IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(tempPath))
        IO.File.WriteAllBytes(tempPath, bytes)
        Return tempPath
    End Function

    Public Function ReadAllBytes(path As String) As Byte()
        path = NormalisePath(path)
        Dim f = Root.OpenFile(path, IO.FileMode.Open)
        Dim buffer(CInt(f.Length) - 1) As Byte
        f.Read(buffer, 0, buffer.Length)
        Return buffer
    End Function
    Public Function ExtractDirectoryToTemp(virtualDir As String) As String
        Dim tempDir = IO.Path.Combine(IO.Path.GetTempPath(), "stardust", IO.Path.GetFileName(virtualDir))
        IO.Directory.CreateDirectory(tempDir)
        For Each file In FS.GetFiles(virtualDir)
            Dim fileName = IO.Path.GetFileName(file)
            IO.File.WriteAllBytes(IO.Path.Combine(tempDir, fileName), FS.ReadAllBytes(file))
        Next
        Return IO.Path.Combine(tempDir, IO.Path.GetFileName(virtualDir))
    End Function
    Public Sub WriteAllText(path As String, contents As String)
        path = NormalisePath(path)
        Dim buffer = System.Text.Encoding.UTF8.GetBytes(contents)
        Using f = Root.OpenFile(path, IO.FileMode.Create)
            f.Write(buffer, 0, buffer.Length)
        End Using
    End Sub

    Public Function FileExists(path As String) As Boolean
        If Root Is Nothing Then
            Throw New NoNullAllowedException("Drive must not be null to read files")
        End If
        Return Root.FileExists(NormalisePath(path))
    End Function

    Public Function DirectoryExists(path As String) As Boolean
        If Root Is Nothing Then
            Throw New NoNullAllowedException("Drive must not be null to read files")
        End If
        Return Root.DirectoryExists(NormalisePath(path))
    End Function

    Public Function GetFiles(path As String) As String()
        Return Root.GetFiles(NormalisePath(path))
    End Function

    Public Function GetDirectories(path As String) As String()
        Return Root.GetDirectories(NormalisePath(path))
    End Function

End Module