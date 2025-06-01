using System.Text.Json.Serialization;

namespace ApiAggregatorAPI.Contracts.BusinessModels
{
	public class MainInfo
	{
		[JsonPropertyName("temp")]
		public double Temperature { get; set; }
		[JsonPropertyName("feels_like")]
		public double FeelsLike { get; set; }

		[JsonPropertyName("temp_min")]
		public double TemperatureMin { get; set; }

		[JsonPropertyName("temp_max")]
		public double TemperatureMax { get; set; }
		public int Pressure { get; set; }
		public int Humidity { get; set; }
		public int SeaLevel { get; set; }

		[JsonPropertyName("grnd_level")]
		public int GroundLevel { get; set; }
	}
}
