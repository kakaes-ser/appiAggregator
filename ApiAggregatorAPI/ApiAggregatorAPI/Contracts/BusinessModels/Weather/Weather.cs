﻿namespace ApiAggregatorAPI.Contracts.BusinessModels
{
	public class Weather
	{
		public int Id { get; set; }
		public string Main { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
	}
}
