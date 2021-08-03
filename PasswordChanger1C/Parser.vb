Namespace ParserServices
    Public Class ParserList
        Inherits List(Of ParserList)
        Private ReadOnly ItemValue As String
        Private ReadOnly Is_List As Boolean

        Public Sub New(itemValue As String)
            Me.ItemValue = itemValue
            Me.Is_List = False
        End Sub

        Public Sub New()
            Me.ItemValue = ""
            Me.Is_List = True
        End Sub

        Public Overrides Function ToString() As String
            If Not Me.IsList() Then
                Return Me.ItemValue
            Else
                Return MyBase.ToString()
            End If
        End Function

        Public ReadOnly Property IsList()
            Get
                Return Me.Is_List
            End Get
        End Property

        Public Overloads Sub Add(ByVal itemValue As String)
            MyBase.Add(New ParserList(itemValue))
        End Sub
    End Class

    Public Class ParsesClass

        Public Shared Function ParseString(Str As String) As ParserList

            Dim Arr = Str.Split(",")

            Dim List = ParseStringInternal(Arr, 0, Arr.Length - 1)

            Return List

        End Function

        Private Shared Function ParseStringInternal(Arr() As String, ByRef Position As Integer, ArrLength As Integer) As ParserList

            'TODO - не обрабатываются ситуации с двойными кавычками и переносами строк в тексте 

            Dim List = New ParserList()

            While True

                Dim Val = Arr(Position).Trim
                If Val.StartsWith("{") Then

                    Arr(Position) = Val.Substring(1)

                    List.Add(ParseStringInternal(Arr, Position, ArrLength))

                ElseIf String.IsNullOrEmpty(Val) Then
                    Position = Position + 1

                Else

                    Dim Pos = Val.IndexOf("}")
                    If Pos > -1 Then

                        Dim Vl2 = Val.Substring(0, Pos)
                        If Not String.IsNullOrEmpty(Vl2) Then
                            List.Add(Vl2)
                        End If

                        Arr(Position) = Val.Substring(Pos + 1)
                        Exit While
                    Else
                        List.Add(Val)
                        Position = Position + 1
                    End If
                End If


                If Position >= ArrLength Then
                    Exit While
                End If

            End While

            Return List

        End Function

    End Class

End Namespace

