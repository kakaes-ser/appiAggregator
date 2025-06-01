using System;

namespace ApiAggregatorAPI.Contracts.BusinessModels
{
	public class Article
	{
		public string Author { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Url { get; set; }
		public string UrlToImage { get; set; }
		public DateTime PaublishedAt { get; set; }
		public string Content { get; set; }
		public Source SourceSource { get; set; }
	}
}
