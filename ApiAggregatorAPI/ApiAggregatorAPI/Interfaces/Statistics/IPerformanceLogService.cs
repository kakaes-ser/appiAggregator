using ApiAggregatorAPI.Contracts.BusinessModels;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Interfaces
{
	public interface IPerformanceLogService
	{
		Task UpdatePerformanceStats(string apiName, double elapsedTime);
		Task<PerformanceStats> GetApiPerformanceStats(string apiName);
	}
}
