using ApiAggregatorAPI.Contracts.BusinessModels;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Interfaces
{
	public interface IApiAggregationService
	{
		Task<ApiAggregationResult> AggregateDataAsync(SearchFilters filters);
	}
}
