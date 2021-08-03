

Imports System.Security.Cryptography
Imports System.Text

Module CommonModule


    Function DecodePasswordStructure(bytes_Input As Byte(), ByRef KeySize As Integer, ByRef KeyData As Byte()) As String

        Dim Base = Convert.ToInt16(bytes_Input.GetValue(0), 10)

        KeySize = Base
        KeyData = New Byte(Base - 1) {}
        For a = 1 To Base
            KeyData(a - 1) = bytes_Input(a)
        Next

        Dim i = Base + 1
        Dim j = 1
        Dim MaxI = bytes_Input.Length

        Dim BytesResult() As Byte = New Byte(MaxI - Base - 1) {}

        While i < MaxI
            If j > Base Then
                j = 1
            End If

            Dim AA = Convert.ToInt16(bytes_Input.GetValue(i), 10)
            Dim BB = Convert.ToInt16(bytes_Input.GetValue(j), 10)
            Dim CC = Convert.ToByte(AA Xor BB) ' 239 for first

            BytesResult.SetValue(CC, i - Base - 1)

            i = i + 1
            j = j + 1

        End While

        Return Encoding.UTF8.GetString(BytesResult)

    End Function


    Function EncodePasswordStructure(Str As String, ByVal KeySize As Integer, ByVal KeyData As Byte()) As Byte()

        Dim bytes_Input = Encoding.UTF8.GetBytes(Str)

        Dim Base = KeySize

        Dim BytesResult() As Byte = New Byte(bytes_Input.Length + Base - 1) {}
        BytesResult.SetValue(Convert.ToByte(Base), 0)
        For ii = 1 To Base
            BytesResult.SetValue(KeyData.GetValue(ii - 1), ii)
        Next

        Dim MaxI = bytes_Input.Length - 1
        Dim i = 1
        Dim j = 1

        While i <= MaxI
            If j > Base Then
                j = 1
            End If

            Dim AA = Convert.ToInt16(bytes_Input.GetValue(i - 1), 10)
            Dim BB = Convert.ToInt16(BytesResult.GetValue(j), 10)
            Dim CC = Convert.ToByte(AA Xor BB)

            BytesResult.SetValue(CC, i + Base)

            i = i + 1
            j = j + 1

        End While

        Return BytesResult

    End Function

    Public Function EncryptStringSHA1(ByVal Str As String) As String

        Dim sha As New SHA1CryptoServiceProvider ' declare sha as a new SHA1CryptoServiceProvider
        Dim bytesToHash() As Byte ' and here is a byte variable

        bytesToHash = System.Text.Encoding.UTF8.GetBytes(Str) ' covert the password into ASCII code

        bytesToHash = sha.ComputeHash(bytesToHash) ' this is where the magic starts and the encryption begins

        Return Convert.ToBase64String(bytesToHash)

        Dim result As String = ""

        For Each b As Byte In bytesToHash
            result += b.ToString("x2")
        Next

        Return result

    End Function

    Sub ParseTableDefinition(ByRef PageHeader As PageParams)

        Dim ParsedString = ParserServices.ParsesClass.ParseString(PageHeader.TableDefinition)

        PageHeader.Fields = New List(Of TableFields)

        Dim RowSize = 1

        Dim TableName = ParsedString.Item(0).Item(0).ToString.Replace("""", "").ToUpper

        PageHeader.TableName = TableName

        For Each a In ParsedString.Item(0).Item(2)
            If Not a.IsList Then
                Continue For
            End If

            Dim Field = New TableFields
            Field.Name = a.Item(0).ToString.Replace("""", "")
            Field.Type = a.Item(1).ToString.Replace("""", "")
            Field.CouldBeNull = a.Item(2).ToString
            Field.Length = a.Item(3).ToString
            Field.Precision = a.Item(4).ToString


            Dim FieldSize = Field.CouldBeNull

            If Field.Type = "B" Then
                FieldSize = FieldSize + Field.Length
            ElseIf Field.Type = "L" Then
                FieldSize = FieldSize + 1
            ElseIf Field.Type = "N" Then
                FieldSize = FieldSize + Math.Truncate((Field.Length + 2) / 2)
            ElseIf Field.Type = "NC" Then
                FieldSize = FieldSize + Field.Length * 2
            ElseIf Field.Type = "NVC" Then
                FieldSize = FieldSize + Field.Length * 2 + 2
            ElseIf Field.Type = "RV" Then
                FieldSize = FieldSize + 16
            ElseIf Field.Type = "I" Then
                FieldSize = FieldSize + 8
            ElseIf Field.Type = "T" Then
                FieldSize = FieldSize + 8
            ElseIf Field.Type = "DT" Then
                FieldSize = FieldSize + 7
            ElseIf Field.Type = "NT" Then
                FieldSize = FieldSize + 8
            End If

            Field.Size = FieldSize
            Field.Offset = RowSize

            RowSize = RowSize + FieldSize

            PageHeader.Fields.Add(Field)

        Next

        PageHeader.RowSize = RowSize

        '{"Files",118,119,96}
        'Данные, BLOB, индексы

        Dim BlockData = Convert.ToInt32(ParsedString.Item(0).Item(5).Item(1).ToString)
        Dim BlockBlob = Convert.ToInt32(ParsedString.Item(0).Item(5).Item(2).ToString)

        PageHeader.BlockData = BlockData
        PageHeader.BlockBlob = BlockBlob

    End Sub

End Module
