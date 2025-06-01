using System.Collections.Generic;

namespace ApiAggregatorAPI.Contracts.Settings
{
	public class AppSettings
	{
		public ApiClientSettings ApiClientSettings { get; set; }
		public CacheSettings CacheSettings { get; set; }
		public RequestStatsSettings RequestStatsSettings { get; set; }
		public List<ExternalApi> ExternalApis { get; set; }
		public List<Category> Categories { get; set; }
	}

	public class ExternalApi
	{
		public string Name { get; set; }
		public string ApiEndPoint { get; set; }
		public string Action { get; set; }
		public string ApiKey { get; set; }
		public bool AddToHeader { get; set; }
		public List<ApiFilter> ApiFilters { get; set; }
	}

	public class ApiClientSettings
	{
		public int MaxRetryCount { get; set; }
		public int DelayInSeconds { get; set; }
		public int TimeOutInSeconds { get; set; }
	}

	public class CacheSettings
	{
		public string ResultsKey { get; set; }
		public int ResultsExpirationMinutes { get; set; }
		public int StatisticsExpirationMinutes { get; set; }
	}

	public class RequestStatsSettings
	{
		public int SlowRequestsMs { get; set; }
		public int FastRequestsMs { get; set; }
	}

	public class ApiFilter
	{
		public string Key { get; set; }
		public string Value { get; set; }
	}
	public class Category
	{
		public int Key { get; set; }
		public string Type { get; set; }
	}
}
