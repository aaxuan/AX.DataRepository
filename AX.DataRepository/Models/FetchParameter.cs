using System.Collections.Generic;
using System.Linq;

namespace AX.DataRepository.Models
{
    public class FetchParameter
    {
        public string Selectfileld { get; set; }

        public List<FetchFilter> WhereFilters { get; set; } = new List<FetchFilter>();

        public string GroupBy { get; set; }

        public List<FetchFilter> HavingFilters { get; set; } = new List<FetchFilter>();

        public string OrderBy { get; set; }

        public int PageIndex { get; set; }

        public int PageItemCount { get; set; }

        public bool UsePage { get { if (PageItemCount == -1) { return false; } return true; } }
        public bool HasWhereFilters { get { return WhereFilters.Count(p => p.IsValid) > 0; } }
        public bool HasHavingFilters { get { return HavingFilters.Count(p => p.IsValid) > 0; } }
    }

    public class FetchFilter
    {
        public string FilterName { get; set; }

        public string FilterType { get; set; }

        public object FilterValue { get; set; }

        public bool IsValid { get { if (string.IsNullOrWhiteSpace(FilterName) || string.IsNullOrWhiteSpace(FilterType)) { return false; } return true; } }
    }
}