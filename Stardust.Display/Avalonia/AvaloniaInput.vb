Imports Avalonia.Input
Imports Microsoft.Xna.Framework.Input

''' Maps MonoGame keyboard/mouse state onto Avalonia's input enums so the
''' display server can forward real hardware events into an offscreen surface.
Friend Module AvaloniaInput

    ''' MonoGame Keys -> Avalonia logical Key.
    Public Function MapKey(mgKey As Keys) As Key
        Select Case mgKey
            Case Keys.Back : Return Key.Back
            Case Keys.Tab : Return Key.Tab
            Case Keys.Enter : Return Key.Enter
            Case Keys.Pause : Return Key.Pause
            Case Keys.CapsLock : Return Key.CapsLock
            Case Keys.Escape : Return Key.Escape
            Case Keys.Space : Return Key.Space
            Case Keys.PageUp : Return Key.PageUp
            Case Keys.PageDown : Return Key.PageDown
            Case Keys.End : Return Key.End
            Case Keys.Home : Return Key.Home
            Case Keys.Left : Return Key.Left
            Case Keys.Up : Return Key.Up
            Case Keys.Right : Return Key.Right
            Case Keys.Down : Return Key.Down
            Case Keys.Insert : Return Key.Insert
            Case Keys.Delete : Return Key.Delete

            Case Keys.D0 To Keys.D9 : Return CType(Key.D0 + (mgKey - Keys.D0), Key)
            Case Keys.A To Keys.Z : Return CType(Key.A + (mgKey - Keys.A), Key)

            Case Keys.NumPad0 To Keys.NumPad9 : Return CType(Key.NumPad0 + (mgKey - Keys.NumPad0), Key)
            Case Keys.Multiply : Return Key.Multiply
            Case Keys.Add : Return Key.Add
            Case Keys.Subtract : Return Key.Subtract
            Case Keys.Decimal : Return Key.Decimal
            Case Keys.Divide : Return Key.Divide

            Case Keys.F1 To Keys.F24 : Return CType(Key.F1 + (mgKey - Keys.F1), Key)

            Case Keys.NumLock : Return Key.NumLock
            Case Keys.Scroll : Return Key.Scroll
            Case Keys.LeftShift : Return Key.LeftShift
            Case Keys.RightShift : Return Key.RightShift
            Case Keys.LeftControl : Return Key.LeftCtrl
            Case Keys.RightControl : Return Key.RightCtrl
            Case Keys.LeftAlt : Return Key.LeftAlt
            Case Keys.RightAlt : Return Key.RightAlt

            Case Keys.OemSemicolon : Return Key.OemSemicolon
            Case Keys.OemPlus : Return Key.OemPlus
            Case Keys.OemComma : Return Key.OemComma
            Case Keys.OemMinus : Return Key.OemMinus
            Case Keys.OemPeriod : Return Key.OemPeriod
            Case Keys.OemQuestion : Return Key.OemQuestion
            Case Keys.OemTilde : Return Key.OemTilde
            Case Keys.OemOpenBrackets : Return Key.OemOpenBrackets
            Case Keys.OemPipe : Return Key.OemPipe
            Case Keys.OemCloseBrackets : Return Key.OemCloseBrackets
            Case Keys.OemQuotes : Return Key.OemQuotes
            Case Keys.Oem8 : Return Key.Oem8
            Case Keys.OemBackslash : Return Key.OemBackslash

            Case Else : Return Key.None
        End Select
    End Function

    ''' MonoGame Keys -> Avalonia PhysicalKey (physical layout position).
    Public Function MapPhysicalKey(mgKey As Keys) As PhysicalKey
        Select Case mgKey
            Case Keys.Escape : Return PhysicalKey.Escape
            Case Keys.Back : Return PhysicalKey.Backspace
            Case Keys.Tab : Return PhysicalKey.Tab
            Case Keys.Enter : Return PhysicalKey.Enter
            Case Keys.CapsLock : Return PhysicalKey.CapsLock
            Case Keys.Space : Return PhysicalKey.Space
            Case Keys.PageUp : Return PhysicalKey.PageUp
            Case Keys.PageDown : Return PhysicalKey.PageDown
            Case Keys.End : Return PhysicalKey.End
            Case Keys.Home : Return PhysicalKey.Home
            Case Keys.Left : Return PhysicalKey.ArrowLeft
            Case Keys.Up : Return PhysicalKey.ArrowUp
            Case Keys.Right : Return PhysicalKey.ArrowRight
            Case Keys.Down : Return PhysicalKey.ArrowDown
            Case Keys.Insert : Return PhysicalKey.Insert
            Case Keys.Delete : Return PhysicalKey.Delete

            Case Keys.D0 To Keys.D9 : Return CType(PhysicalKey.Digit0 + (mgKey - Keys.D0), PhysicalKey)
            Case Keys.A To Keys.Z : Return CType(PhysicalKey.A + (mgKey - Keys.A), PhysicalKey)

            Case Keys.NumPad0 To Keys.NumPad9 : Return CType(PhysicalKey.NumPad0 + (mgKey - Keys.NumPad0), PhysicalKey)
            Case Keys.Multiply : Return PhysicalKey.NumPadMultiply
            Case Keys.Add : Return PhysicalKey.NumPadAdd
            Case Keys.Subtract : Return PhysicalKey.NumPadSubtract
            Case Keys.Decimal : Return PhysicalKey.NumPadDecimal
            Case Keys.Divide : Return PhysicalKey.NumPadDivide

            Case Keys.F1 To Keys.F24 : Return CType(PhysicalKey.F1 + (mgKey - Keys.F1), PhysicalKey)

            Case Keys.NumLock : Return PhysicalKey.NumLock
            Case Keys.Scroll : Return PhysicalKey.ScrollLock
            Case Keys.LeftShift : Return PhysicalKey.ShiftLeft
            Case Keys.RightShift : Return PhysicalKey.ShiftRight
            Case Keys.LeftControl : Return PhysicalKey.ControlLeft
            Case Keys.RightControl : Return PhysicalKey.ControlRight
            Case Keys.LeftAlt : Return PhysicalKey.AltLeft
            Case Keys.RightAlt : Return PhysicalKey.AltRight
            Case Keys.Apps : Return PhysicalKey.ContextMenu

            Case Keys.OemSemicolon : Return PhysicalKey.Semicolon
            Case Keys.OemPlus : Return PhysicalKey.Equal
            Case Keys.OemComma : Return PhysicalKey.Comma
            Case Keys.OemMinus : Return PhysicalKey.Minus
            Case Keys.OemPeriod : Return PhysicalKey.Period
            Case Keys.OemQuestion : Return PhysicalKey.Slash
            Case Keys.OemTilde : Return PhysicalKey.Backquote
            Case Keys.OemOpenBrackets : Return PhysicalKey.BracketLeft
            Case Keys.OemPipe : Return PhysicalKey.Backslash
            Case Keys.OemCloseBrackets : Return PhysicalKey.BracketRight
            Case Keys.OemQuotes : Return PhysicalKey.Quote

            Case Else : Return PhysicalKey.None
        End Select
    End Function

    ''' MonoGame MouseButton state -> Avalonia MouseButton for a single press.
    Public Function MapButton(leftPressed As Boolean, rightPressed As Boolean, middlePressed As Boolean) As MouseButton
        If rightPressed Then Return MouseButton.Right
        If middlePressed Then Return MouseButton.Middle
        Return MouseButton.Left
    End Function
End Module