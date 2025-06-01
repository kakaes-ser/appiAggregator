using ApiAggregatorAPI.Contracts.BusinessModels;
using ApiAggregatorAPI.Contracts.Settings;
using ApiAggregatorAPI.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Services
{
	public class DataRetrieveService : IDataRetrieveService
	{
		private readonly IApiAggregationService _apiAggregationService;
		private readonly ICacheService _cacheService;
		private readonly AppSettings _AppSettings;
		public DataRetrieveService(IApiAggregationService apiAggregationService, ICacheService cacheService,
			IOptions<AppSettings> appsettings)
		{
			_apiAggregationService = apiAggregationService;
			_cacheService = cacheService;
			_AppSettings = appsettings.Value;
		}

		public async Task<ApiAggregationResult> GetData(SearchFilters searchFilters)
		{
			var cachedResult = await _cacheService.GetCacheAsync<ApiAggregationResult>(_AppSettings.CacheSettings.ResultsKey);

			if (cachedResult != null && !searchFilters.Refresh)
			{
				return FilteredData(cachedResult, searchFilters);
			}
			var results = await _apiAggregationService.AggregateDataAsync();

			await _cacheService.SetCacheAsync(_AppSettings.CacheSettings.ResultsKey, results, TimeSpan.FromMinutes(_AppSettings.CacheSettings.ResultsExpirationMinutes));

			return FilteredData(results, searchFilters);
		}

		private ApiAggregationResult FilteredData(ApiAggregationResult results, SearchFilters searchFilters)
		{
			if (searchFilters.NewsDateTo is not null)
			{
				results.NewsResults.Articles = results.NewsResults.Articles.Where(w => w.PublishedAt < searchFilters.NewsDateTo.Value).ToList();
			}

			if (searchFilters.NewsDateFrom is not null)
			{
				results.NewsResults.Articles = results.NewsResults.Articles.Where(w => w.PublishedAt > searchFilters.NewsDateTo.Value).ToList();
			}

			if (!string.IsNullOrWhiteSpace(searchFilters.Keyword))
			{
				results.NewsResults.Articles = results.NewsResults.Articles.Where(w => w.Title.Contains(searchFilters.Keyword)).ToList();
				results.LibraryResults = FilterLibraryResultsByKeyword(results.LibraryResults, searchFilters.Keyword);
			}

			return results ?? new();
		}

		public static LibraryResults FilterLibraryResultsByKeyword(LibraryResults libraryResults, string keyword)
		{
			if (libraryResults == null || libraryResults.Docs == null || string.IsNullOrWhiteSpace(keyword))
				return libraryResults;

			keyword = keyword.ToLowerInvariant();

			var filteredDocs = libraryResults.Docs
				.Where(doc =>
					!string.IsNullOrWhiteSpace(doc.Title) && doc.Title.ToLowerInvariant().Contains(keyword) ||
					doc.AuthorName != null && doc.AuthorName.Any(author => author?.ToLowerInvariant().Contains(keyword) == true))
				.ToList();

			return new LibraryResults
			{
				NumFound = filteredDocs.Count,
				Start = 0,
				NumFoundExact = true,
				NumFoundUnderscore = filteredDocs.Count,
				DocumentationUrl = libraryResults.DocumentationUrl,
				Query = keyword,
				Offset = null,
				Docs = filteredDocs
			};
		}
	}
}
