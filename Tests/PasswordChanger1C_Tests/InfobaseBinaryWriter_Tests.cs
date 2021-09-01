using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace PasswordChanger1C.Tests
{
    using static AccessFunctions;

    public class InfobaseBinaryWriter_Tests
    {
        private static string streamToString(in Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static IEnumerable<string> SplitByLength(string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

        [Fact()]
        public void WritePages_Seq()
        {
            string Output;
            const int PageSize = 10;
            const int PagesCount = 10;
            const int PatchSize = PageSize * 3;
            long[] changePages = { 3, 4, 5 };

            string BlankPage = new String('-', PageSize);
            string Blank = string.Concat(Enumerable.Repeat(BlankPage, PagesCount));
            string Patch = new String('+', PatchSize);
            var Blank_bin = System.Text.Encoding.ASCII.GetBytes(Blank);
            var Patch_bin = System.Text.Encoding.ASCII.GetBytes(Patch);

            using (var test_stream = new MemoryStream(Blank_bin))
            {
                using var writer = new InfobaseBinaryWriter(test_stream, PageSize);
                writer.WritePages(changePages.ToList(), Patch_bin);
                Output = streamToString(test_stream);
            }

            Assert.Collection(SplitByLength(Output, PageSize),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal("++++++++++", Page),
                (Page) => Assert.Equal("++++++++++", Page),
                (Page) => Assert.Equal("++++++++++", Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page)
            );
        }

        [Fact()]
        public void WritePages_SkipPage()
        {
            string Output;
            const int PageSize = 10;
            const int PagesCount = 10;
            const int PatchSize = PageSize * 3;
            long[] changePages = { 3, 5, 7 };

            string BlankPage = new String('-', PageSize);
            string Blank = string.Concat(Enumerable.Repeat(BlankPage, PagesCount));
            string Patch = new String('+', PatchSize);
            var Blank_bin = System.Text.Encoding.ASCII.GetBytes(Blank);
            var Patch_bin = System.Text.Encoding.ASCII.GetBytes(Patch);

            using (var test_stream = new MemoryStream(Blank_bin))
            {
                using var writer = new InfobaseBinaryWriter(test_stream, PageSize);
                writer.WritePages(changePages.ToList(), Patch_bin);
                Output = streamToString(test_stream);
            }

            Assert.Collection(SplitByLength(Output, PageSize),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal("++++++++++", Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal("++++++++++", Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal("++++++++++", Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page)
            );
        }

        [Fact()]
        public void WriteToPagesAt_Seq()
        {
            string Output;
            const int PageSize = 10;
            const int PagesCount = 10;
            const int offset = PageSize + PageSize / 2;
            const int PatchSize = PageSize * 2 + 3;
            long[] changePages = { 2, 3, 4, 5 };

            string BlankPage = new String('#', PageSize);
            string Blank = string.Concat(Enumerable.Repeat(BlankPage, PagesCount));
            string Patch = new String('@', PatchSize);
            var Patch_bin = System.Text.Encoding.ASCII.GetBytes(Patch);

            using (var test_stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(Blank)))
            {
                using var writer = new InfobaseBinaryWriter(test_stream, PageSize);
                writer.WriteToPagesAt(changePages.ToList(), offset, Patch_bin);
                Output = streamToString(test_stream);
            }

            Assert.Collection(SplitByLength(Output, PageSize),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal("#####@@@@@", Page),
                (Page) => Assert.Equal("@@@@@@@@@@", Page),
                (Page) => Assert.Equal("@@@@@@@@##", Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page)
            );
        }

        [Fact()]
        public void WriteToPagesAt_SkipPage()
        {
            string Output;
            const int PageSize = 10;
            const int PagesCount = 10;
            const int offset = PageSize + PageSize / 2;
            const int PatchSize = PageSize * 2 + 3;
            long[] changePages = { 2, 3, 5, 7 };

            string BlankPage = new String('#', PageSize);
            string Blank = string.Concat(Enumerable.Repeat(BlankPage, PagesCount));
            string Patch = new String('@', PatchSize);
            var Patch_bin = System.Text.Encoding.ASCII.GetBytes(Patch);

            using (var test_stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(Blank)))
            {
                using var writer = new InfobaseBinaryWriter(test_stream, PageSize);
                writer.WriteToPagesAt(changePages.ToList(), offset, Patch_bin);
                Output = streamToString(test_stream);
            }

            Assert.Collection(SplitByLength(Output, PageSize),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal("#####@@@@@", Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal("@@@@@@@@@@", Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal("@@@@@@@@##", Page),
                (Page) => Assert.Equal(BlankPage, Page),
                (Page) => Assert.Equal(BlankPage, Page)
            );
        }
    }
}