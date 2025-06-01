using System;

namespace ApiAggregatorAPI.Contracts.BusinessModels
{
    public class NewsFilters
    {
        public string Keyword { get; set; }
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
    }
}
