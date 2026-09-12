Imports Stardust.Display

''' <summary>
''' Adapter for buttons. Subclasses the real Stardust Button so it renders and
''' behaves like every other button, and forwards press events to the owning
''' Java facade object via the java_onClick method.
''' </summary>
Public Class JavaButton
    Inherits Button

    Private _owner As Object

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(text As String)
        MyBase.New(text)
    End Sub

    Public Sub SetOwner(owner As Object)
        _owner = owner
    End Sub

    Public Overrides Sub OnMouseClick(localX As Integer, localY As Integer)
        MyBase.OnMouseClick(localX, localY)
        JavaCall.Dispatch(_owner, "java_onClick")
    End Sub

End Class