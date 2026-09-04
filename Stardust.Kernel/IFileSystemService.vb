''' <summary>
''' Abstraction over the file system that the process/loader layer needs.
''' Implemented by the host (Stardust.Core's FS module) so this kernel library
''' stays free of concrete file-system dependencies.
''' </summary>
Public Interface IFileSystemService
    Function ReadAllText(path As String) As String
    Function FileExists(path As String) As Boolean
    Function ExtractToTemp(virtualPath As String) As String
End Interface
