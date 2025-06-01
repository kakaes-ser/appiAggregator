using System.Collections.Generic;

namespace ApiAggregatorAPI.Contracts.BusinessModels
{
	public class ApiAggregationResult
	{
		public WeatherResults WeatherResults { get; set; }
		public NewsResults NewsResults { get; set; }
		public LibraryResults LibraryResults { get; set; }
		public List<string> Errors { get; set; }
	}
}
