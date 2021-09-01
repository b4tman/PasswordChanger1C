using Xunit;
using PasswordChanger1C.ParserServices;

namespace PasswordChanger1C.ParserServices.Tests
{
    public class ParsesClass_Tests
    {
        // https://en.wikipedia.org/wiki/Byte_order_mark
        private const string UTF16BE_BOM = "\uFEFF";

        private const string TestStr = UTF16BE_BOM +
            "﻿{4a4fe769-57db-450e-938d-8cb13d175539,\"User\",\"�\",\"Descr\",00000000-0000-0000-0000-000000000000,\r\n" +
            "{1,4d288b2d-1ab3-4139-abcc-4fbe71b85d5b},cf34b2a5-8606-4b59-b43f-7f5ba09e0032,1,1,,0,0," +
            "\"NWoZK3kTsExUV00Ywo1G5jlUKKs=\",\"NWoZK3kTsExUV00Ywo1G5jlUKKs=\",2,1,20210802185345,0,0,\r\n{0},1,\r\n{0},1,0,1,\"\"}\0";

        [Fact()]
        public void ParseString_Test()
        {
            var result = ParsesClass.ParseString(TestStr);
            Assert.Collection(result,
                (Root) => Assert.Collection(Root,
                    (Item) => Assert.Equal("4a4fe769-57db-450e-938d-8cb13d175539", Item.ToString()),
                    (Item) => Assert.Equal("\"User\"", Item.ToString()),
                    (Item) => Assert.Equal("\"�\"", Item.ToString()),
                    (Item) => Assert.Equal("\"Descr\"", Item.ToString()),
                    (Item) => Assert.Equal("00000000-0000-0000-0000-000000000000", Item.ToString()),
                    (Item) => Assert.Collection(Item,
                        (SubItem) => Assert.Equal("1", SubItem.ToString()),
                        (SubItem) => Assert.Equal("4d288b2d-1ab3-4139-abcc-4fbe71b85d5b", SubItem.ToString())
                    ),
                    (Item) => Assert.Equal("cf34b2a5-8606-4b59-b43f-7f5ba09e0032", Item.ToString()),
                    (Item) => Assert.Equal("1", Item.ToString()),
                    (Item) => Assert.Equal("1", Item.ToString()),
                    //(Item) => Assert.Equal("", Item.ToString()), // empty strings are ignored
                    (Item) => Assert.Equal("0", Item.ToString()),
                    (Item) => Assert.Equal("0", Item.ToString()),
                    (Item) => Assert.Equal("\"NWoZK3kTsExUV00Ywo1G5jlUKKs=\"", Item.ToString()),
                    (Item) => Assert.Equal("\"NWoZK3kTsExUV00Ywo1G5jlUKKs=\"", Item.ToString()),
                    (Item) => Assert.Equal("2", Item.ToString()),
                    (Item) => Assert.Equal("1", Item.ToString()),
                    (Item) => Assert.Equal("20210802185345", Item.ToString()),
                    (Item) => Assert.Equal("0", Item.ToString()),
                    (Item) => Assert.Equal("0", Item.ToString()),
                    (Item) => Assert.Collection(Item,
                        (SubItem) => Assert.Equal("0", SubItem.ToString())
                    ),
                    (Item) => Assert.Equal("1", Item.ToString()),
                    (Item) => Assert.Collection(Item,
                        (SubItem) => Assert.Equal("0", SubItem.ToString())
                    ),
                    (Item) => Assert.Equal("1", Item.ToString()),
                    (Item) => Assert.Equal("0", Item.ToString()),
                    (Item) => Assert.Equal("1", Item.ToString())
                    //(Item) => Assert.Equal("\"\"", Item.ToString()) // empty strings are ignored
                )
            );
        }
    }
}