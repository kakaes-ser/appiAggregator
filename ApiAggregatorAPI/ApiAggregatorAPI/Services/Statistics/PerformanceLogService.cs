using ApiAggregatorAPI.Contracts.BusinessModels;
using ApiAggregatorAPI.Contracts.Settings;
using ApiAggregatorAPI.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Services
{
	public class PerformanceLogService : IPerformanceLogService
	{
		private readonly ICacheService _cacheService;
		private readonly AppSettings _AppSettings;
		public PerformanceLogService(ICacheService cacheService,
			IOptions<AppSettings> appSettings)
		{
			this._cacheService = cacheService;
			_AppSettings = appSettings.Value;
		}

		public async Task UpdatePerformanceStats(string apiName, double elapsedTime)
		{
			var cacheKey = $"ApiStats_{apiName}";
			PerformanceStats stats = await _cacheService.GetCacheAsync<PerformanceStats>(cacheKey);
			if (stats == null)
			{
				stats = new();
			}
			stats.ApiName = apiName;
			stats.TotalRequests++;
			stats.TotalResponseTime += elapsedTime;

			if (elapsedTime < _AppSettings.RequestStatsSettings.FastRequestsMs)
			{
				stats.FastRequests++;
			}

			else if (elapsedTime >= _AppSettings.RequestStatsSettings.FastRequestsMs && elapsedTime <= _AppSettings.RequestStatsSettings.SlowRequestsMs)
			{
				stats.AverageRequests++;
			}

			else
			{
				stats.SlowRequests++;
			}

			await _cacheService.SetCacheAsync(cacheKey, stats, TimeSpan.FromMinutes(_AppSettings.CacheSettings.StatisticsExpirationMinutes));
		}

		public async Task<PerformanceStats> GetApiPerformanceStats(string apiName)
		{
			var cacheKey = $"ApiStats_{apiName}";
			return await _cacheService.GetCacheAsync<PerformanceStats>(cacheKey);
		}
	}
}
