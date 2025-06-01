using System;

namespace ApiAggregatorAPI.Contracts.BusinessModels
{
	public class SearchFilters
	{
		public string NewsKey { get; set; }
		public DateTime NewsDateFrom { get; set; }
		public DateTime NewsDateTo { get; set; }
		public decimal Longitude { get; set; }
		public decimal Latitude { get; set; }
		public string LibraryKey { get; set; }
		public string LibraryPlace { get; set; }
		public int LibraryPostsLimit { get; set; }
	}
}
