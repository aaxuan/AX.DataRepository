using System.Collections.Generic;

namespace AX.DataRepository.Models
{
    public class PageResult<T>
    {
        public List<T> Data { get; set; }

        public int TotalCount { get; set; }

        public int PageIndex { get; set; }

        public int PageItemCount { get; set; }
    }
}