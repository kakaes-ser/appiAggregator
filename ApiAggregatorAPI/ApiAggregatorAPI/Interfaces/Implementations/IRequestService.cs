using ApiAggregatorAPI.Contracts.ApiClient;
using ApiAggregatorAPI.Contracts.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Interfaces
{
	public interface IRequestService
	{
		Task<ApiCallResult> ExecuteWithRetry(Func<Task<string>> apiCall, string apiName, int maxRetries, int delay);
		Task<string> ExecuteAsync(ExternalApi externalApi, Dictionary<string, string> queryParameters);
		void InitializeRestClient(string apiName, string url, int timeOut);
	}
}
