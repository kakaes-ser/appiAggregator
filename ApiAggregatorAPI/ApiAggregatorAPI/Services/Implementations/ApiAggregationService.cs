using ApiAggregatorAPI.Contracts.ApiClient;
using ApiAggregatorAPI.Contracts.BusinessModels;
using ApiAggregatorAPI.Contracts.Settings;
using ApiAggregatorAPI.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Services
{
	public class ApiAggregationService : IApiAggregationService
	{
		private readonly AppSettings _AppSettings;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly IRequestService _requestService;

		public ApiAggregationService(IRequestService requestService,
			IOptions<AppSettings> appsettings)
		{
			_requestService = requestService;
			_AppSettings = appsettings.Value;
			_jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
		}

		public async Task<ApiAggregationResult> AggregateDataAsync()
		{
			var executeTasks = new List<Task<(string ApiName, ApiCallResult Result)>>();
			if (_AppSettings != null && _AppSettings.ApiClientSettings != null && _AppSettings.ExternalApis.Any())
			{
				foreach (var externalApi in _AppSettings.ExternalApis)
				{
					if (!string.IsNullOrWhiteSpace(externalApi.Name) && !string.IsNullOrWhiteSpace(externalApi.ApiEndPoint) && !string.IsNullOrWhiteSpace(externalApi.Action))
					{
						_requestService.InitializeRestClient(externalApi.Name, externalApi.ApiEndPoint, _AppSettings.ApiClientSettings.TimeOutInSeconds);
						var queryParams = externalApi.ApiFilters.ToDictionary(f => f.Key, f => f.Value);

						executeTasks.Add(Task.Run(async () =>
						{
							var result = await _requestService.ExecuteWithRetry(() => _requestService.ExecuteAsync(externalApi, queryParams), externalApi.Name, _AppSettings.ApiClientSettings.MaxRetryCount ?? 0, _AppSettings.ApiClientSettings.DelayInSeconds ?? 0);
							return (externalApi.Name, result);
						}));
					}
				}
			}

			var results = await Task.WhenAll(executeTasks);

			var resultDict = results.ToDictionary(r => r.ApiName, r => r.Result);
			return ParseResults(results, resultDict);
		}

		private ApiAggregationResult ParseResults((string ApiName, ApiCallResult Result)[] results, Dictionary<string, ApiCallResult> resultDict)
		{
			resultDict.TryGetValue(ResolveType(ResultType.Weather), out var weatherResult);
			resultDict.TryGetValue(ResolveType(ResultType.News), out var newsResult);
			resultDict.TryGetValue(ResolveType(ResultType.Library), out var libraryResult);

			var aggregatedData = new ApiAggregationResult
			{
				WeatherResults = weatherResult?.Errors is null && weatherResult.Data is not null ? JsonSerializer.Deserialize<WeatherResults>(weatherResult.Data, _jsonSerializerOptions) : new(),

				NewsResults = newsResult?.Errors is null && newsResult.Data is not null ? JsonSerializer.Deserialize<NewsResults>(newsResult.Data, _jsonSerializerOptions) : new(),

				LibraryResults = libraryResult?.Errors is null && libraryResult.Data is not null ? JsonSerializer.Deserialize<LibraryResults>(libraryResult.Data, _jsonSerializerOptions) : new(),

				Errors = results.SelectMany(r => r.Result.Errors ?? new List<string>()).ToList()
			};

			return aggregatedData;

			string ResolveType(ResultType resultType)
			{
				return _AppSettings.Categories.SingleOrDefault(s => s.Key == (int)resultType).Type;
			}
		}
	}
}
