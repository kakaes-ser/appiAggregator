using ApiAggregatorAPI.Contracts;
using ApiAggregatorAPI.Contracts.ApiClient;
using ApiAggregatorAPI.Contracts.Settings;
using ApiAggregatorAPI.Interfaces;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Services
{
	public class RequestService : IRequestService, IDisposable
	{
		private readonly IPerformanceLogService _performanceLogService;

		private List<(string ApiName, RestClient Client)> _restClients;

		private bool _disposed = false;
		public RequestService(IPerformanceLogService performanceLogService)
		{
			_performanceLogService = performanceLogService;
		}
		public async Task<ApiCallResult> ExecuteWithRetry(Func<Task<string>> apiCall, string apiName, int maxRetries, int delay)
		{
			List<string> errors = new List<string>();
			int retryCount = 0;

			DateTime startTime = DateTime.Now;

			while (retryCount < maxRetries)
			{
				try
				{
					var result = await apiCall();
					DateTime endTime = DateTime.Now;
					var elapsedTime = (endTime - startTime).TotalMilliseconds;
					await _performanceLogService.UpdatePerformanceStats(apiName, elapsedTime);
					return new() { Data = result };
				}

				catch (ApiClientException ex)
				{
					retryCount++;
					if (ex.ErrorType == ErrorType.Transient)
					{
						await Task.Delay(TimeSpan.FromSeconds(delay));
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

		public async Task<string> ExecuteAsync(ExternalApi externalApi, Dictionary<string, string> queryStringParameters)
		{
			RestClient client = ResolveRestClient(externalApi.Name);
			var request = new RestRequest($"/{externalApi.Action}", Method.Get);

			if (!string.IsNullOrWhiteSpace(externalApi.ApiKey))
			{
				request.AddHeader("x-api-key", externalApi.ApiKey);
			}

			if (queryStringParameters != null)
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

		public void InitializeRestClient(string apiName, string url, int timeOut)
		{
			var options = new RestClientOptions
			{
				Timeout = TimeSpan.FromSeconds(timeOut)
			};


			(_restClients ??= new()).Add((apiName, new(url)));
		}

		private RestClient ResolveRestClient(string apiName)
		{
			return _restClients.SingleOrDefault(s => s.ApiName == apiName).Client;
		}

		private void HandleFailedResponseStatusCodes(RestResponse restResponse)
		{
			switch (restResponse.StatusCode)
			{
				case HttpStatusCode.RequestTimeout:
				case HttpStatusCode.TooManyRequests:
				case HttpStatusCode.BadGateway:
				case HttpStatusCode.ServiceUnavailable:
				case HttpStatusCode.GatewayTimeout:
					throw new ApiClientException(ErrorType.Transient, restResponse.ErrorMessage);
				default:
					throw new ApiClientException(ErrorType.Permanent, restResponse.ErrorMessage);
			}
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
