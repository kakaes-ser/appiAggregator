using System;

namespace ApiAggregatorAPI.Contracts.BusinessModels
{
	public class SearchFilters
	{
		public string Keyword { get; set; }
		public DateTime? NewsDateFrom { get; set; }
		public DateTime? NewsDateTo { get; set; }
	}
}
