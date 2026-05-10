Imports System.IO
Imports DiscUtils.Fat
Imports DiscUtils.Vhdx, DiscUtils.Complete

Public Class FSConnector

        Public Sub InitDrive(ByRef partition As FatFileSystem)
            Dim DefaulFolders = {"\bin", "\sys", "\tmp", "\dev", "\proc", "\home", "\usr", "\etc"}
            For Each folder In DefaulFolders
                partition.CreateDirectory(folder)
            Next
            WriteEtcMotd(partition)
        End Sub
        Public Sub WriteEtcMotd(ByRef partition As FatFileSystem)
            Dim motdContent As String = "Welcome to Stardust OS!" & Environment.NewLine & "This is a custom operating system built on top of .NET 10.0." & Environment.NewLine & "Enjoy your stay!"
            Using stream As Stream = partition.OpenFile("etc\motd", FileMode.Create, FileAccess.Write)
                Using writer As New StreamWriter(stream)
                    writer.Write(motdContent)
                End Using
            End Using
        End Sub
    End Class
