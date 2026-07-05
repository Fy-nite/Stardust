Imports javax.swing
Imports javax.swing.plaf

Public Module PicoTronTheme
    Private _installed As Boolean

    Public Sub Install()
        If _installed Then Return
        _installed = True

        ' Custom UI delegates are temporarily disabled because IKVM cannot
        ' resolve .NET types through the Swing UIDefaults mechanism
        ' (UIDefaults.getUI() uses Class.forName() which fails on .NET types,
        ' and LazyValue/ActiveValue implementations also don't bridge properly).
        ' Swing's Metal look-and-feel defaults are used instead.
        '
        ' TODO: Revisit when IKVM interop supports UIDefaults.LazyValue properly.
    End Sub
End Module
