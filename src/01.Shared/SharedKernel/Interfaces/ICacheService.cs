namespace SharedKernel.Interfaces
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration = default);
        Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan expiration = default);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
}
