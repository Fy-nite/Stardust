Imports System.Text.Json
Imports Stardust.Core

Namespace Apps

    ''' <summary>
    ''' Describes an installed application bundle (.app) found on the file system.
    ''' A bundle is a directory named Name.app containing a manifest.json that
    ''' optionally carries metadata about the app (macOS .app-style).
    ''' </summary>
    Public Class AppBundleInfo
        Public Property BundleId As String = ""
        Public Property Name As String = ""
        Public Property Version As String = ""
        Public Property Description As String = ""
        Public Property Launcher As String = ""
        Public Property Icon As String = ""
        Public Property Category As String = ""
        Public Property Hidden As Boolean = False
        Public Property Path As String = ""
        Public Property IsValid As Boolean = False
    End Class

    ''' <summary>
    ''' Scans the file system for .app bundles and reads their manifest metadata.
    ''' </summary>
    Public Module AppBundleLoader

        ''' <summary>
        ''' Scans the given virtual directory for *.app bundles and returns their metadata.
        ''' Bundles without a manifest, or invalid manifests, are returned as invalid entries.
        ''' </summary>
        Public Function ScanApps(virtualDir As String) As List(Of AppBundleInfo)
            Dim result As New List(Of AppBundleInfo)
            Dim normDir = If(String.IsNullOrEmpty(virtualDir), "/", virtualDir)

            If Not FS.DirectoryExists(normDir) Then Return result

            For Each dirName In FS.GetDirectories(normDir)
                Dim leaf = dirName.Split("/"c, "\"c).Last()
                If Not leaf.EndsWith(".app", StringComparison.OrdinalIgnoreCase) Then Continue For

                Dim bundleDir = JoinPath(normDir, leaf)
                result.Add(ReadManifest(bundleDir, leaf))
            Next

            Return result
        End Function

        ''' <summary>
        ''' Reads manifest.json from a single .app bundle directory.
        ''' </summary>
        Public Function ReadManifest(bundleDir As String, Optional displayName As String = "") As AppBundleInfo
            Dim info As New AppBundleInfo
            info.Path = bundleDir
            info.Name = If(displayName, PathName(bundleDir))
            info.Name = info.Name.Replace(".app", "")

            Dim manifestPath = JoinPath(bundleDir, "manifest.json")
            If Not FS.FileExists(manifestPath) Then Return info ' invalid, no manifest

            Try
                Dim json = FS.ReadAllText(manifestPath)
                Dim doc = System.Text.Json.JsonDocument.Parse(json)
                Dim root = doc.RootElement
                Dim v As JsonElement

                If root.TryGetProperty("bundleId", v) Then info.BundleId = v.GetString()
                If root.TryGetProperty("name", v) Then info.Name = v.GetString()
                If root.TryGetProperty("version", v) Then info.Version = v.GetString()
                If root.TryGetProperty("description", v) Then info.Description = v.GetString()
                If root.TryGetProperty("launcher", v) Then info.Launcher = v.GetString()
                If root.TryGetProperty("icon", v) Then info.Icon = v.GetString()
                If root.TryGetProperty("category", v) Then info.Category = v.GetString()
                If root.TryGetProperty("hidden", v) Then info.Hidden = TryGetBool(v)

                info.IsValid = Not String.IsNullOrEmpty(info.Name)
                Return info
            Catch
                Return info ' invalid manifest
            End Try
        End Function

        Private Function TryGetBool(el As JsonElement) As Boolean
            If el.ValueKind = JsonValueKind.True Then Return True
            If el.ValueKind = JsonValueKind.False Then Return False
            Dim s = el.GetString()
            Return If(s, "").Equals("true", StringComparison.OrdinalIgnoreCase) OrElse If(s, "").Equals("1", StringComparison.OrdinalIgnoreCase)
        End Function

        Private Function PathName(full As String) As String
            Return full.Split("/"c, "\"c).Last()
        End Function

        Private Function JoinPath(left As String, right As String) As String
            left = left.TrimEnd("/"c, "\"c)
            right = right.TrimStart("/"c, "\"c)
            Return left & "\" & right
        End Function
    End Module

End Namespace
