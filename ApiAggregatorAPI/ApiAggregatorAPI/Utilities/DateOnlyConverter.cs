﻿using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiAggregatorAPI.Utilities
{
	public class DateOnlyConverter : JsonConverter<DateOnly>
	{
		private const string Format = "yyyy-MM-dd";

		public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return DateOnly.FromDateTime(DateTime.Parse(reader.GetString()));
		}

		public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
		}
	}
}
