using SharedKernel.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace SharedKernel.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value) : default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration = default)
        {
            if (expiration == default)
                expiration = new TimeSpan(1, 0, 30);

            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiration);
        }

        public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan expiration = default)
        {
            if (expiration == default)
                expiration = new TimeSpan(1, 0, 30);

            var json = JsonSerializer.Serialize(value);
            return await _db.StringSetAsync(key, json, expiration, when: When.NotExists);
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }
    }
}
