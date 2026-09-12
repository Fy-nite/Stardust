Imports Stardust.Display
Imports Stardust.JavaApi

''' <summary>
''' The base adapter widget. Subclasses the real Stardust TextBox so the widget
''' renders exactly like every other TextBox, and forwards user input back to
''' the owning Java facade object via JavaCall, which reflects onto the
''' java_onXxx(...) methods the Java class implements.
''' </summary>
Public Class JavaWidget
    Inherits Widget

    Private _owner As Object

    Public Sub SetOwner(owner As Object)
        _owner = owner
    End Sub

    Public Sub RemoveOwner()
        _owner = Nothing
    End Sub

    Public Overrides Sub OnMouseClick(localX As Integer, localY As Integer)
        MyBase.OnMouseClick(localX, localY)
        JavaCall.Dispatch(_owner, "java_onMouseClick", localX, localY)
    End Sub

    Public Overrides Sub OnMouseDoubleClick(localX As Integer, localY As Integer)
        MyBase.OnMouseDoubleClick(localX, localY)
        JavaCall.Dispatch(_owner, "java_onMouseDoubleClick", localX, localY)
    End Sub

    Public Overrides Sub OnMouseEnter()
        MyBase.OnMouseEnter()
        JavaCall.Dispatch(_owner, "java_onMouseEnter")
    End Sub

    Public Overrides Sub OnMouseLeave()
        MyBase.OnMouseLeave()
        JavaCall.Dispatch(_owner, "java_onMouseLeave")
    End Sub

    Public Overrides Sub OnMouseScroll(delta As Integer)
        MyBase.OnMouseScroll(delta)
        JavaCall.Dispatch(_owner, "java_onMouseScroll", delta)
    End Sub

    Public Overrides Sub OnKeyPress(keyInfo As ConsoleKeyInfo)
        MyBase.OnKeyPress(keyInfo)
        JavaCall.Dispatch(_owner, "java_onKeyPress",
                          keyInfo.KeyChar,
                          CInt(keyInfo.Key),
                          (keyInfo.Modifiers And ConsoleModifiers.Shift) <> 0)
    End Sub

End Class