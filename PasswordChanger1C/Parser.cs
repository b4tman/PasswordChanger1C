using System.Collections.Generic;

namespace PasswordChanger1C.ParserServices
{
    public class ParserList : List<ParserList>
    {
        private readonly string ItemValue;
        private readonly bool Is_List;

        public ParserList(string itemValue)
        {
            ItemValue = itemValue;
            Is_List = false;
        }

        public ParserList()
        {
            ItemValue = "";
            Is_List = true;
        }

        public override string ToString()
        {
            if (!IsList)
            {
                return ItemValue;
            }
            else
            {
                return base.ToString();
            }
        }

        public bool IsList
        {
            get
            {
                return Is_List;
            }
        }

        public void Add(string itemValue)
        {
            Add(new ParserList(itemValue));
        }
    }

    public class ParsesClass
    {
        public static ParserList ParseString(string Str)
        {
            var Arr = Str.Split(',');
            int argPosition = 0;
            var List = ParseStringInternal(Arr, ref argPosition, Arr.Length - 1);
            return List;
        }

        private static ParserList ParseStringInternal(string[] Arr, ref int Position, int ArrLength)
        {

            // TODO - не обрабатываются ситуации с двойными кавычками и переносами строк в тексте 

            var List = new ParserList();
            while (true)
            {
                string Val = Arr[Position].Trim();
                if (Val.StartsWith("{"))
                {
                    Arr[Position] = Val.Substring(1);
                    List.Add(ParseStringInternal(Arr, ref Position, ArrLength));
                }
                else if (string.IsNullOrEmpty(Val))
                {
                    Position = Position + 1;
                }
                else
                {
                    int Pos = Val.IndexOf("}");
                    if (Pos > -1)
                    {
                        string Vl2 = Val.Substring(0, Pos);
                        if (!string.IsNullOrEmpty(Vl2))
                        {
                            List.Add(Vl2);
                        }

                        Arr[Position] = Val.Substring(Pos + 1);
                        break;
                    }
                    else
                    {
                        List.Add(Val);
                        Position = Position + 1;
                    }
                }

                if (Position >= ArrLength)
                {
                    break;
                }
            }

            return List;
        }
    }
}