using AluguelDeMotos.Shared.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace AluguelDeMotos.Redis.Service
{
  public class RedisCacheService : IRedisCacheService
  {
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisCacheService(string connectionString)
    {
      _redis = ConnectionMultiplexer.Connect(connectionString);
      _database = _redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
      var value = await _database.StringGetAsync(key);

      if (value.IsNullOrEmpty)
        return default;

      return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
      var serializedValue = JsonSerializer.Serialize(value);

      if (expiration.HasValue)
        await _database.StringSetAsync(key, serializedValue, new Expiration(expiration.Value));
      else
        await _database.StringSetAsync(key, serializedValue);
    }

    public async Task RemoveAsync(string key)
    {
      await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
      return await _database.KeyExistsAsync(key);
    }
  }
}


