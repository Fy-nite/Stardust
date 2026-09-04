Imports Stardust.Display

Module Program
    Sub Main(args As String())
        ' Standalone launcher for the Stardust Display Server.
        ' Programs may alternatively reference Stardust.Display as a library
        ' and drive DisplayServer.Instance directly.
        DisplayServer.Instance.RunServer(Sub()
            Console.WriteLine("[Stardust.Display] Display server started (no startup apps configured in standalone mode).")
        End Sub)
    End Sub
End Module
