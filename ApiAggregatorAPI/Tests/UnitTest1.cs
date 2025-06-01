using ApiAggregatorAPI.Contracts;
using ApiAggregatorAPI.Contracts.BusinessModels;
using ApiAggregatorAPI.Contracts.Settings;
using ApiAggregatorAPI.Interfaces;
using ApiAggregatorAPI.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests
{
	public class UnitTest1
	{
		private readonly Mock<ICacheService> _cacheMock = new();
		private readonly Mock<IPerformanceLogService> _performanceMock = new();
		private readonly Mock<IRequestService> _requestServiceMock = new();
		private readonly Mock<IDataRetrieveService> _dataRetrieverServiceMock = new();
		private readonly Mock<IApiAggregationService> _apiAggregationServiceMock = new();
		private readonly RequestService _requestService;
		private readonly ApiAggregationService _apiAggregationService;
		private readonly DataRetrieveService _dataRetrieveService;
		private AppSettings? _appSettings;

		public UnitTest1()
		{
			InitializeSettings();
			var optionsMock = new Mock<IOptions<AppSettings>>();
			optionsMock.Setup(o => o.Value).Returns(_appSettings);
			_requestService = new RequestService(_performanceMock.Object);
			_apiAggregationService = new ApiAggregationService(_requestServiceMock.Object, optionsMock.Object);
			_dataRetrieveService = new DataRetrieveService(_apiAggregationServiceMock.Object, _cacheMock.Object, optionsMock.Object);
		}


		[Fact]
		public async Task ShouldRetryOnTransientError()
		{
			var externalApi = _appSettings?.ExternalApis.First();
			var query = new Dictionary<string, string>();

			var callCount = 0;

			Func<Task<string>> apiCall = () =>
			{
				callCount++;
				if (callCount < 2)
					throw new ApiClientException(ErrorType.Transient, "Timeout");
				return Task.FromResult("{ \"data\": \"success\" }");
			};

			var result = await _requestService.ExecuteWithRetry(apiCall, "library", _appSettings.ApiClientSettings.MaxRetryCount, _appSettings.ApiClientSettings.DelayInSeconds);

			Assert.NotNull(result.Data);
			Assert.Null(result.Errors);
			Assert.Equal(2, callCount);
		}

		[Fact]
		public async Task ReturnsCachedDataIfAvailable()
		{

			var expected = new ApiAggregationResult { LibraryResults = new LibraryResults { Docs = new List<BookDoc>() } };
			_cacheMock.Setup(c => c.GetCacheAsync<ApiAggregationResult>("results")).ReturnsAsync(expected);

			var result = await _dataRetrieveService.GetData(new());

			Assert.Equal(expected, result);
			_cacheMock.Verify(c => c.SetCacheAsync(It.IsAny<string>(), It.IsAny<ApiAggregationResult>(), It.IsAny<TimeSpan>()), Times.Never);
		}

		private void InitializeSettings()
		{
			_appSettings = new AppSettings();
			_appSettings.ExternalApis = new()
			{ new ExternalApi
			{
				Name = "library", ApiEndPoint = "https://openlibrary.org:8081", Action = "search.json", ApiFilters = new List<ApiFilter>(){ new ApiFilter { Key = "q", Value = "philosophy"} }
			}

			};
			_appSettings.ApiClientSettings = new()
			{
				DelayInSeconds = 2,
				MaxRetryCount = 3,
				TimeOutInSeconds = 20
			};
			_appSettings.CacheSettings = new()
			{
				ResultsKey = "results",
				ResultsExpirationMinutes = 5,
				StatisticsExpirationMinutes = 5,
			};
		}
	}
}