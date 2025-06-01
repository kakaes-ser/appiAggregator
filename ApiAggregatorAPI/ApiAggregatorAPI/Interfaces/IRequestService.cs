using ApiAggregatorAPI.Contracts.ApiClient;
using System;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Interfaces
{
	public interface IRequestService
	{
		Task<ApiCallResult> ExecuteWithRetry(Func<Task<string>> apiCall, string apiName, int maxRetries, int delay);
	}
}
