using ApiAggregatorAPI.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Services.Cache
{
	public class CacheService : ICacheService
	{
		private readonly IMemoryCache _memoryCache;

		public CacheService(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		public async Task SetCacheAsync<T>(string key, T data, TimeSpan expiration)
		{
			// Setting data in memory cache
			_memoryCache.Set(key, data, expiration);
			await Task.CompletedTask;
		}

		public async Task<T> GetCacheAsync<T>(string key)
		{
			// Retrieving data from memory cache
			if (_memoryCache.TryGetValue(key, out T cachedData))
			{
				return await Task.FromResult(cachedData);
			}
			else
			{
				return await Task.FromResult(default(T)); // Return default if not found
			}
		}

		public async Task RemoveCacheAsync(string key)
		{
			_memoryCache.Remove(key);
			await Task.CompletedTask;
		}
	}
}
