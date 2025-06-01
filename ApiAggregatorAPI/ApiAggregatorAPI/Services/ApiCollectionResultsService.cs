using ApiAggregatorAPI.Contracts.BusinessModels;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Services
{
	public class ApiCollectionResultsService
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public ApiCollectionResultsService(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		public async Task<ApiAggregationResult> AggregateDataAsync()
		{
			var client = _httpClientFactory.CreateClient();
			var weatherTask = client.GetAsync("https://api.weather.com/v1/current"); // Example URL
			var newsTask = client.GetAsync("https://api.news.com/latest"); // Example URL
			await Task.WhenAll(weatherTask, newsTask);
			return null;
		}
	}
}
