Imports javax.swing
Imports javax.swing.plaf

''' <summary>
''' LazyValue that returns a shared singleton PicoTronButtonUI.
''' UIManager stores this instead of a class-name string, avoiding Java
''' Class.forName() which cannot resolve IKVM/.NET types.
''' </summary>
Public Class PicoTronButtonUILazyValue
    Implements UIDefaults.LazyValue

    Private Shared ReadOnly _instance As New PicoTronButtonUI()

    Public Function createValue(table As UIDefaults) As Object Implements UIDefaults.LazyValue.createValue
        Return _instance
    End Function
End Class

''' <summary>
''' LazyValue that returns a shared singleton PicoTronPanelUI.
''' </summary>
Public Class PicoTronPanelUILazyValue
    Implements UIDefaults.LazyValue

    Private Shared ReadOnly _instance As New PicoTronPanelUI()

    Public Function createValue(table As UIDefaults) As Object Implements UIDefaults.LazyValue.createValue
        Return _instance
    End Function
End Class

''' <summary>
''' ActiveValue that creates a fresh PicoTronInternalFrameUI on every call.
''' BasicInternalFrameUI.installUI(JComponent) sets the frame reference from
''' the component parameter, so passing Nothing to the constructor is safe.
''' An ActiveValue (not LazyValue) is required here because each JInternalFrame
''' needs its own UI instance (the UI holds per-frame state).
''' </summary>
Public Class PicoTronInternalFrameUIActiveValue
    Implements UIDefaults.ActiveValue

    Public Function createValue(table As UIDefaults) As Object Implements UIDefaults.ActiveValue.createValue
        Return New PicoTronInternalFrameUI(Nothing)
    End Function
End Class
