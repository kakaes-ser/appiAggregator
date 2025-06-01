using ApiAggregatorAPI.Contracts.BusinessModels;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Interfaces
{
	public interface IDataRetrieveService
	{
		Task<ApiAggregationResult> GetData(SearchFilters searchFilters);
	}
}
