Imports System.Collections.Generic
Public Class RegistrySystem(Of T)
    Private _Registry As List(Of RegistryService)
    Public Function GetService(Of T)() As RegistryService
    End Function
End Class
