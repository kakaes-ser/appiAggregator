using System.Text.Json.Serialization;

namespace ApiAggregatorAPI.Contracts.BusinessModels
{
	public class Coordinates
	{
		[JsonPropertyName("lon")]
		public decimal Longitude { get; set; }

		[JsonPropertyName("lat")]
		public decimal Latitude { get; set; }
	}
}
