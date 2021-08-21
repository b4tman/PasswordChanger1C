using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PasswordChanger1C
{
    public static partial class AccessFunctions
    {
        public class InfobaseBinaryReader : BinaryReader
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
            ///  Error when buffer length not enought
            /// </summary>
            public class ReadBufferLengthException : IOException
            {
                public ReadBufferLengthException(string message) : base(message)
                {
                }

                public ReadBufferLengthException(int ExpectedSize, int ActualSize) : base($"Attempted to read {ExpectedSize} bytes to buffer with length {ActualSize} bytes")
                {
                }
            }

            public int PageSize { get; set; }

            public InfobaseBinaryReader(Stream input) : base(input)
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

            /// <summary>
            /// Read Page to buffer, starting at index (of buffer)
            /// </summary>
            /// <param name="PageNumber"></param>
            /// <param name="buffer">output buffer</param>
            /// <param name="index">buffer offset</param>
            public void ReadPageTo(long PageNumber, ref byte[] buffer, int index)
            {
                if (buffer.Length - index < PageSize) throw new ReadBufferLengthException(PageSize, buffer.Length - index);
                var PageBuffer = ReadPage(PageNumber);
                PageBuffer.CopyTo(buffer.AsMemory(index));
            }

            /// <summary>
            /// read all pages from list to a new buffer
            /// </summary>
            /// <param name="Pages">list of pages</param>
            /// <returns>readed bytes</returns>
            public byte[] ReadPages(in List<long> Pages)
            {
                byte[] buffer = new byte[Pages.Count * PageSize];
                int offset = 0;
                foreach (long Page in Pages)
                {
                    ReadPageTo(Page, ref buffer, offset);
                    offset += PageSize;
                }
                return buffer;
            }

            /// <summary>
            /// read all pages from list of storage tables (StorageTables[]->DataBlocks[])
            /// </summary>
            /// <param name="StorageTables">list of storage tables</param>
            /// <returns>readed bytes</returns>
            public byte[] ReadPages(in List<StorageTable> StorageTables)
            {
                var Pages = StorageTables.Aggregate(new List<long>(),
                    (Pages, ST) =>
                    {
                        Pages.AddRange(ST.DataBlocks);
                        return Pages;
                    }
                );
                return ReadPages(Pages);
            }
        }
    }
}