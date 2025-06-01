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
		private readonly IPerformanceLogService _performanceService;
		private readonly AppSettings _AppSettings;
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private List<(string ApiName, RestClient Client)> _restClients;

		private bool _disposed = false;

		public ApiAggregationService(ICacheService cacheService, IPerformanceLogService performanceService,
			IOptions<AppSettings> appsettings)
		{
			_performanceService = performanceService;
			_AppSettings = appsettings.Value;
			_jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
		}

		public async Task<ApiAggregationResult> AggregateDataAsync()
		{
			var executeTasks = new List<Task<(string ApiName, ApiCallResult Result)>>();

			foreach (var externalApi in _AppSettings.ExternalApis)
			{
				InitializeRestClient(externalApi.Name);
				var queryParams = externalApi.ApiFilters.ToDictionary(f => f.Key, f => f.Value);

				executeTasks.Add(Task.Run(async () =>
				{
					var result = await ExecuteWithRetry(() => Execute(externalApi, queryParams, externalApi.ApiKey), externalApi.Name);
					return (externalApi.Name, result);
				}));
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
				WeatherResults = weatherResult?.Errors is null ? JsonSerializer.Deserialize<WeatherResults>(weatherResult.Data, _jsonSerializerOptions) : new(),

				NewsResults = newsResult?.Errors is null ? JsonSerializer.Deserialize<NewsResults>(newsResult.Data, _jsonSerializerOptions) : new(),

				LibraryResults = libraryResult?.Errors is null ? JsonSerializer.Deserialize<LibraryResults>(libraryResult.Data, _jsonSerializerOptions) : new(),

				Errors = results.SelectMany(r => r.Result.Errors ?? new List<string>()).ToList()
			};

			return aggregatedData;

			string ResolveType(ResultType resultType)
			{
				return _AppSettings.Categories.SingleOrDefault(s => s.Key == (int)resultType).Type;
			}
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

		private void InitializeRestClient(string apiName)
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


			(_restClients ??= new()).Add((apiName, new(externalApi.ApiEndPoint)));
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
			return _restClients.SingleOrDefault(s => s.ApiName == apiName).Client;
		}
		public void Dispose()
		{
			if (!_disposed)
			{
				if (_restClients is not null)
				{
					foreach (var restClient in _restClients)
					{
						restClient.Client.Dispose();
					}
				}
			}
		}
	}
}
