using ApiAggregatorAPI.Contracts.BusinessModels;
using ApiAggregatorAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AggregationDataController : ControllerBase
	{
		private readonly IApiAggregationService _aggregationService;
		private readonly IPerformanceStatisticsService _performanceStatisticsService;

		public AggregationDataController(IApiAggregationService aggregationService,
			IPerformanceStatisticsService performanceStatisticsService)
		{
			_aggregationService = aggregationService;
			this._performanceStatisticsService = performanceStatisticsService;
		}

		[HttpPost]
		public async Task<IActionResult> GetAggregatedData([FromBody] SearchFilters filters)
		{
			var aggregatedData = await _aggregationService.AggregateDataAsync(filters);
			return Ok(aggregatedData);
		}

		[HttpGet("performanceSummary")]
		public async Task<ActionResult> GetApiPerformanceStats()
		{
			var stats = await _performanceStatisticsService.GetSummary();
			return Ok(stats);
		}
	}
}