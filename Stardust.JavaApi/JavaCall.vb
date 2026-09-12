Imports System.Reflection

''' <summary>
''' Reflection dispatcher used by the adapter widgets to call back into the
''' Java facade object that owns them. The Java side implements methods named
''' java_onXxx(...) on the exposed widget class; the adapter finds the method
''' by name + argument types and invokes it with the boxed parameters.
'''
''' Dispatch is deliberately exception-safe: a bad Java handler must never
''' take the display thread down, so any failure is swallowed.
''' </summary>
Friend Module JavaCall

        Public Sub Dispatch(owner As Object, name As String, ParamArray args As Object())
            If owner Is Nothing Then Return

            Try
                Dim types(args.Length - 1) As Type
                For i = 0 To args.Length - 1
                    types(i) = If(args(i) Is Nothing, GetType(Object), args(i).GetType())
                Next

                Dim m = owner.GetType().GetMethod(
                    name,
                    BindingFlags.Public Or BindingFlags.Instance,
                    Nothing, types, Nothing)

                If m Is Nothing Then Return
                m.Invoke(owner, args)
            Catch
                ' swallow: a Java handler must never take the UI thread down
            End Try
        End Sub

    End Module