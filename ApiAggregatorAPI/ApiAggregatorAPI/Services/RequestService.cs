using ApiAggregatorAPI.Contracts;
using ApiAggregatorAPI.Contracts.ApiClient;
using ApiAggregatorAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Services
{
	public class RequestService : IRequestService
	{
		private readonly IPerformanceLogService _performanceLogService;
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
	}
}
