using StackExchange.Redis;
using System.Text.Json;

namespace ApiService.Services;

public class RedisCacheService
{
    private readonly IDatabase _database;
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(5);

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value) where T : class
    {
        var serialized = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, serialized, _defaultExpiry);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}
