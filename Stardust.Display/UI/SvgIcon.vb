Imports System.Linq
Imports System.Xml.Linq
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Structure PointF
    Public Property X As Single
    Public Property Y As Single
    Public Sub New(x As Single, y As Single)
        Me.X = x
        Me.Y = y
    End Sub
End Structure

Public Structure RectF
    Public Property X As Single
    Public Property Y As Single
    Public Property Width As Single
    Public Property Height As Single
    Public Sub New(x As Single, y As Single, w As Single, h As Single)
        Me.X = x : Me.Y = y : Me.Width = w : Me.Height = h
    End Sub
End Structure

Public Class SvgIcon
    Private _width As Integer
    Private _height As Integer
    Private _elements As New List(Of SvgElement)

    Public Class SvgElement
        Public Property Type As SvgShapeType
        Public Property X As Single
        Public Property Y As Single
        Public Property Width As Single
        Public Property Height As Single
        Public Property X2 As Single
        Public Property Y2 As Single
        Public Property Radius As Single
        Public Property RadiusX As Single
        Public Property RadiusY As Single
        Public Property Points As List(Of PointF)
        Public Property PathData As String
        Public Property Fill As Color = Color.White
        Public Property Stroke As Color = Color.Transparent
        Public Property StrokeWidth As Single = 1
        Public Property CornerRadius As Single = 0
    End Class

    Public Enum SvgShapeType
        Rect
        Circle
        Ellipse
        Line
        Polyline
        Polygon
        Path
    End Enum

    Public Sub New(svgContent As String)
        Parse(svgContent)
    End Sub

    Public Sub New(svgPath As IO.FileInfo)
        Parse(IO.File.ReadAllText(svgPath.FullName))
    End Sub

    Private Sub Parse(svg As String)
        Try
            Dim doc = XDocument.Parse(svg)
            Dim root = doc.Root
            If root Is Nothing Then Return

            Dim viewBox = ParseViewBox(root.Attribute("viewBox")?.Value)
            _width = CInt(viewBox.Width)
            _height = CInt(viewBox.Height)

            If _width <= 0 Then _width = CInt(ParseFloat(root.Attribute("width")?.Value, 24))
            If _height <= 0 Then _height = CInt(ParseFloat(root.Attribute("height")?.Value, 24))

            ParseElements(root)
        Catch
        End Try
    End Sub

    Private Sub ParseElements(parent As XElement)
        For Each el In parent.Elements()
            Dim elName = el.Name.LocalName.ToLower()

            Select Case elName
                Case "rect"
                    _elements.Add(ParseRect(el))
                Case "circle"
                    _elements.Add(ParseCircle(el))
                Case "ellipse"
                    _elements.Add(ParseEllipse(el))
                Case "line"
                    _elements.Add(ParseLine(el))
                Case "polyline"
                    _elements.Add(ParsePolyline(el))
                Case "polygon"
                    _elements.Add(ParsePolygon(el))
                Case "path"
                    _elements.Add(ParsePath(el))
                Case "g"
                    ParseElements(el)
                Case "svg"
                    ParseElements(el)
            End Select
        Next
    End Sub

    Private Function ParseRect(el As XElement) As SvgElement
        Return New SvgElement With {
            .Type = SvgShapeType.Rect,
            .X = ParseFloat(el.Attribute("x")?.Value, 0),
            .Y = ParseFloat(el.Attribute("y")?.Value, 0),
            .Width = ParseFloat(el.Attribute("width")?.Value, 0),
            .Height = ParseFloat(el.Attribute("height")?.Value, 0),
            .CornerRadius = ParseFloat(el.Attribute("rx")?.Value, 0),
            .Fill = ParseColor(el.Attribute("fill")?.Value, Color.White),
            .Stroke = ParseColor(el.Attribute("stroke")?.Value, Color.Transparent),
            .StrokeWidth = ParseFloat(el.Attribute("stroke-width")?.Value, 1)
        }
    End Function

    Private Function ParseCircle(el As XElement) As SvgElement
        Return New SvgElement With {
            .Type = SvgShapeType.Circle,
            .X = ParseFloat(el.Attribute("cx")?.Value, 0),
            .Y = ParseFloat(el.Attribute("cy")?.Value, 0),
            .Radius = ParseFloat(el.Attribute("r")?.Value, 0),
            .Fill = ParseColor(el.Attribute("fill")?.Value, Color.White),
            .Stroke = ParseColor(el.Attribute("stroke")?.Value, Color.Transparent),
            .StrokeWidth = ParseFloat(el.Attribute("stroke-width")?.Value, 1)
        }
    End Function

    Private Function ParseEllipse(el As XElement) As SvgElement
        Return New SvgElement With {
            .Type = SvgShapeType.Ellipse,
            .X = ParseFloat(el.Attribute("cx")?.Value, 0),
            .Y = ParseFloat(el.Attribute("cy")?.Value, 0),
            .RadiusX = ParseFloat(el.Attribute("rx")?.Value, 0),
            .RadiusY = ParseFloat(el.Attribute("ry")?.Value, 0),
            .Fill = ParseColor(el.Attribute("fill")?.Value, Color.White),
            .Stroke = ParseColor(el.Attribute("stroke")?.Value, Color.Transparent),
            .StrokeWidth = ParseFloat(el.Attribute("stroke-width")?.Value, 1)
        }
    End Function

    Private Function ParseLine(el As XElement) As SvgElement
        Return New SvgElement With {
            .Type = SvgShapeType.Line,
            .X = ParseFloat(el.Attribute("x1")?.Value, 0),
            .Y = ParseFloat(el.Attribute("y1")?.Value, 0),
            .X2 = ParseFloat(el.Attribute("x2")?.Value, 0),
            .Y2 = ParseFloat(el.Attribute("y2")?.Value, 0),
            .Stroke = ParseColor(el.Attribute("stroke")?.Value, Color.White),
            .StrokeWidth = ParseFloat(el.Attribute("stroke-width")?.Value, 1)
        }
    End Function

    Private Function ParsePolyline(el As XElement) As SvgElement
        Return New SvgElement With {
            .Type = SvgShapeType.Polyline,
            .Points = ParsePoints(el.Attribute("points")?.Value),
            .Fill = ParseColor(el.Attribute("fill")?.Value, Color.Transparent),
            .Stroke = ParseColor(el.Attribute("stroke")?.Value, Color.White),
            .StrokeWidth = ParseFloat(el.Attribute("stroke-width")?.Value, 1)
        }
    End Function

    Private Function ParsePolygon(el As XElement) As SvgElement
        Return New SvgElement With {
            .Type = SvgShapeType.Polygon,
            .Points = ParsePoints(el.Attribute("points")?.Value),
            .Fill = ParseColor(el.Attribute("fill")?.Value, Color.White),
            .Stroke = ParseColor(el.Attribute("stroke")?.Value, Color.Transparent),
            .StrokeWidth = ParseFloat(el.Attribute("stroke-width")?.Value, 1)
        }
    End Function

    Private Function ParsePath(el As XElement) As SvgElement
        Return New SvgElement With {
            .Type = SvgShapeType.Path,
            .PathData = el.Attribute("d")?.Value,
            .Fill = ParseColor(el.Attribute("fill")?.Value, Color.White),
            .Stroke = ParseColor(el.Attribute("stroke")?.Value, Color.Transparent),
            .StrokeWidth = ParseFloat(el.Attribute("stroke-width")?.Value, 1)
        }
    End Function

    Public Function Render(graphics As GraphicsDevice, scale As Integer) As Texture2D
        If _width <= 0 OrElse _height <= 0 Then
            Return CreatePlaceholder(graphics, scale)
        End If

        Dim texW = _width * scale
        Dim texH = _height * scale
        Dim pixels(texW * texH - 1) As Color

        For Each elem In _elements
            RasterizeElement(pixels, texW, texH, elem, scale)
        Next

        Dim tex As New Texture2D(graphics, texW, texH)
        tex.SetData(pixels)
        Return tex
    End Function

    Private Sub RasterizeElement(pixels As Color(), texW As Integer, texH As Integer, elem As SvgElement, scale As Integer)
        Select Case elem.Type
            Case SvgShapeType.Rect
                RasterizeRect(pixels, texW, texH, elem, scale)
            Case SvgShapeType.Circle
                RasterizeCircle(pixels, texW, texH, elem, scale)
            Case SvgShapeType.Ellipse
                RasterizeEllipse(pixels, texW, texH, elem, scale)
            Case SvgShapeType.Line
                RasterizeLine(pixels, texW, texH, elem, scale)
            Case SvgShapeType.Polyline, SvgShapeType.Polygon
                RasterizePolyline(pixels, texW, texH, elem, scale)
            Case SvgShapeType.Path
                RasterizePath(pixels, texW, texH, elem, scale)
        End Select
    End Sub

    Private Sub RasterizeRect(pixels As Color(), texW As Integer, texH As Integer, elem As SvgElement, scale As Integer)
        Dim x0 = CInt(elem.X * scale)
        Dim y0 = CInt(elem.Y * scale)
        Dim w = CInt(elem.Width * scale)
        Dim h = CInt(elem.Height * scale)

        If elem.Fill.A > 0 Then
            For y = y0 To y0 + h - 1
                For x = x0 To x0 + w - 1
                    SetPixel(pixels, texW, texH, x, y, elem.Fill)
                Next
            Next
        End If

        If elem.Stroke.A > 0 Then
            DrawStrokeRect(pixels, texW, texH, x0, y0, w, h, elem.Stroke, CInt(elem.StrokeWidth * scale))
        End If
    End Sub

    Private Sub RasterizeCircle(pixels As Color(), texW As Integer, texH As Integer, elem As SvgElement, scale As Integer)
        Dim cx = CInt(elem.X * scale)
        Dim cy = CInt(elem.Y * scale)
        Dim r = CInt(elem.Radius * scale)

        For y = cy - r To cy + r
            For x = cx - r To cx + r
                Dim dist = Math.Sqrt((x - cx) ^ 2 + (y - cy) ^ 2)
                If elem.Fill.A > 0 AndAlso dist <= r Then
                    SetPixel(pixels, texW, texH, x, y, elem.Fill)
                ElseIf elem.Stroke.A > 0 AndAlso Math.Abs(dist - r) < elem.StrokeWidth * scale Then
                    SetPixel(pixels, texW, texH, x, y, elem.Stroke)
                End If
            Next
        Next
    End Sub

    Private Sub RasterizeEllipse(pixels As Color(), texW As Integer, texH As Integer, elem As SvgElement, scale As Integer)
        Dim cx = CInt(elem.X * scale)
        Dim cy = CInt(elem.Y * scale)
        Dim rx = CInt(elem.RadiusX * scale)
        Dim ry = CInt(elem.RadiusY * scale)
        If rx <= 0 OrElse ry <= 0 Then Return

        For y = cy - ry To cy + ry
            For x = cx - rx To cx + rx
                Dim dx = (x - cx) / CSng(rx)
                Dim dy = (y - cy) / CSng(ry)
                Dim dist = dx * dx + dy * dy
                If elem.Fill.A > 0 AndAlso dist <= 1.0F Then
                    SetPixel(pixels, texW, texH, x, y, elem.Fill)
                ElseIf elem.Stroke.A > 0 AndAlso Math.Abs(Math.Sqrt(dist) - 1.0F) * Math.Max(rx, ry) < elem.StrokeWidth * scale Then
                    SetPixel(pixels, texW, texH, x, y, elem.Stroke)
                End If
            Next
        Next
    End Sub

    Private Sub RasterizeLine(pixels As Color(), texW As Integer, texH As Integer, elem As SvgElement, scale As Integer)
        Dim x0 = CInt(elem.X * scale)
        Dim y0 = CInt(elem.Y * scale)
        Dim x1 = CInt(elem.X2 * scale)
        Dim y1 = CInt(elem.Y2 * scale)
        Dim color = If(elem.Stroke.A > 0, elem.Stroke, elem.Fill)
        Dim thickness = Math.Max(1, CInt(elem.StrokeWidth * scale))

        RasterizeLineBresenham(pixels, texW, texH, x0, y0, x1, y1, color, thickness)
    End Sub

    Private Sub RasterizePolyline(pixels As Color(), texW As Integer, texH As Integer, elem As SvgElement, scale As Integer)
        If elem.Points Is Nothing OrElse elem.Points.Count < 2 Then Return

        Dim color = If(elem.Stroke.A > 0, elem.Stroke, elem.Fill)
        Dim thickness = Math.Max(1, CInt(elem.StrokeWidth * scale))

        For i = 0 To elem.Points.Count - 2
            Dim x0 = CInt(elem.Points(i).X * scale)
            Dim y0 = CInt(elem.Points(i).Y * scale)
            Dim x1 = CInt(elem.Points(i + 1).X * scale)
            Dim y1 = CInt(elem.Points(i + 1).Y * scale)
            RasterizeLineBresenham(pixels, texW, texH, x0, y0, x1, y1, color, thickness)
        Next

        If elem.Type = SvgShapeType.Polygon AndAlso elem.Points.Count > 2 Then
            Dim x0 = CInt(elem.Points.Last().X * scale)
            Dim y0 = CInt(elem.Points.Last().Y * scale)
            Dim x1 = CInt(elem.Points(0).X * scale)
            Dim y1 = CInt(elem.Points(0).Y * scale)
            RasterizeLineBresenham(pixels, texW, texH, x0, y0, x1, y1, color, thickness)
        End If
    End Sub

    Private Sub RasterizePath(pixels As Color(), texW As Integer, texH As Integer, elem As SvgElement, scale As Integer)
        If String.IsNullOrEmpty(elem.PathData) Then Return
        Dim segments = ParsePathData(elem.PathData)
        Dim color = If(elem.Stroke.A > 0, elem.Stroke, elem.Fill)
        Dim thickness = Math.Max(1, CInt(elem.StrokeWidth * scale))

        For Each seg In segments
            RasterizeLineBresenham(pixels, texW, texH,
                CInt(seg(0).X * scale), CInt(seg(0).Y * scale),
                CInt(seg(1).X * scale), CInt(seg(1).Y * scale),
                color, thickness)
        Next
    End Sub

    Private Shared Function ParsePathData(d As String) As List(Of PointF())
        Dim result As New List(Of PointF())
        If String.IsNullOrEmpty(d) Then Return result

        d = d.Replace(",", " ").Replace("-", " -").Replace("  ", " ").Trim()
        Dim parts = d.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim cx As Single = 0
        Dim cy As Single = 0
        Dim startX As Single = 0
        Dim startY As Single = 0
        Dim i = 0

        While i < parts.Length
            Dim cmd = parts(i)(0)
            i += 1

            Select Case cmd
                Case "M"c, "m"c
                    While i < parts.Length AndAlso Not Char.IsLetter(parts(i)(0))
                        Dim px = Single.Parse(parts(i), System.Globalization.CultureInfo.InvariantCulture)
                        Dim py = Single.Parse(parts(i + 1), System.Globalization.CultureInfo.InvariantCulture)
                        If cmd = "m"c Then px += cx : py += cy
                        cx = px : cy = py
                        startX = cx : startY = cy
                        i += 2
                    End While

                Case "L"c, "l"c
                    While i < parts.Length AndAlso Not Char.IsLetter(parts(i)(0))
                        Dim px = Single.Parse(parts(i), System.Globalization.CultureInfo.InvariantCulture)
                        Dim py = Single.Parse(parts(i + 1), System.Globalization.CultureInfo.InvariantCulture)
                        If cmd = "l"c Then px += cx : py += cy
                        result.Add({New PointF(cx, cy), New PointF(px, py)})
                        cx = px : cy = py
                        i += 2
                    End While

                Case "H"c, "h"c
                    While i < parts.Length AndAlso Not Char.IsLetter(parts(i)(0))
                        Dim px = Single.Parse(parts(i), System.Globalization.CultureInfo.InvariantCulture)
                        If cmd = "h"c Then px += cx
                        result.Add({New PointF(cx, cy), New PointF(px, cy)})
                        cx = px
                        i += 1
                    End While

                Case "V"c, "v"c
                    While i < parts.Length AndAlso Not Char.IsLetter(parts(i)(0))
                        Dim py = Single.Parse(parts(i), System.Globalization.CultureInfo.InvariantCulture)
                        If cmd = "v"c Then py += cy
                        result.Add({New PointF(cx, cy), New PointF(cx, py)})
                        cy = py
                        i += 1
                    End While

                Case "Z"c, "z"c
                    result.Add({New PointF(cx, cy), New PointF(startX, startY)})
                    cx = startX : cy = startY

                Case "C"c, "c"c
                    While i + 5 < parts.Length AndAlso Not Char.IsLetter(parts(i)(0))
                        Dim x1 = Single.Parse(parts(i), System.Globalization.CultureInfo.InvariantCulture)
                        Dim y1 = Single.Parse(parts(i + 1), System.Globalization.CultureInfo.InvariantCulture)
                        Dim x2 = Single.Parse(parts(i + 2), System.Globalization.CultureInfo.InvariantCulture)
                        Dim y2 = Single.Parse(parts(i + 3), System.Globalization.CultureInfo.InvariantCulture)
                        Dim px = Single.Parse(parts(i + 4), System.Globalization.CultureInfo.InvariantCulture)
                        Dim py = Single.Parse(parts(i + 5), System.Globalization.CultureInfo.InvariantCulture)
                        If cmd = "c"c Then
                            x1 += cx : y1 += cy : x2 += cx : y2 += cy : px += cx : py += cy
                        End If
                        Dim bezierPts = SubdivideBezier(New PointF(cx, cy), New PointF(x1, y1), New PointF(x2, y2), New PointF(px, py), 8)
                        For j = 0 To bezierPts.Count - 2
                            result.Add({bezierPts(j), bezierPts(j + 1)})
                        Next
                        cx = px : cy = py
                        i += 6
                    End While

                Case "Q"c, "q"c
                    While i + 3 < parts.Length AndAlso Not Char.IsLetter(parts(i)(0))
                        Dim qx = Single.Parse(parts(i), System.Globalization.CultureInfo.InvariantCulture)
                        Dim qy = Single.Parse(parts(i + 1), System.Globalization.CultureInfo.InvariantCulture)
                        Dim px = Single.Parse(parts(i + 2), System.Globalization.CultureInfo.InvariantCulture)
                        Dim py = Single.Parse(parts(i + 3), System.Globalization.CultureInfo.InvariantCulture)
                        If cmd = "q"c Then qx += cx : qy += cy : px += cx : py += cy
                        Dim bezierPts = SubdivideQuad(New PointF(cx, cy), New PointF(qx, qy), New PointF(px, py), 8)
                        For j = 0 To bezierPts.Count - 2
                            result.Add({bezierPts(j), bezierPts(j + 1)})
                        Next
                        cx = px : cy = py
                        i += 4
                    End While

                Case Else
                    i += 1
            End Select
        End While

        Return result
    End Function

    Private Shared Function SubdivideBezier(p0 As PointF, p1 As PointF, p2 As PointF, p3 As PointF, segments As Integer) As List(Of PointF)
        Dim pts As New List(Of PointF)()
        For t = 0.0F To 1.0F Step 1.0F / segments
            Dim u = 1 - t
            Dim x = u * u * u * p0.X + 3 * u * u * t * p1.X + 3 * u * t * t * p2.X + t * t * t * p3.X
            Dim y = u * u * u * p0.Y + 3 * u * u * t * p1.Y + 3 * u * t * t * p2.Y + t * t * t * p3.Y
            pts.Add(New PointF(x, y))
        Next
        pts.Add(p3)
        Return pts
    End Function

    Private Shared Function SubdivideQuad(p0 As PointF, p1 As PointF, p2 As PointF, segments As Integer) As List(Of PointF)
        Dim pts As New List(Of PointF)()
        For t = 0.0F To 1.0F Step 1.0F / segments
            Dim u = 1 - t
            Dim x = u * u * p0.X + 2 * u * t * p1.X + t * t * p2.X
            Dim y = u * u * p0.Y + 2 * u * t * p1.Y + t * t * p2.Y
            pts.Add(New PointF(x, y))
        Next
        pts.Add(p2)
        Return pts
    End Function

    Private Shared Sub RasterizeLineBresenham(pixels As Color(), texW As Integer, texH As Integer,
                                                x0 As Integer, y0 As Integer, x1 As Integer, y1 As Integer,
                                                color As Color, thickness As Integer)
        Dim dx = Math.Abs(x1 - x0)
        Dim dy = Math.Abs(y1 - y0)
        Dim sx = If(x0 < x1, 1, -1)
        Dim sy = If(y0 < y1, 1, -1)
        Dim err = dx - dy

        While True
            For tx = -thickness \ 2 To thickness \ 2
                For ty = -thickness \ 2 To thickness \ 2
                    SetPixel(pixels, texW, texH, x0 + tx, y0 + ty, color)
                Next
            Next

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

    Private Shared Sub DrawStrokeRect(pixels As Color(), texW As Integer, texH As Integer,
                                       x0 As Integer, y0 As Integer, w As Integer, h As Integer,
                                       color As Color, thickness As Integer)
        For i = 0 To thickness - 1
            RasterizeLineBresenham(pixels, texW, texH, x0, y0 + i, x0 + w - 1, y0 + i, color, 1)
            RasterizeLineBresenham(pixels, texW, texH, x0, y0 + h - 1 - i, x0 + w - 1, y0 + h - 1 - i, color, 1)
            RasterizeLineBresenham(pixels, texW, texH, x0 + i, y0, x0 + i, y0 + h - 1, color, 1)
            RasterizeLineBresenham(pixels, texW, texH, x0 + w - 1 - i, y0, x0 + w - 1 - i, y0 + h - 1, color, 1)
        Next
    End Sub

    Private Shared Sub SetPixel(pixels As Color(), texW As Integer, texH As Integer, x As Integer, y As Integer, color As Color)
        If x < 0 OrElse x >= texW OrElse y < 0 OrElse y >= texH Then Return
        If color.A = 0 Then Return
        pixels(y * texW + x) = color
    End Sub

    Private Shared Function CreatePlaceholder(graphics As GraphicsDevice, scale As Integer) As Texture2D
        Dim s = 24 * scale
        Dim pixels(s * s - 1) As Color
        Dim c = New Color(220, 60, 60)
        For i = 0 To s - 1
            pixels(i) = c
            pixels((s - 1) * s + i) = c
            pixels(i * s) = c
            pixels(i * s + s - 1) = c
        Next
        Dim tex As New Texture2D(graphics, s, s)
        tex.SetData(pixels)
        Return tex
    End Function

    Private Shared Function ParseFloat(value As String, defaultVal As Single) As Single
        If String.IsNullOrEmpty(value) Then Return defaultVal
        Single.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, defaultVal)
        Return defaultVal
    End Function

    Private Shared Function ParseColor(value As String, defaultColor As Color) As Color
        If String.IsNullOrEmpty(value) Then Return defaultColor
        value = value.Trim().ToLower()

        If value = "none" Then Return Color.Transparent

        If value.StartsWith("#") Then
            Dim hex = value.Substring(1)
            If hex.Length = 3 Then
                Dim r = Convert.ToInt32(hex(0).ToString() & hex(0).ToString(), 16)
                Dim g = Convert.ToInt32(hex(1).ToString() & hex(1).ToString(), 16)
                Dim b = Convert.ToInt32(hex(2).ToString() & hex(2).ToString(), 16)
                Return New Color(r, g, b)
            ElseIf hex.Length >= 6 Then
                Dim r = Convert.ToInt32(hex.Substring(0, 2), 16)
                Dim g = Convert.ToInt32(hex.Substring(2, 2), 16)
                Dim b = Convert.ToInt32(hex.Substring(4, 2), 16)
                Dim a = If(hex.Length >= 8, Convert.ToInt32(hex.Substring(6, 2), 16), 255)
                Return New Color(r, g, b, a)
            End If
        End If

        Select Case value
            Case "black" : Return Color.Black
            Case "white" : Return Color.White
            Case "red" : Return Color.Red
            Case "green" : Return Color.Green
            Case "blue" : Return Color.Blue
            Case "yellow" : Return Color.Yellow
            Case "cyan" : Return Color.Cyan
            Case "magenta" : Return Color.Magenta
            Case "gray", "grey" : Return Color.Gray
            Case "orange" : Return New Color(255, 165, 0)
            Case "purple" : Return New Color(128, 0, 128)
            Case "pink" : Return New Color(255, 192, 203)
            Case "brown" : Return New Color(139, 69, 19)
            Case "transparent" : Return Color.Transparent
        End Select

        Return defaultColor
    End Function

    Private Shared Function ParseViewBox(value As String) As RectF
        If String.IsNullOrEmpty(value) Then Return New RectF(0, 0, 0, 0)
        Dim parts = value.Split({" "c, ","c}, StringSplitOptions.RemoveEmptyEntries)
        If parts.Length < 4 Then Return New RectF(0, 0, 0, 0)
        Return New RectF(
            Single.Parse(parts(0), System.Globalization.CultureInfo.InvariantCulture),
            Single.Parse(parts(1), System.Globalization.CultureInfo.InvariantCulture),
            Single.Parse(parts(2), System.Globalization.CultureInfo.InvariantCulture),
            Single.Parse(parts(3), System.Globalization.CultureInfo.InvariantCulture))
    End Function

    Private Shared Function ParsePoints(value As String) As List(Of PointF)
        Dim result As New List(Of PointF)()
        If String.IsNullOrEmpty(value) Then Return result
        Dim nums = value.Replace(",", " ").Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim i = 0
        While i + 1 < nums.Length
            result.Add(New PointF(
                Single.Parse(nums(i), System.Globalization.CultureInfo.InvariantCulture),
                Single.Parse(nums(i + 1), System.Globalization.CultureInfo.InvariantCulture)))
            i += 2
        End While
        Return result
    End Function
End Class
