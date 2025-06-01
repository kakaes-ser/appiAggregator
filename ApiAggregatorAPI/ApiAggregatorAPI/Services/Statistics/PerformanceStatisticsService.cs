using ApiAggregatorAPI.Contracts.BusinessModels;
using ApiAggregatorAPI.Contracts.Settings;
using ApiAggregatorAPI.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Services
{
	public class PerformanceStatisticsService : IPerformanceStatisticsService
	{
		private readonly IPerformanceLogService _performanceService;
		private readonly AppSettings _AppSettings;
		public PerformanceStatisticsService(IPerformanceLogService performanceService,
			IOptions<AppSettings> appsettings)
		{
			_performanceService = performanceService;
			_AppSettings = appsettings.Value;
		}

		public async Task<List<PerformanceStats>> GetSummary()
		{
			List<PerformanceStats> performanceStats = new();
			foreach (var externalApi in _AppSettings.ExternalApis)
			{
				var results = await _performanceService.GetApiPerformanceStats(externalApi.Name);
				if (results != null)
				{
					performanceStats.Add(results);
				}
			}
			return performanceStats;
		}
	}
}
