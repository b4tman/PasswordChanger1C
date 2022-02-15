using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PasswordChanger1C.ParserServices
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
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
            return IsList switch
            {
                false => ItemValue,
                true => string.Concat("{", string.Join(",", this), "}")
            };
        }

        public bool IsList
        {
            get => Is_List;
        }

        public void Add(string itemValue)
        {
            Add(new ParserList(itemValue));
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }

    public class ParsesClass
    {
        public static ParserList ParseString(in string Str)
        {
            using var str_reader = new StringReader(Str);
            var List = ParseStringInternal(str_reader);
            return List;
        }

        /// <summary>
        /// characters iterator for text reader
        /// </summary>
        /// <param name="str_reader">text reader</param>
        /// <returns></returns>
        private static IEnumerable<char> GetChars(TextReader str_reader)
        {
            int chunk = str_reader.Read();
            while (chunk != -1)
            {
                yield return (char)chunk;
                chunk = str_reader.Read();
            }
        }

        private enum ParsingTokenType
        {
            Open,
            Close,
            Separator,
            Quote,
            Value,
            Ignore
        }


        /// <summary>
        /// token type by char
        /// </summary>
        /// <param name="chunk">char</param>
        /// <returns></returns>
        private static ParsingTokenType getTokenType(char chunk)
        {
            return chunk switch
            {
                '{' => ParsingTokenType.Open,
                '}' => ParsingTokenType.Close,
                ',' => ParsingTokenType.Separator,
                '"' => ParsingTokenType.Quote,
                '\r' => ParsingTokenType.Ignore,
                '\n' => ParsingTokenType.Ignore,
                '\uFEFF' => ParsingTokenType.Ignore,
                _ => ParsingTokenType.Value
            };
        }

        /// <summary>
        /// brackets parser
        /// </summary>
        /// <param name="str_reader">text reader</param>
        /// <returns></returns>
        private static ParserList ParseStringInternal(TextReader str_reader)
        {
            var list = new ParserList();
            ParsingTokenType type;
            var cur_value = "";
            var has_value = false;
            var is_quoted = false;

            foreach (var chunk in GetChars(str_reader))
            {
                type = getTokenType(chunk);

                // check if token is quoted (value)
                if (is_quoted && ParsingTokenType.Quote != type)
                {
                    type = ParsingTokenType.Value;
                }

                switch (type)
                {
                    case ParsingTokenType.Open:
                        list.Add(ParseStringInternal(str_reader));
                        has_value = false;
                        break;
                    case ParsingTokenType.Close:
                        if (has_value)
                        {
                            list.Add(cur_value);
                            cur_value = "";
                            has_value = false;
                        }
                        return list;
                    case ParsingTokenType.Separator:
                        if (has_value)
                        {
                            list.Add(cur_value);
                            cur_value = "";
                            has_value = false;
                        }                      
                        break;
                    case ParsingTokenType.Quote:
                        cur_value += chunk;
                        is_quoted = !is_quoted;
                        break;
                    case ParsingTokenType.Value:
                        has_value = true;
                        cur_value += chunk;
                        break;
                    //case ParsingTokenType.Ignore: continue;
                    default: continue;
                }
            }
            return list;
        }
    }
}