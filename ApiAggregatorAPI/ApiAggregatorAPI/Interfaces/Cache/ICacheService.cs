using System;
using System.Threading.Tasks;

namespace ApiAggregatorAPI.Interfaces
{
    public interface ICacheService
    {
        Task SetCacheAsync<T>(string key, T data, TimeSpan expiration);
        Task<T> GetCacheAsync<T>(string key);
        Task RemoveCacheAsync(string key);
    }
}
