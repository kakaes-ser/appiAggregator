using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiAggregatorAPI.Contracts.BusinessModels
{
	public class LibraryResults
	{
		[JsonPropertyName("numFound")]
		public int NumFound { get; set; }

		[JsonPropertyName("start")]
		public int Start { get; set; }

		[JsonPropertyName("numFoundExact")]
		public bool NumFoundExact { get; set; }

		[JsonPropertyName("num_found")]
		public int NumFoundUnderscore { get; set; }

		[JsonPropertyName("documentation_url")]
		public string DocumentationUrl { get; set; }

		[JsonPropertyName("q")]
		public string Query { get; set; }

		[JsonPropertyName("offset")]
		public object Offset { get; set; }

		[JsonPropertyName("docs")]
		public List<BookDoc> Docs { get; set; }
	}
}
