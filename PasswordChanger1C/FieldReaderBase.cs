using System;
using System.Text;

namespace PasswordChanger1C
{
    public static partial class AccessFunctions
    {
        internal class FieldReaderBase
        {
            protected TableFields Field { get; set; }
            protected int FieldOffset { get; set; }

            protected byte[] Data { get; set; }
            protected InfobaseBinaryReader reader { get; set; }
            protected AccessFunctions.PageParams PageHeader { get; set; }

            public FieldReaderBase(InfobaseBinaryReader reader, AccessFunctions.PageParams PageHeader, byte[] data)
            {
                this.reader = reader;
                this.PageHeader = PageHeader;
                this.Data = data;
            }

            public void SetField(TableFields Field, int FieldOffset)
            {
                this.Field = Field;
                this.FieldOffset = FieldOffset;
            }

            protected Boolean Value_Boolean
            {
                get => BitConverter.ToBoolean(Data, FieldOffset + Field.CouldBeNull);
            }

            protected byte[] Value_Bytes
            {
                get
                {
                    string base64str = Convert.ToBase64String(Data, FieldOffset + Field.CouldBeNull, Field.Size - Field.CouldBeNull);
                    return Convert.FromBase64String(base64str);
                }
            }

            protected string Value_FixedString
            {
                get => Encoding.Unicode.GetString(Data, FieldOffset, Field.Size);
            }

            protected string Value_UnlimitedString
            {
                // TODO
                get => "";
            }

            protected string Value_VarString
            {
                get => Encoding.Unicode.GetString(Data, FieldOffset + Field.CouldBeNull + 2, StringLength * 2).Trim();
            }

            protected int StringLength
            {
                get
                {
                    int result = (Data[FieldOffset] + Data[FieldOffset + 1] * 256);
                    result = Math.Min(Field.Size, result);
                    return result;
                }
            }

            protected int Value_Number
            {
                get
                {
                    int result;
                    var StrBuilder = new StringBuilder();
                    for (int i = 0; i < Field.Size; i++)
                    {
                        var character = Convert.ToString(Data[FieldOffset + i], 16);
                        if (character.Length == 1)
                        {
                            StrBuilder.Append("0");
                        }
                        StrBuilder = StrBuilder.Append(character);
                    }

                    var StrNumber = StrBuilder.ToString();
                    string FirstSimbol = StrNumber.Substring(0, 1);
                    StrNumber = StrNumber.Substring(1, Field.Length);
                    if (string.IsNullOrEmpty(StrNumber))
                    {
                        result = 0;
                    }
                    else
                    {
                        result = Convert.ToInt32(StrNumber) / (Field.Precision > 0 ? Field.Precision * 10 : 1);
                        if (FirstSimbol == "0")
                        {
                            result *= -1;
                        }
                    }
                    return result;
                }
            }

            protected DateTime Value_DateTime
            {
                get
                {
                    DateTime result;
                    var BytesDate = new byte[7]; // 7 байт
                    for (int i = 0; i <= 6; i++)
                        BytesDate[i] = Convert.ToByte(Convert.ToString(Data[FieldOffset + i], 16));
                    try
                    {
                        result = new DateTime((BytesDate[0] * 100) + BytesDate[1],
                                                BytesDate[2],
                                                BytesDate[3],
                                                BytesDate[4],
                                                BytesDate[5],
                                                BytesDate[6]);
                    }
                    catch (Exception)
                    {
                        result = new DateTime();
                    }
                    return result;
                }
            }

            public int BlobDataPos
            {
                get => BitConverter.ToInt32(Data, FieldOffset);
            }

            public int BlobDataSize
            {
                get => BitConverter.ToInt32(Data, FieldOffset + 4);
            }

            // двоичные данные неограниченной длины
            protected virtual byte[] Value_BlobData
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public object Value
            {
                get
                {
                    return Field.Type switch
                    {
                        "NC" => Value_FixedString,
                        "NVC" => Value_VarString,
                        "NT" => Value_UnlimitedString,
                        "N" => Value_Number,
                        "I" => Value_BlobData,
                        "DT" => Value_DateTime,
                        "L" => Value_Boolean,
                        "B" => Value_Bytes,
                        _ => null
                    };
                }
            }
        }
    }
}