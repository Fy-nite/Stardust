Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class Panel
    Inherits Widget

    Public Property LayoutDirection As LayoutMode = LayoutMode.None
    Public Property ItemSpacing As Integer = 4

    Public Enum LayoutMode
        None
        Vertical
        Horizontal
    End Enum

    Public Sub New()
    End Sub

    Public Sub New(backgroundColor As Color)
        Me.BackgroundColor = backgroundColor
    End Sub

    Public Sub New(backgroundColor As Color, borderColor As Color)
        Me.BackgroundColor = backgroundColor
        Me.BorderColor = borderColor
    End Sub

    Public Overrides Sub Update(gameTime As GameTime)
        If Not Visible Then Return
        PerformLayout()
        MyBase.Update(gameTime)
    End Sub

    Private Sub PerformLayout()
        If LayoutDirection = LayoutMode.None Then Return

        Dim cx = Padding.Left
        Dim cy = Padding.Top

        For Each child In Children
            child.X = cx
            child.Y = cy

            If LayoutDirection = LayoutMode.Vertical Then
                cy += child.Height + ItemSpacing
            Else
                cx += child.Width + ItemSpacing
            End If
        Next
    End Sub
End Class
