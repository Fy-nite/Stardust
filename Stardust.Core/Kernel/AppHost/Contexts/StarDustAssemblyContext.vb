Imports System.Reflection
Imports System.Runtime.Loader
Namespace AppHost.Contexts
    Public Class StardustLoadContext
        Inherits AssemblyLoadContext

        Private ReadOnly _resolver As AssemblyDependencyResolver

        Public Sub New(appPath As String)
            MyBase.New(isCollectible:=True)
            _resolver = New AssemblyDependencyResolver(appPath)
        End Sub

        Protected Overrides Function Load(assemblyName As AssemblyName) As Assembly
            Dim path = _resolver.ResolveAssemblyToPath(assemblyName)
            If path IsNot Nothing Then
                Return LoadFromAssemblyPath(path)
            End If
            Return Nothing ' fall back to default context (Stardust's own assemblies)
        End Function

    End Class
End Namespace