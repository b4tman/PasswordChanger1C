using System;
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
            public void WriteToPage(long PageNumber, in byte[] buffer)
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
            public void WriteToPage(long PageNumber, in byte[] buffer, int index, int count)
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
            public void WritePage(long PageNumber, in byte[] buffer, int index)
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
            public void WritePages(in List<long> Pages, in byte[] buffer, int index = 0)
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
            public void WritePages(in List<StorageTable> StorageTables, in byte[] buffer, int index = 0)
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

            /// <summary>
            /// Write data from bufer(starting at buffer_index) to pages, starting at pages_offset
            /// (to patch data in pages)
            /// </summary>
            /// <param name="Pages">list of page numbers</param>
            /// <param name="pages_offset">offset in pages</param>
            /// <param name="buffer">data buffer</param>
            /// <param name="buffer_index">optional: data buffer start index</param>
            /// <param name="count">optional: count of data to write</param>
            /// <example> 
            /// input  = (stream_data = "1234567890", page_size=2, pages={1,2,3}, offset=3, buffer="---")
            /// output = (stream_data = "12345---90")
            /// -- pages data: {"12", "34", "56", "78", "90"}
            /// -- start page = 2           ^
            /// -- start page offset = 1      ^
            /// input  = (stream_data = "1234567890", page_size=2, pages={0,2,4}, offset=3, buffer="---")
            /// output = (stream_data = "12345-78--")
            /// </example>
            public void WriteToPagesAt(in List<long> Pages, int pages_offset, in byte[] buffer, int buffer_index=0, int count=0)
            {
                if (0 == count) count = buffer.Length - buffer_index;
                int size_pages = PageSize * Pages.Count - pages_offset;
                if (count > size_pages) throw new WritePageSizeException(size_pages, count);


                int cur_page_offset = pages_offset % PageSize;
                int pages_count = Math.Max(1, count / PageSize + (cur_page_offset > 0 ? 1 : 0));
                var pages_changed = Pages.Skip(pages_offset / PageSize).Take(pages_count);

                foreach (var Page in pages_changed)
                {
                    int chunk = Math.Min(count, PageSize - cur_page_offset);
                    
                    SeekToPage(Page);
                    if (cur_page_offset > 0) Seek(cur_page_offset, SeekOrigin.Current);
                    
                    Write(buffer, buffer_index, chunk);
                    
                    buffer_index += chunk;
                    count -= chunk;
                    cur_page_offset = 0;
                }
            }

            /// <summary>
            /// Write data from bufer(starting at buffer_index) to pages(from list of storage tables (StorageTables[]->DataBlocks[])),
            /// starting at pages_offset (to patch data in pages)
            /// </summary>
            /// <param name="StorageTables"></param>
            /// <param name="pages_offset">offset in pages</param>
            /// <param name="buffer">data buffer</param>
            /// <param name="buffer_index">optional: data buffer start index</param>
            /// <param name="count">optional: count of data to write</param>
            public void WriteToPagesAt(in List<StorageTable> StorageTables, int pages_offset, in byte[] buffer, int buffer_index = 0, int count = 0)
            {
                var Pages = StorageTables.Aggregate(new List<long>(),
                    (Pages, ST) =>
                    {
                        Pages.AddRange(ST.DataBlocks);
                        return Pages;
                    }
                );
                WriteToPagesAt(Pages, pages_offset, buffer, buffer_index, count);
            }
        }
    }
}
