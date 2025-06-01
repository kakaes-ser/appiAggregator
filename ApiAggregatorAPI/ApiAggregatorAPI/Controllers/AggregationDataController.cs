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
		private readonly IDataRetrieveService _dataRetrieveService;
		private readonly IPerformanceStatisticsService _performanceStatisticsService;

		public AggregationDataController(IDataRetrieveService dataRetrieveService,
			IPerformanceStatisticsService performanceStatisticsService)
		{
			_dataRetrieveService = dataRetrieveService;
			this._performanceStatisticsService = performanceStatisticsService;
		}

		[HttpPost]
		public async Task<IActionResult> GetAggregatedData([FromBody] SearchFilters filters)
		{
			var aggregatedData = await _dataRetrieveService.GetData(filters);
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