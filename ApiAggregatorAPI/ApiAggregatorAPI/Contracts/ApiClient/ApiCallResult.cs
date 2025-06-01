using System.Collections.Generic;

namespace ApiAggregatorAPI.Contracts.ApiClient
{
	public class ApiCallResult
	{
		public string Data { get; set; }
		public List<string> Errors { get; set; }
	}
}
