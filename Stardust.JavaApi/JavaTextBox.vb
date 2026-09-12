Imports Stardust.Display

''' <summary>
''' Adapter for text boxes. Subclasses the real Stardust TextBox so it renders
''' and edits like every other TextBox, and forwards text change / submit events
''' to the owning Java facade object via java_onTextChanged / java_onSubmit.
''' </summary>
Public Class JavaTextBox
    Inherits TextBox

    Private _owner As Object

    Public Sub New()
        MyBase.New()
        AddHandler OnTextChanged, AddressOf OnTextChangedRelay
        AddHandler OnSubmit, AddressOf OnSubmitRelay
    End Sub

    Public Sub New(width As Integer)
        MyBase.New(width)
        AddHandler OnTextChanged, AddressOf OnTextChangedRelay
        AddHandler OnSubmit, AddressOf OnSubmitRelay
    End Sub

    Public Sub SetOwner(owner As Object)
        _owner = owner
    End Sub

    Private Sub OnTextChangedRelay(sender As Object, e As EventArgs)
        JavaCall.Dispatch(_owner, "java_onTextChanged", Text)
    End Sub

    Private Sub OnSubmitRelay(sender As Object, e As EventArgs)
        JavaCall.Dispatch(_owner, "java_onSubmit", Text)
    End Sub

End Class