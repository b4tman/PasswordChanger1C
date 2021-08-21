using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PasswordChanger1C
{
    public static partial class AccessFunctions
    {
        public class InfobaseBinaryWriter : BinaryWriter
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

            /// <summary>
            ///  Error when attempted to write full page data from buffer with size lower than page size
            /// </summary>
            public class WriteFullPageSizeException : IOException
            {
                public WriteFullPageSizeException(string message) : base(message)
                {
                }

                public WriteFullPageSizeException(int PageSize, int DataLength) : base($"Attempted to write full page data from buffer with size {DataLength} to page with size {PageSize}")
                {
                }
            }

            public int PageSize { get; set; }

            public InfobaseBinaryWriter(Stream output) : base(output)
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

            /// <summary>
            /// Write data to a Page
            /// </summary>
            /// <param name="PageNumber">page number</param>
            /// <param name="buffer">data bufer</param>
            public void WriteToPage(long PageNumber, byte[] buffer)
            {
                if (PageSize == 0) throw new PageSizeNotSetException();
                if (buffer.Length > PageSize) throw new WritePageSizeException(PageSize, buffer.Length);

                SeekToPage(PageNumber);
                Write(buffer);
            }

            /// <summary>
            /// Write data (in buffer at index with count) to a page
            /// </summary>
            /// <param name="PageNumber">page number</param>
            /// <param name="buffer">data buffer</param>
            /// <param name="index">data buffer start index</param>
            /// <param name="count">count of data to write</param>
            public void WriteToPage(long PageNumber, byte[] buffer, int index, int count)
            {
                if (PageSize == 0) throw new PageSizeNotSetException();
                if (count > PageSize) throw new WritePageSizeException(PageSize, count);

                SeekToPage(PageNumber);
                Write(buffer, index, count);
            }

            /// <summary>
            /// Write data (in buffer at index) to a page, count=PageSize
            /// </summary>
            /// <param name="PageNumber">page number</param>
            /// <param name="buffer">data buffer</param>
            /// <param name="index">data buffer start index</param>
            public void WritePage(long PageNumber, byte[] buffer, int index)
            {
                if (buffer.Length - index < PageSize) throw new WritePageSizeException(PageSize, buffer.Length - index);
                WriteToPage(PageNumber, buffer, index, PageSize);
            }

            /// <summary>
            /// Write data(in buffer at index) to pages, size= PageSize*Pages.Count
            /// </summary>
            /// <param name="Pages">list of page numbers</param>
            /// <param name="buffer">data buffer</param>
            /// <param name="index">data buffer start index</param>
            public void WritePages(in List<long> Pages, byte[] buffer, int index = 0)
            {
                foreach (var Page in Pages)
                {
                    WritePage(Page, buffer, index);
                    index += PageSize;
                }
            }

            /// <summary>
            /// Write data(in buffer at index) to pages (from list of storage tables (StorageTables[]->DataBlocks[])), size= PageSize*Pages.Count
            /// </summary>
            /// <param name="StorageTables"></param>
            /// <param name="buffer">data buffer</param>
            /// <param name="index">data buffer start index</param>
            public void WritePages(in List<StorageTable> StorageTables, byte[] buffer, int index = 0)
            {
                var Pages = StorageTables.Aggregate(new List<long>(),
                    (Pages, ST) =>
                    {
                        Pages.AddRange(ST.DataBlocks);
                        return Pages;
                    }
                );
                WritePages(Pages, buffer, index);
            }
        }
    }
}