using System.Collections.Generic;

namespace ApiAggregatorAPI.Contracts.BusinessModels
{
	public class NewsResults
	{
		public string Status { get; set; }
		public int TotalResults { get; set; }
		public List<Article> Articles { get; set; }
	}
}
