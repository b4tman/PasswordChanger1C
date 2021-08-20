using System.IO;

namespace PasswordChanger1C
{
    public class InfobaseBinaryWriter: BinaryWriter
    {
        /// <summary>
        ///  Error when attempted to write data with size greater than page size
        /// </summary>
        public class WritePageSizeException : IOException
        {
            public WritePageSizeException(string message) : base(message)
            {
            }
            public WritePageSizeException(int PageSize, int DataLength) : base($"Attempted to write data with size {DataLength} to page with size {PageSize}")
            {
            }
        }

        public int PageSize { get; set; }

        public InfobaseBinaryWriter(Stream output): base(output)
        {
        }

        public InfobaseBinaryWriter(Stream output, int _PageSize) : base(output)
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

        public void WriteToPage(long PageNumber, byte[] buffer)
        {
            if (buffer.Length > PageSize) throw new WritePageSizeException(PageSize, buffer.Length);

            SeekToPage(PageNumber);
            Write(buffer);
        }

        public void WriteToPage(long PageNumber, byte[] buffer, int index, int count)
        {
            if (count > PageSize) throw new WritePageSizeException(PageSize, count);

            SeekToPage(PageNumber);
            Write(buffer, index, count);
        }
    }
}