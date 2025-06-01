using ApiAggregatorAPI.Contracts;
using ApiAggregatorAPI.Contracts.ApiClient;
using ApiAggregatorAPI.Contracts.BusinessModels;
using ApiAggregatorAPI.Contracts.Settings;
using ApiAggregatorAPI.Interfaces;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Services
{
	public class ApiAggregationService : IApiAggregationService, IDisposable
	{
		private readonly ICacheService _cacheService;
		private readonly IPerformanceLogService _performanceService;
		private readonly AppSettings _AppSettings;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly RestClient _weatherClient;
		private readonly RestClient _newsClient;
		private readonly RestClient _libraryClient;

		private bool _disposed = false;

		public ApiAggregationService(ICacheService cacheService, IPerformanceLogService performanceService,
			IOptions<AppSettings> appsettings)
		{
			_cacheService = cacheService;
			_performanceService = performanceService;
			_AppSettings = appsettings.Value;
			_jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

			_weatherClient = InitializeRestClient("weather");
			_newsClient = InitializeRestClient("news");
			_libraryClient = InitializeRestClient("library");
		}

		public async Task<ApiAggregationResult> AggregateDataAsync(SearchFilters searchFilters)
		{
			var cachedResult = await _cacheService.GetCacheAsync<ApiAggregationResult>("aggregatedData");

			if (cachedResult != null)
			{
				return cachedResult;
			}

			List<Task<ApiCallResult>> tasks = new()
			{
				CallWeatherAsync(new (){Longitude  = searchFilters.Latitude, Latitude = searchFilters.Latitude}),
				CallNewsArticleAsync(new(){Keyword = searchFilters.NewsKey, DateFrom = DateOnly.FromDateTime(searchFilters.NewsDateFrom), DateTo = DateOnly.FromDateTime(searchFilters.NewsDateTo)}),
				CallLibraryAsync(new(){Keyword = searchFilters.LibraryKey, Place = searchFilters.LibraryPlace, Limit = searchFilters.LibraryPostsLimit}),
			};

			var results = await Task.WhenAll(tasks);

			WeatherResults weatherResults = results[0]?.Errors is null ? JsonSerializer.Deserialize<WeatherResults>(results[0].Data, _jsonSerializerOptions) : new();
			NewsResults newsResults = results[1]?.Errors is null ? JsonSerializer.Deserialize<NewsResults>(results[1].Data, _jsonSerializerOptions) : new();
			LibraryResults libraryResults = results[2]?.Errors is null ? JsonSerializer.Deserialize<LibraryResults>(results[2].Data, _jsonSerializerOptions) : new();

			//await _cacheService.SetCacheAsync("weather", weatherResults, TimeSpan.FromMinutes(2));
			//await _cacheService.SetCacheAsync("news", newsResults, TimeSpan.FromMinutes(2));
			//await _cacheService.SetCacheAsync("library", libraryResults, TimeSpan.FromMinutes(2));

			var aggregatedData = new ApiAggregationResult
			{
				WeatherResults = weatherResults,
				NewsResults = newsResults,
				LibraryResults = libraryResults,
				Errors = results.Where(w => w.Errors != null).SelectMany(w => w.Errors).ToList()
			};

			await _cacheService.SetCacheAsync("aggregatedData", aggregatedData, TimeSpan.FromMinutes(_AppSettings.CacheSettings.ResultsExpirationMinutes));

			return aggregatedData;
		}

		private async Task<ApiCallResult> CallNewsArticleAsync(NewsFilters filters)
		{
			ExternalApi externalApi = _AppSettings.ExternalApis.FirstOrDefault(f => f.Name == "news");

			Dictionary<string, string> queryStringParameters = new Dictionary<string, string>()
			{
				{"q", filters.Keyword },
				{"from", filters.DateFrom.ToString("yyyy-MM-dd") },
				{"to", filters.DateTo.ToString("yyyy-MM-dd") }
			};

			return await ExecuteWithRetry(() => Execute(externalApi, queryStringParameters, externalApi.ApiKey), externalApi.Name);
		}

		private async Task<ApiCallResult> CallWeatherAsync(WeatherFilters filters)
		{
			ExternalApi externalApi = _AppSettings.ExternalApis.FirstOrDefault(f => f.Name == "weather");

			Dictionary<string, string> queryStringParameters = new Dictionary<string, string>()
			{
				{"lat", filters.Latitude.ToString() },
				{"lon", filters.Longitude.ToString() },
			};

			return await ExecuteWithRetry(() => Execute(externalApi, queryStringParameters, externalApi.ApiKey), externalApi.Name);

		}

		private async Task<ApiCallResult> CallLibraryAsync(LibraryFilters filters)
		{
			ExternalApi externalApi = _AppSettings.ExternalApis.FirstOrDefault(f => f.Name == "library");

			Dictionary<string, string> queryStringParameters = new Dictionary<string, string>()
			{
				{"q", filters.Keyword },
				{"place", filters.Place },
				{"limit", filters.Limit.ToString() }
			};

			return await ExecuteWithRetry(() => Execute(externalApi, queryStringParameters, externalApi.ApiKey), externalApi.Name);
		}

		private async Task<ApiCallResult> ExecuteWithRetry(Func<Task<string>> apiCall, string apiName)
		{
			List<string> errors = new List<string>();
			int retryCount = 0;

			DateTime startTime = DateTime.Now;

			while (retryCount < _AppSettings.ApiClientSettings.MaxRetryCount)
			{
				try
				{
					var result = await apiCall();
					DateTime endTime = DateTime.Now;
					var elapsedTime = (endTime - startTime).TotalMilliseconds;
					await _performanceService.UpdatePerformanceStats(apiName, elapsedTime);
					return new() { Data = result };
				}

				catch (ApiClientException ex)
				{
					retryCount++;
					if (ex.ErrorType == ErrorType.Transient)
					{
						await Task.Delay(TimeSpan.FromSeconds(_AppSettings.ApiClientSettings.DelayInSeconds));
					}
					else
					{
						errors.Add($"Retriving data from {apiName} failed because of: {ex.Message}");
						return new ApiCallResult { Errors = errors };
					}
				}
				catch (Exception ex)
				{
					errors.Add($"Retriving data from {apiName} failed because of: {ex.Message}");
					return new ApiCallResult { Errors = errors };
				}
			}

			return new ApiCallResult { Errors = errors };
		}

		private async Task<string> Execute(ExternalApi externalApi, Dictionary<string, string> queryStringParameters, string apiName)
		{

			RestClient client = ResolveRestClient(externalApi.Name);

			RestRequest request = new($"/{externalApi.Action}", Method.Get);

			if (!string.IsNullOrWhiteSpace(externalApi.ApiKey))
			{
				request.AddHeader("x-api-key", externalApi.ApiKey);
			}

			if (queryStringParameters is not null)
			{
				foreach (var param in queryStringParameters)
				{
					request.AddParameter(param.Key, param.Value, ParameterType.QueryString);
				}
			}
			var response = await client.ExecuteAsync(request);

			if (!response.IsSuccessful)
			{
				HandleFailedResponseStatusCodes(response);
			}

			return response.Content;

		}

		private RestClient InitializeRestClient(string apiName)
		{

			var externalApi = _AppSettings.ExternalApis.FirstOrDefault(f => f.Name == apiName);
			if (externalApi == null)
			{
				throw new Exception($"API configuration for '{apiName}' not found.");
			}
			var options = new RestClientOptions
			{
				BaseUrl = new Uri(externalApi.ApiEndPoint),
				Timeout = TimeSpan.FromSeconds(_AppSettings.ApiClientSettings.TimeOutInSeconds)
			};

			return new RestClient(externalApi.ApiEndPoint);
		}

		private void HandleFailedResponseStatusCodes(RestResponse restResponse)
		{
			switch (restResponse.StatusCode)
			{
				case System.Net.HttpStatusCode.RequestTimeout:
				case System.Net.HttpStatusCode.TooManyRequests:
				case System.Net.HttpStatusCode.BadGateway:
				case System.Net.HttpStatusCode.ServiceUnavailable:
				case System.Net.HttpStatusCode.GatewayTimeout:
					throw new ApiClientException(ErrorType.Transient, restResponse.ErrorMessage);
				default: throw new ApiClientException(ErrorType.Permanent, restResponse.ErrorMessage);
			}
		}

		private RestClient ResolveRestClient(string apiName)
		{
			return apiName switch
			{
				"news" => _newsClient,
				"weather" => _weatherClient,
				"library" => _libraryClient,
				_ => throw new Exception($"Cannot resolve rest client implementation for api {apiName}.")
			};
		}
		public void Dispose()
		{
			if (!_disposed)
			{
				_weatherClient?.Dispose();
				_newsClient?.Dispose();
				_libraryClient?.Dispose();
				_disposed = true;
			}
		}
	}
}
