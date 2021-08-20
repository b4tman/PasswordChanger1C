using System.IO;

namespace PasswordChanger1C
{
    public static partial class AccessFunctions
    {
        public class InfobaseBinaryReader: BinaryReader
        {
            /// <summary>
            ///  Error when Read returned not expected size of bytes
            /// </summary>
            public class PageReadException : IOException
            {
                public PageReadException(string message) : base(message)
                {
                }
                public PageReadException(int PageSize, int ReadedSize) : base($"Read Page with size {PageSize} returned {ReadedSize} bytes")
                {
                }
            }

            /// <summary>
            ///  Error when PageSize not set
            /// </summary>
            public class PageSizeNotSetException : IOException
            {
                public PageSizeNotSetException(string message) : base(message)
                {
                }
                public PageSizeNotSetException() : this("PageSize not set")
                {
                }
            }

            public int PageSize { get; set; }

            public InfobaseBinaryReader(Stream input): base(input)
            {                    
            }

            public InfobaseBinaryReader(Stream input, int _PageSize) : base(input)
            {
                PageSize = _PageSize;
            }

            public long Seek(long offset, SeekOrigin origin)
            {
                return BaseStream.Seek(offset, origin);
            }

            public void SeekToPage(long PageNumber)
            {
                Seek(PageNumber * PageSize, SeekOrigin.Begin);
            }

            public override byte[] ReadBytes(int count)
            {
                var result = base.ReadBytes(count);
                if (result.Length != count) throw new PageReadException(result.Length, count);
                return result;
            }

            public byte[] ReadPage()
            {
                if (PageSize == 0) throw new PageSizeNotSetException();
                return ReadBytes(PageSize);
            }

            public byte[] ReadPage(long PageNumber)
            {
                SeekToPage(PageNumber);
                return ReadPage();
            }

        }
    }
}