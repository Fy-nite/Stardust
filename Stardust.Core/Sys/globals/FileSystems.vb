Imports System.IO
Imports Stardust.Core.Drivers.VFS.Interfaces
Imports Stardust.Core.Drivers.VFS.FileSystem

Public Module FS
    Private ReadOnly Mounts As New Dictionary(Of String, IFileSystemDriver)(StringComparer.OrdinalIgnoreCase)

    Sub New()
        ' Try to find RootFS in the current directory or parent directories
        Dim currentPath = AppDomain.CurrentDomain.BaseDirectory
        Dim rootDir = ""
        
        ' Look up to 4 levels up for the RootFS folder (common in dev environments)
        For i As Integer = 0 To 4
            Dim potentialPath = Path.Combine(currentPath, "RootFS")
            If Directory.Exists(potentialPath) Then
                rootDir = potentialPath
                Exit For
            End If
            currentPath = Path.GetDirectoryName(currentPath)
            If currentPath Is Nothing Then Exit For
        Next

        If String.IsNullOrEmpty(rootDir) Then
            ' Fallback to creating a local RootFS if not found
            rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RootFS")
        End If

        Mount("/", New FolderDriver(rootDir))
    End Sub

    Public Sub Mount(mountPoint As String, driver As IFileSystemDriver)
        mountPoint = NormalisePath(mountPoint)
        If Not mountPoint.StartsWith("\") Then mountPoint = "\" & mountPoint
        Mounts(mountPoint) = driver
    End Sub

    Private Function NormalisePath(path As String) As String
        Dim result = path.Replace("/", "\").TrimEnd("\"c)
        Console.WriteLine($"[VFS] Normalised {path} -> {result}")
        Return result
    End Function

    Private Function ResolvePath(path As String, ByRef outRelativePath As String) As IFileSystemDriver
        Dim normPath = NormalisePath(path)
        If String.IsNullOrEmpty(normPath) Then normPath = "\"

        ' Find the longest matching mount point
        Dim bestMatch = Mounts.Keys.Where(Function(k) normPath.StartsWith(k, StringComparison.OrdinalIgnoreCase)).
                                    OrderByDescending(Function(k) k.Length).
                                    FirstOrDefault()

        If bestMatch IsNot Nothing Then
            outRelativePath = normPath.Substring(bestMatch.Length)
            If Not outRelativePath.StartsWith("\") Then outRelativePath = "\" & outRelativePath
            
            Console.WriteLine($"[VFS] Resolved {path} -> {bestMatch}:{outRelativePath}")
            Return Mounts(bestMatch)
        End If

        Console.WriteLine($"[VFS] Failed to resolve {path}")
        Return Nothing
    End Function

    Public Function ReadAllText(path As String) As String
        Dim relPath As String = ""
        Dim driver = ResolvePath(path, relPath)
        If driver Is Nothing Then Throw New DirectoryNotFoundException($"No mount point found for {path}")

        Dim bytes = driver.ReadAllBytes(relPath)
        Return System.Text.Encoding.UTF8.GetString(bytes)
    End Function

    Public Sub WriteAllText(path As String, contents As String)
        Dim relPath As String = ""
        Dim driver = ResolvePath(path, relPath)
        If driver Is Nothing Then Throw New DirectoryNotFoundException($"No mount point found for {path}")

        Dim bytes = System.Text.Encoding.UTF8.GetBytes(contents)
        driver.WriteAllBytes(relPath, bytes)
    End Sub

    Public Function FileExists(path As String) As Boolean
        Dim relPath As String = ""
        Dim driver = ResolvePath(path, relPath)
        Return If(driver?.FileExists(relPath), False)
    End Function

    Public Function DirectoryExists(path As String) As Boolean
        Dim relPath As String = ""
        Dim driver = ResolvePath(path, relPath)
        Return If(driver?.DirectoryExists(relPath), False)
    End Function

    Public Function GetFiles(path As String) As String()
        Dim relPath As String = ""
        Dim driver = ResolvePath(path, relPath)
        Return If(driver?.GetFiles(relPath), Array.Empty(Of String))
    End Function

    ' Helper to bridge old code: ExtractToTemp
    Public Function ExtractToTemp(virtualPath As String) As String
        Dim bytes = ReadAllBytes(virtualPath)
        Dim tempPath = IO.Path.Combine(IO.Path.GetTempPath(), "stardust", IO.Path.GetFileName(virtualPath))
        IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(tempPath))
        IO.File.WriteAllBytes(tempPath, bytes)
        Return tempPath
    End Function

    Public Function ReadAllBytes(path As String) As Byte()
        Dim relPath As String = ""
        Dim driver = ResolvePath(path, relPath)
        Return If(driver?.ReadAllBytes(relPath), Array.Empty(Of Byte))
    End Function

End Module
