using ApiAggregatorAPI.Contracts.BusinessModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Interfaces
{
	public interface IPerformanceStatisticsService
	{
		Task<List<PerformanceStats>> GetSummary();
	}
}
