using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace PasswordChanger1C.Tests
{
    using static AccessFunctions;

    public class InfobaseBinaryReader_Tests
    {
        private static byte[] Hex(string _hex)
        {
            string hex = _hex.Replace(" ", "");
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// generate test bytes (hex enumerated)
        /// </summary>
        /// <param name="PageSize"></param>
        /// <param name="PagesCount"></param>
        /// <returns>byte array</returns>
        /// <example>
        ///   GenTestData(10, 2) => (hex):
        ///   | 10 11 12 13 14 15 16 17 18 19 |
        ///   | 20 21 22 23 24 25 26 27 28 29 |
        /// </example>
        private static byte[] GenTestData(int PageSize, int PagesCount)
        {
            IEnumerable<byte> GenRow(int len, byte add)
            {
                return Enumerable.Range(0, len)
                                 .Select(x => Convert.ToByte(x + add));
            }
            return Enumerable.Range(0, PagesCount).Aggregate(new List<byte>(),
                    (Data, row_num) =>
                    {
                        Data.AddRange(GenRow(PageSize, Convert.ToByte(row_num * 16)));
                        return Data;
                    }
            ).ToArray();
        }

        private static IEnumerable<T[]> SplitByLength<T>(T[] arr, int maxLength)
        {
            for (int index = 0; index < arr.Length; index += maxLength)
            {
                yield return arr.Skip(index).Take(Math.Min(maxLength, arr.Length - index)).ToArray();
            }
        }

        [Fact()]
        public void ReadPages_Seq()
        {
            const int PageSize = 10;
            const int PagesCount = 10;
            long[] readPages = { 3, 4, 5 };

            byte[] Data = GenTestData(PageSize, PagesCount);

            byte[] DataReaded;
            using (var test_stream = new MemoryStream(Data))
            {
                using var reader = new InfobaseBinaryReader(test_stream, PageSize);
                DataReaded = reader.ReadPages(readPages.ToList());
            }

            Assert.Collection(SplitByLength(DataReaded, PageSize),
                (Page) => Assert.Equal(Hex("30 31 32 33 34 35 36 37 38 39"), Page),
                (Page) => Assert.Equal(Hex("40 41 42 43 44 45 46 47 48 49"), Page),
                (Page) => Assert.Equal(Hex("50 51 52 53 54 55 56 57 58 59"), Page)
            );
        }

        [Fact()]
        public void ReadPages_SkipPage()
        {
            const int PageSize = 10;
            const int PagesCount = 10;
            long[] readPages = { 3, 5, 7 };

            byte[] Data = GenTestData(PageSize, PagesCount);

            byte[] DataReaded;
            using (var test_stream = new MemoryStream(Data))
            {
                using var reader = new InfobaseBinaryReader(test_stream, PageSize);
                DataReaded = reader.ReadPages(readPages.ToList());
            }

            Assert.Collection(SplitByLength(DataReaded, PageSize),
                (Page) => Assert.Equal(Hex("30 31 32 33 34 35 36 37 38 39"), Page),
                (Page) => Assert.Equal(Hex("50 51 52 53 54 55 56 57 58 59"), Page),
                (Page) => Assert.Equal(Hex("70 71 72 73 74 75 76 77 78 79"), Page)
            );
        }
    }
}