Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class IconRenderer

    Public Enum IconType
        None
        Close
        Minimize
        Maximize
        Restore
        Back
        Forward
        Refresh
        Search
        Settings
        Folder
        File
        Terminal
        Browser
        Warning
        ErrorIcon
        Info
        Success
        Play
        Pause
        StopIcon
        Add
        Remove
        Edit
        Delete
        Save
        Open
        Copy
        Paste
        Undo
        Redo
        Home
        User
        Heart
        Star
        Check
        Cross
        ArrowUp
        ArrowDown
        ArrowLeft
        ArrowRight
        Menu
        Eye
        LockIcon
        Unlock
        Download
        Upload
        Mail
        Calendar
        Clock
        Image
        Music
        Video
        Code
        Link
        Tag
    End Enum

    Public Shared Sub Draw(batch As SpriteBatch, icon As IconType, x As Integer, y As Integer, size As Integer, color As Color)
        If icon = IconType.None Then Return

        Dim px = DisplayServer.Instance.WhitePixel
        Dim s = size
        Dim hs = s \ 2
        Dim q = s \ 4
        Dim tq = s * 3 \ 4

        Select Case icon
            Case IconType.Close
                DrawX(batch, px, x, y, s, color)
            Case IconType.Minimize
                DrawLineH(batch, px, x + q, y + tq, s \ 2, color)
            Case IconType.Maximize
                DrawRectOutline(batch, px, x + q, y + q, s \ 2, s \ 2, color)
            Case IconType.Restore
                DrawRectOutline(batch, px, x + q + 2, y + q, s \ 2 - 2, s \ 2, color)
                DrawLineH(batch, px, x + q + 2, y + q, s \ 2 - 2, color)
                DrawRectOutline(batch, px, x + q, y + q + 2, s \ 2 - 2, s \ 2 - 2, color)
            Case IconType.Back
                DrawArrowLeft(batch, px, x, y, s, color)
            Case IconType.Forward
                DrawArrowRight(batch, px, x, y, s, color)
            Case IconType.Refresh
                DrawRefresh(batch, px, x, y, s, color)
            Case IconType.Search
                DrawSearch(batch, px, x, y, s, color)
            Case IconType.Settings
                DrawGear(batch, px, x, y, s, color)
            Case IconType.Folder
                DrawFolder(batch, px, x, y, s, color)
            Case IconType.File
                DrawFile(batch, px, x, y, s, color)
            Case IconType.Terminal
                DrawTerminal(batch, px, x, y, s, color)
            Case IconType.Browser
                DrawBrowser(batch, px, x, y, s, color)
            Case IconType.Warning
                DrawTriangle(batch, px, x, y, s, New Color(255, 180, 40))
            Case IconType.ErrorIcon
                DrawX(batch, px, x, y, s, New Color(220, 60, 60))
            Case IconType.Info
                DrawCircleOutline(batch, px, x + s \ 2, y + s \ 2, s * 3 \ 4, New Color(66, 135, 245))
                FillRectDirect(batch, px, x + s \ 2, y + s \ 3, 1, 1, New Color(66, 135, 245))
            Case IconType.Success
                DrawCheck(batch, px, x, y, s, New Color(66, 200, 100))
            Case IconType.Play
                DrawPlay(batch, px, x, y, s, color)
            Case IconType.Pause
                DrawPause(batch, px, x, y, s, color)
            Case IconType.StopIcon
                FillRectDirect(batch, px, x + q, y + q, s \ 2, s \ 2, color)
            Case IconType.Add
                DrawPlus(batch, px, x, y, s, color)
            Case IconType.Remove
                DrawLineH(batch, px, x + q, y + hs, s \ 2, color)
            Case IconType.Check
                DrawCheck(batch, px, x, y, s, color)
            Case IconType.Cross
                DrawX(batch, px, x, y, s, color)
            Case IconType.Home
                DrawHome(batch, px, x, y, s, color)
            Case IconType.Heart
                DrawHeart(batch, px, x, y, s, color)
            Case IconType.Star
                DrawStar(batch, px, x, y, s, color)
            Case IconType.ArrowUp
                DrawArrowUp(batch, px, x, y, s, color)
            Case IconType.ArrowDown
                DrawArrowDown(batch, px, x, y, s, color)
            Case IconType.ArrowLeft
                DrawArrowLeft(batch, px, x, y, s, color)
            Case IconType.ArrowRight
                DrawArrowRight(batch, px, x, y, s, color)
            Case IconType.Menu
                DrawMenu(batch, px, x, y, s, color)
            Case IconType.Eye
                DrawEye(batch, px, x, y, s, color)
            Case IconType.LockIcon
                DrawLock(batch, px, x, y, s, color)
            Case Else
                FillRectDirect(batch, px, x + q, y + q, s \ 2, s \ 2, color)
        End Select
    End Sub

    Private Shared Sub DrawLineH(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, w As Integer, color As Color)
        FillRectDirect(batch, px, x, y, w, 1, color)
    End Sub

    Private Shared Sub DrawLineV(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, h As Integer, color As Color)
        FillRectDirect(batch, px, x, y, 1, h, color)
    End Sub

    Private Shared Sub FillRectDirect(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, w As Integer, h As Integer, color As Color)
        If w <= 0 OrElse h <= 0 Then Return
        batch.Draw(px, New Rectangle(x, y, w, h), color)
    End Sub

    Private Shared Sub DrawRectOutline(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, w As Integer, h As Integer, color As Color)
        If w <= 0 OrElse h <= 0 Then Return
        DrawLineH(batch, px, x, y, w, color)
        DrawLineH(batch, px, x, y + h - 1, w, color)
        DrawLineV(batch, px, x, y, h, color)
        DrawLineV(batch, px, x + w - 1, y, h, color)
    End Sub

    Private Shared Sub DrawX(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim m = s \ 3
        For i = 0 To s - m * 2 - 1
            Dim t = i / CSng(Math.Max(1, s - m * 2 - 1))
            FillRectDirect(batch, px, x + m + CInt(t * (s - m * 2)), y + m + i, 1, 1, color)
            FillRectDirect(batch, px, x + s - m - 1 - CInt(t * (s - m * 2)), y + m + i, 1, 1, color)
        Next
    End Sub

    Private Shared Sub DrawCheck(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim q = s \ 4
        Dim pts = New Point() {
            New Point(q, s \ 2),
            New Point(q + 1, s \ 2 + 1),
            New Point(q + 2, s \ 2 + 2),
            New Point(s \ 2 - 1, tq(s) + 1),
            New Point(s \ 2, tq(s)),
            New Point(s \ 2 + 1, tq(s) - 1),
            New Point(s \ 2 + 2, tq(s) - 2)
        }
        For Each p In pts
            FillRectDirect(batch, px, x + p.X, y + p.Y, 1, 1, color)
        Next
    End Sub

    Private Shared Function tq(s As Integer) As Integer
        Return s \ 4
    End Function

    Private Shared Sub DrawTriangle(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        For row = 0 To s - 1
            Dim ratio = row / CSng(Math.Max(1, s - 1))
            Dim w = CInt(ratio * s)
            Dim startX = x + (s - w) \ 2
            If w > 0 Then FillRectDirect(batch, px, startX, y + row, w, 1, color)
        Next
    End Sub

    Private Shared Sub DrawArrowLeft(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim hs = s \ 2
        For i = 0 To hs
            FillRectDirect(batch, px, x + i, y + hs - i, 1, 1 + i * 2, color)
        Next
    End Sub

    Private Shared Sub DrawArrowRight(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim hs = s \ 2
        For i = 0 To hs
            FillRectDirect(batch, px, x + s - 1 - i, y + hs - i, 1, 1 + i * 2, color)
        Next
    End Sub

    Private Shared Sub DrawArrowUp(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim hs = s \ 2
        For i = 0 To hs
            FillRectDirect(batch, px, x + hs - i, y + i, 1 + i * 2, 1, color)
        Next
    End Sub

    Private Shared Sub DrawArrowDown(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim hs = s \ 2
        For i = 0 To hs
            FillRectDirect(batch, px, x + hs - i, y + s - 1 - i, 1 + i * 2, 1, color)
        Next
    End Sub

    Private Shared Sub DrawRefresh(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        DrawCircleOutline(batch, px, x, y, s, color)
        DrawLineH(batch, px, x + s \ 2, y, s \ 2, color)
        DrawArrowRight(batch, px, x + s - s \ 3, y + s \ 4, s \ 3, color)
    End Sub

    Private Shared Sub DrawSearch(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim r = s * 3 \ 8
        DrawCircleOutline(batch, px, x + s \ 3, y + s \ 3, r * 2, color)
        DrawLineH(batch, px, x + s \ 3 + r, y + s \ 3 + r, s \ 3, color)
        DrawLineV(batch, px, x + s \ 3 + r + s \ 3 - 1, y + s \ 3 + r, s \ 4, color)
    End Sub

    Private Shared Sub DrawCircleOutline(batch As SpriteBatch, px As Texture2D, cx As Integer, cy As Integer, diameter As Integer, color As Color)
        Dim r = diameter \ 2
        For angle = 0 To 359 Step 10
            Dim rad = angle * Math.PI / 180.0
            Dim px2 = cx + CInt(Math.Cos(rad) * r)
            Dim py2 = cy + CInt(Math.Sin(rad) * r)
            FillRectDirect(batch, px, px2, py2, 1, 1, color)
        Next
    End Sub

    Private Shared Sub DrawGear(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        DrawCircleOutline(batch, px, x + s \ 2, y + s \ 2, s \ 2, color)
        DrawCircleOutline(batch, px, x + s \ 2, y + s \ 2, s \ 3, color)
        For angle = 0 To 315 Step 45
            Dim rad = angle * Math.PI / 180.0
            Dim cx = x + s \ 2
            Dim cy = y + s \ 2
            Dim x1 = cx + CInt(Math.Cos(rad) * s \ 3)
            Dim y1 = cy + CInt(Math.Sin(rad) * s \ 3)
            FillRectDirect(batch, px, x1 - 1, y1 - 1, 3, 3, color)
        Next
    End Sub

    Private Shared Sub DrawFolder(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim w = s * 3 \ 4
        Dim h = s * 2 \ 3
        Dim fx = x + (s - w) \ 2
        Dim fy = y + (s - h) \ 2
        DrawRectOutline(batch, px, fx, fy + 2, w, h - 2, color)
        DrawLineH(batch, px, fx, fy + 2, w \ 3, color)
        FillRectDirect(batch, px, fx + 1, fy + 3, w - 2, 1, color)
    End Sub

    Private Shared Sub DrawFile(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim w = s \ 2
        Dim h = s - 2
        Dim fx = x + (s - w) \ 2
        Dim fy = y + 1
        DrawRectOutline(batch, px, fx, fy, w, h, color)
        DrawLineH(batch, px, fx + 2, fy + h \ 3, w - 4, color)
        DrawLineH(batch, px, fx + 2, fy + h * 2 \ 3, w - 4, color)
    End Sub

    Private Shared Sub DrawTerminal(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        DrawRectOutline(batch, px, x + 1, y + 1, s - 2, s - 2, color)
        DrawLineV(batch, px, x + s \ 3, y + s \ 4, s \ 2, color)
        DrawLineH(batch, px, x + s \ 3, y + s \ 2, s \ 4, color)
    End Sub

    Private Shared Sub DrawBrowser(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        DrawRectOutline(batch, px, x + 1, y + 1, s - 2, s - 2, color)
        DrawLineH(batch, px, x + 1, y + s \ 4, s - 2, color)
        For i = 0 To 2
            FillRectDirect(batch, px, x + 3 + i * 3, y + 3, 2, 2, color)
        Next
    End Sub

    Private Shared Sub DrawPlay(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        For row = 0 To s - 1
            Dim ratio = row / CSng(Math.Max(1, s - 1))
            Dim w = CInt(ratio * s \ 2)
            If w > 0 Then FillRectDirect(batch, px, x + s \ 4 + (s \ 4 - w \ 2), y + row, w, 1, color)
        Next
    End Sub

    Private Shared Sub DrawPause(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim gap = s \ 6
        FillRectDirect(batch, px, x + s \ 3, y + 2, gap, s - 4, color)
        FillRectDirect(batch, px, x + s * 2 \ 3 - gap, y + 2, gap, s - 4, color)
    End Sub

    Private Shared Sub DrawPlus(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim t = Math.Max(1, s \ 8)
        Dim hs = s \ 2
        FillRectDirect(batch, px, x + hs - t \ 2, y + q4(s), t, s \ 2, color)
        FillRectDirect(batch, px, x + q4(s), y + hs - t \ 2, s \ 2, t, color)
    End Sub

    Private Shared Function q4(s As Integer) As Integer
        Return s \ 4
    End Function

    Private Shared Sub DrawMenu(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim gap = s \ 5
        Dim barY = s \ 5
        For i = 0 To 2
            FillRectDirect(batch, px, x + s \ 5, y + barY + i * gap, s * 3 \ 5, Math.Max(1, s \ 10), color)
        Next
    End Sub

    Private Shared Sub DrawHome(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim hs = s \ 2
        DrawTriangle(batch, px, x + 1, y + 1, s - 2, color)
        FillRectDirect(batch, px, x + hs - s \ 6, y + hs, s \ 3, s \ 3, color)
    End Sub

    Private Shared Sub DrawHeart(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim q = s \ 4
        FillRectDirect(batch, px, x + 1, y + q, q, q, color)
        FillRectDirect(batch, px, x + q + 1, y + q - 1, q, q, color)
        FillRectDirect(batch, px, x + q * 2 + 1, y + q, q, q, color)
        FillRectDirect(batch, px, x + q * 3 + 1, y + q, q, q, color)
        For i = 0 To s - q * 2
            Dim w = s - i * 2
            If w > 0 Then FillRectDirect(batch, px, x + i, y + q * 2 + i, w, 1, color)
        Next
    End Sub

    Private Shared Sub DrawStar(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim cx = x + s \ 2
        Dim cy = y + s \ 2
        Dim outerR = s \ 2 - 1
        Dim innerR = s \ 5

        For angle = 0 To 359 Step 36
            Dim rad = angle * Math.PI / 180.0
            Dim nextRad = (angle + 36) * Math.PI / 180.0
            Dim x1 = cx + CInt(Math.Cos(rad) * outerR)
            Dim y1 = cy + CInt(Math.Sin(rad) * outerR)
            Dim x2 = cx + CInt(Math.Cos(nextRad) * innerR)
            Dim y2 = cy + CInt(Math.Sin(nextRad) * innerR)
            DrawLineBetween(batch, px, x1, y1, x2, y2, color)
            Dim x3 = cx + CInt(Math.Cos(nextRad) * outerR)
            Dim y3 = cy + CInt(Math.Sin(nextRad) * outerR)
            DrawLineBetween(batch, px, x2, y2, x3, y3, color)
        Next
    End Sub

    Private Shared Sub DrawLineBetween(batch As SpriteBatch, px As Texture2D, x0 As Integer, y0 As Integer, x1 As Integer, y1 As Integer, color As Color)
        Dim dx = Math.Abs(x1 - x0)
        Dim dy = Math.Abs(y1 - y0)
        Dim sx = If(x0 < x1, 1, -1)
        Dim sy = If(y0 < y1, 1, -1)
        Dim err = dx - dy

        While True
            FillRectDirect(batch, px, x0, y0, 1, 1, color)
            If x0 = x1 AndAlso y0 = y1 Then Exit While
            Dim e2 = 2 * err
            If e2 > -dy Then
                err -= dy
                x0 += sx
            End If
            If e2 < dx Then
                err += dx
                y0 += sy
            End If
        End While
    End Sub

    Private Shared Sub DrawEye(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim cy = y + s \ 2
        For i = 0 To s - 1
            Dim ratio = Math.Abs(i - s \ 2) / CSng(s \ 2)
            Dim w = CInt((1 - ratio) * s)
            If w > 0 Then
                FillRectDirect(batch, px, x + (s - w) \ 2, cy - s \ 2 + i, w, 1, color)
            End If
        Next
        Dim r = s \ 6
        DrawCircleOutline(batch, px, x + s \ 2, cy, r * 2, color)
    End Sub

    Private Shared Sub DrawLock(batch As SpriteBatch, px As Texture2D, x As Integer, y As Integer, s As Integer, color As Color)
        Dim bodyH = s * 2 \ 3
        Dim bodyY = y + s - bodyH - 1
        DrawRectOutline(batch, px, x + s \ 4, bodyY, s \ 2, bodyH, color)
        DrawCircleOutline(batch, px, x + s \ 2, bodyY, s \ 3, color)
        FillRectDirect(batch, px, x + s \ 4, bodyY, s \ 2, bodyH \ 2, color)
    End Sub
End Class
