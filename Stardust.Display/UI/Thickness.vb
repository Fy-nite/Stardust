Public Structure Thickness
    Public Property Left As Integer
    Public Property Top As Integer
    Public Property Right As Integer
    Public Property Bottom As Integer

    Public Sub New(uniform As Integer)
        Left = uniform
        Top = uniform
        Right = uniform
        Bottom = uniform
    End Sub

    Public Sub New(horizontal As Integer, vertical As Integer)
        Left = horizontal
        Top = vertical
        Right = horizontal
        Bottom = vertical
    End Sub

    Public Sub New(left As Integer, top As Integer, right As Integer, bottom As Integer)
        Me.Left = left
        Me.Top = top
        Me.Right = right
        Me.Bottom = bottom
    End Sub

    Public ReadOnly Property Horizontal As Integer
        Get
            Return Left + Right
        End Get
    End Property

    Public ReadOnly Property Vertical As Integer
        Get
            Return Top + Bottom
        End Get
    End Property

    Public Shared ReadOnly Property Zero As Thickness
        Get
            Return New Thickness(0)
        End Get
    End Property
End Structure
