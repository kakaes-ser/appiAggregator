using ApiAggregatorAPI.Contracts.Settings;
using ApiAggregatorAPI.Interfaces;
using ApiAggregatorAPI.Services;
using ApiAggregatorAPI.Services.Cache;
using ApiAggregatorAPI.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
	});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IApiAggregationService, ApiAggregationService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IPerformanceLogService, PerformanceLogService>();
builder.Services.AddScoped<IPerformanceStatisticsService, PerformanceStatisticsService>();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
