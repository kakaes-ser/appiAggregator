using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiAggregatorAPI.Contracts.BusinessModels
{
	public class WeatherResults
	{
		[JsonPropertyName("coord")]
		public Coordinates Coordinates { get; set; }
		public List<Weather> Weather { get; set; }
		public string Base { get; set; }

		[JsonPropertyName("main")]
		public MainInfo MainInfo { get; set; }
		public int Visibility { get; set; }
		public Wind Wind { get; set; }
		public Clouds Clouds { get; set; }
		public int Dt { get; set; }
		[JsonPropertyName("sys")]
		public SystemInfo SystemInfo { get; set; }
		public int Timezone { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		[JsonPropertyName("cod")]
		public int Code { get; set; }
	}
}
