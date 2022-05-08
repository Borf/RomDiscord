using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

namespace RomDiscord.Services
{
	public class DiskCache : IDistributedCache
	{
        private readonly ConcurrentDictionary<string, byte[]> cache = new ConcurrentDictionary<string, byte[]>();

        public DiskCache()
		{
			try
			{
				using (var fs = new FileStream("session.txt", FileMode.Open))
					cache = JsonSerializer.Deserialize<ConcurrentDictionary<string, byte[]>>(fs);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public byte[]? Get(string key)
        {
            if(cache.ContainsKey(key))
                return (byte[]?)cache[key];
            return null;
        }

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(Get(key));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            cache[key] = value;
            using (var fs = new FileStream("session.txt", FileMode.Create))
                JsonSerializer.Serialize(fs, cache);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key)
        {

        }

        public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            Refresh(key);
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            byte[]? removed;
            cache.TryRemove(key, out removed);
        }

        public Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }

}
