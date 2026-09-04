Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class StardustFont
    Private _texture As Texture2D
    Private _charWidth As Integer
    Private _charHeight As Integer
    Private _firstChar As Integer
    Private _charCount As Integer

    Public Sub New(graphics As GraphicsDevice)
        _charWidth = 6
        _charHeight = 10
        _firstChar = 32
        _charCount = 95

        Dim fontData As Byte() = GetFontData()
        Dim atlasWidth = _charCount * _charWidth
        Dim pixels(atlasWidth * _charHeight - 1) As Color

        For i = 0 To _charCount - 1
            For row = 0 To _charHeight - 1
                Dim b As Byte = fontData(i * _charHeight + row)
                For col = 0 To _charWidth - 1
                    If (b And CByte(1 << (_charWidth - 1 - col))) <> 0 Then
                        pixels(row * atlasWidth + i * _charWidth + col) = Color.White
                    End If
                Next
            Next
        Next

        _texture = New Texture2D(graphics, atlasWidth, _charHeight)
        _texture.SetData(pixels)
    End Sub

    Public Sub DrawString(batch As SpriteBatch, text As String, position As Vector2, color As Color)
        If String.IsNullOrEmpty(text) Then Return
        Dim x As Single = position.X
        For Each c In text
            Dim idx = AscW(c) - _firstChar
            If idx >= 0 AndAlso idx < _charCount Then
                Dim srcRect = New Rectangle(idx * _charWidth, 0, _charWidth, _charHeight)
                batch.Draw(_texture, New Rectangle(CInt(x), CInt(position.Y), _charWidth, _charHeight), srcRect, color)
            End If
            x += _charWidth
        Next
    End Sub

    Public Sub DrawStringClipped(batch As SpriteBatch, text As String, position As Vector2, color As Color, clipRect As Rectangle)
        If String.IsNullOrEmpty(text) Then Return
        Dim x As Single = position.X
        For Each c In text
            Dim idx = AscW(c) - _firstChar
            If idx >= 0 AndAlso idx < _charCount Then
                Dim destRect = New Rectangle(CInt(x), CInt(position.Y), _charWidth, _charHeight)
                If destRect.Intersects(clipRect) Then
                    Dim srcRect = New Rectangle(idx * _charWidth, 0, _charWidth, _charHeight)
                    batch.Draw(_texture, destRect, srcRect, color)
                End If
            End If
            x += _charWidth
        Next
    End Sub

    Public Function MeasureString(text As String) As Vector2
        If String.IsNullOrEmpty(text) Then Return Vector2.Zero
        Return New Vector2(text.Length * _charWidth, _charHeight)
    End Function

    Public ReadOnly Property CharWidth As Integer
        Get
            Return _charWidth
        End Get
    End Property

    Public ReadOnly Property CharHeight As Integer
        Get
            Return _charHeight
        End Get
    End Property

    Public Shared Function GetPublicFontData() As Byte()
        Return GetFontData()
    End Function

    Private Shared Function GetFontData() As Byte()
        Return New Byte() {
            &H00, &H00, &H00, &H00, &H00, &H00, &H00, &H00, &H00, &H00,
            &H08, &H08, &H08, &H08, &H08, &H00, &H08, &H00, &H00, &H00,
            &H14, &H14, &H14, &H00, &H00, &H00, &H00, &H00, &H00, &H00,
            &H14, &H14, &H3E, &H14, &H3E, &H14, &H14, &H00, &H00, &H00,
            &H08, &H3C, &H0A, &H1C, &H28, &H1E, &H08, &H00, &H00, &H00,
            &H22, &H21, &H02, &H04, &H08, &H11, &H21, &H00, &H00, &H00,
            &H0C, &H12, &H0C, &H1A, &H12, &H12, &H1D, &H00, &H00, &H00,
            &H08, &H08, &H10, &H00, &H00, &H00, &H00, &H00, &H00, &H00,
            &H04, &H08, &H10, &H10, &H10, &H10, &H08, &H04, &H00, &H00,
            &H10, &H08, &H04, &H04, &H04, &H04, &H08, &H10, &H00, &H00,
            &H00, &H08, &H2A, &H1C, &H2A, &H08, &H00, &H00, &H00, &H00,
            &H00, &H08, &H08, &H3E, &H08, &H08, &H00, &H00, &H00, &H00,
            &H00, &H00, &H00, &H00, &H00, &H08, &H08, &H04, &H00, &H00,
            &H00, &H00, &H00, &H3E, &H00, &H00, &H00, &H00, &H00, &H00,
            &H00, &H00, &H00, &H00, &H00, &H00, &H08, &H00, &H00, &H00,
            &H01, &H01, &H02, &H04, &H08, &H10, &H20, &H00, &H00, &H00,
            &H1C, &H22, &H26, &H2A, &H32, &H22, &H1C, &H00, &H00, &H00,
            &H08, &H0C, &H08, &H08, &H08, &H08, &H1C, &H00, &H00, &H00,
            &H1C, &H22, &H01, &H06, &H08, &H10, &H3E, &H00, &H00, &H00,
            &H1C, &H22, &H01, &H0C, &H01, &H22, &H1C, &H00, &H00, &H00,
            &H02, &H06, &H0A, &H12, &H3F, &H02, &H02, &H00, &H00, &H00,
            &H3E, &H20, &H3C, &H01, &H01, &H22, &H1C, &H00, &H00, &H00,
            &H06, &H08, &H10, &H1C, &H22, &H22, &H1C, &H00, &H00, &H00,
            &H3E, &H01, &H02, &H04, &H08, &H08, &H08, &H00, &H00, &H00,
            &H1C, &H22, &H22, &H1C, &H22, &H22, &H1C, &H00, &H00, &H00,
            &H1C, &H22, &H22, &H1E, &H01, &H02, &H1C, &H00, &H00, &H00,
            &H00, &H00, &H08, &H00, &H00, &H08, &H00, &H00, &H00, &H00,
            &H00, &H00, &H08, &H00, &H00, &H08, &H08, &H04, &H00, &H00,
            &H02, &H04, &H08, &H10, &H08, &H04, &H02, &H00, &H00, &H00,
            &H00, &H00, &H3E, &H00, &H3E, &H00, &H00, &H00, &H00, &H00,
            &H10, &H08, &H04, &H02, &H04, &H08, &H10, &H00, &H00, &H00,
            &H1C, &H22, &H01, &H02, &H04, &H00, &H04, &H00, &H00, &H00,
            &H1C, &H22, &H2E, &H2A, &H2E, &H20, &H1E, &H00, &H00, &H00,
            &H08, &H14, &H22, &H22, &H3E, &H22, &H22, &H00, &H00, &H00,
            &H3C, &H22, &H22, &H3C, &H22, &H22, &H3C, &H00, &H00, &H00,
            &H1C, &H22, &H20, &H20, &H20, &H22, &H1C, &H00, &H00, &H00,
            &H38, &H24, &H22, &H22, &H22, &H24, &H38, &H00, &H00, &H00,
            &H3E, &H20, &H20, &H3C, &H20, &H20, &H3E, &H00, &H00, &H00,
            &H3E, &H20, &H20, &H3C, &H20, &H20, &H20, &H00, &H00, &H00,
            &H1C, &H22, &H20, &H26, &H22, &H22, &H1C, &H00, &H00, &H00,
            &H22, &H22, &H22, &H3E, &H22, &H22, &H22, &H00, &H00, &H00,
            &H1C, &H08, &H08, &H08, &H08, &H08, &H1C, &H00, &H00, &H00,
            &H01, &H01, &H01, &H01, &H01, &H22, &H1C, &H00, &H00, &H00,
            &H22, &H24, &H28, &H30, &H28, &H24, &H22, &H00, &H00, &H00,
            &H20, &H20, &H20, &H20, &H20, &H20, &H3E, &H00, &H00, &H00,
            &H22, &H36, &H2A, &H2A, &H22, &H22, &H22, &H00, &H00, &H00,
            &H22, &H22, &H32, &H2A, &H26, &H22, &H22, &H00, &H00, &H00,
            &H1C, &H22, &H22, &H22, &H22, &H22, &H1C, &H00, &H00, &H00,
            &H3C, &H22, &H22, &H3C, &H20, &H20, &H20, &H00, &H00, &H00,
            &H1C, &H22, &H22, &H22, &H2A, &H24, &H1A, &H00, &H00, &H00,
            &H3C, &H22, &H22, &H3C, &H28, &H24, &H22, &H00, &H00, &H00,
            &H1C, &H22, &H20, &H1C, &H02, &H22, &H1C, &H00, &H00, &H00,
            &H3E, &H08, &H08, &H08, &H08, &H08, &H08, &H00, &H00, &H00,
            &H22, &H22, &H22, &H22, &H22, &H22, &H1C, &H00, &H00, &H00,
            &H22, &H22, &H22, &H22, &H14, &H14, &H08, &H00, &H00, &H00,
            &H22, &H22, &H22, &H2A, &H2A, &H36, &H22, &H00, &H00, &H00,
            &H22, &H22, &H14, &H08, &H14, &H22, &H22, &H00, &H00, &H00,
            &H22, &H22, &H14, &H08, &H08, &H08, &H08, &H00, &H00, &H00,
            &H3E, &H01, &H02, &H04, &H08, &H10, &H3E, &H00, &H00, &H00,
            &H1C, &H10, &H10, &H10, &H10, &H10, &H1C, &H00, &H00, &H00,
            &H20, &H10, &H08, &H04, &H02, &H01, &H00, &H00, &H00, &H00,
            &H1C, &H04, &H04, &H04, &H04, &H04, &H1C, &H00, &H00, &H00,
            &H08, &H14, &H22, &H00, &H00, &H00, &H00, &H00, &H00, &H00,
            &H00, &H00, &H00, &H00, &H00, &H00, &H3E, &H00, &H00, &H00,
            &H10, &H08, &H04, &H00, &H00, &H00, &H00, &H00, &H00, &H00,
            &H00, &H00, &H1C, &H02, &H1E, &H22, &H1E, &H00, &H00, &H00,
            &H20, &H20, &H3C, &H22, &H22, &H22, &H3C, &H00, &H00, &H00,
            &H00, &H00, &H1C, &H22, &H20, &H22, &H1C, &H00, &H00, &H00,
            &H02, &H02, &H1E, &H22, &H22, &H22, &H1E, &H00, &H00, &H00,
            &H00, &H00, &H1C, &H22, &H3E, &H20, &H1C, &H00, &H00, &H00,
            &H06, &H08, &H08, &H1C, &H08, &H08, &H08, &H00, &H00, &H00,
            &H00, &H00, &H1E, &H22, &H22, &H1E, &H02, &H1C, &H00, &H00,
            &H20, &H20, &H2C, &H32, &H22, &H22, &H22, &H00, &H00, &H00,
            &H08, &H00, &H0C, &H08, &H08, &H08, &H1C, &H00, &H00, &H00,
            &H04, &H00, &H06, &H04, &H04, &H04, &H04, &H08, &H00, &H00,
            &H20, &H20, &H24, &H28, &H30, &H28, &H24, &H00, &H00, &H00,
            &H0C, &H08, &H08, &H08, &H08, &H08, &H1C, &H00, &H00, &H00,
            &H00, &H00, &H34, &H2A, &H2A, &H2A, &H22, &H00, &H00, &H00,
            &H00, &H00, &H2C, &H32, &H22, &H22, &H22, &H00, &H00, &H00,
            &H00, &H00, &H1C, &H22, &H22, &H22, &H1C, &H00, &H00, &H00,
            &H00, &H00, &H3C, &H22, &H22, &H3C, &H20, &H20, &H00, &H00,
            &H00, &H00, &H1E, &H22, &H22, &H1E, &H02, &H02, &H00, &H00,
            &H00, &H00, &H2C, &H32, &H20, &H20, &H20, &H00, &H00, &H00,
            &H00, &H00, &H1E, &H20, &H1C, &H02, &H3C, &H00, &H00, &H00,
            &H08, &H08, &H1C, &H08, &H08, &H08, &H06, &H00, &H00, &H00,
            &H00, &H00, &H22, &H22, &H22, &H26, &H1A, &H00, &H00, &H00,
            &H00, &H00, &H22, &H22, &H22, &H14, &H08, &H00, &H00, &H00,
            &H00, &H00, &H22, &H22, &H2A, &H2A, &H14, &H00, &H00, &H00,
            &H00, &H00, &H22, &H14, &H08, &H14, &H22, &H00, &H00, &H00,
            &H00, &H00, &H22, &H22, &H22, &H1E, &H02, &H1C, &H00, &H00,
            &H00, &H00, &H3E, &H02, &H0C, &H10, &H3E, &H00, &H00, &H00,
            &H04, &H08, &H08, &H10, &H08, &H08, &H04, &H00, &H00, &H00,
            &H08, &H08, &H08, &H08, &H08, &H08, &H08, &H00, &H00, &H00,
            &H10, &H08, &H08, &H04, &H08, &H08, &H10, &H00, &H00, &H00,
            &H00, &H00, &H00, &H12, &H2C, &H00, &H00, &H00, &H00, &H00
        }
    End Function
End Class
