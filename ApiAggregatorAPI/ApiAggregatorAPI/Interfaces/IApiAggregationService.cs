using ApiAggregatorAPI.Contracts.BusinessModels;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Interfaces
{
	public interface IApiAggregationService
	{
		Task<ApiAggregationResult> AggregateDataAsync();
		//Task<ApiCallResult> ExecuteWithRetry(Func<Task<string>> apiCall, string apiName);
	}
}
