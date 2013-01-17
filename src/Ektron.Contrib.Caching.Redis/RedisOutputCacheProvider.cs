using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Caching;
using Ektron.Contrib.Caching.Common;
using Ektron.Contrib.Caching.Common.Configuration;
using ServiceStack.Redis;

namespace Ektron.Contrib.Caching.Redis
{
	public class RedisOutputCacheProvider : OutputCacheProvider
	{
		private readonly string _host;
		private readonly int _port;
		private readonly List<HostElement> _slaves = new List<HostElement>();
		private readonly List<string> _blackList = new List<string>();
		private readonly string _password;

		public RedisOutputCacheProvider()
		{
			var redisConfiguration = ConfigurationManager.GetSection("redis") as RedisConfiguration;

			if (redisConfiguration == null)
				throw new ArgumentException("Invalid Redis Configuration");

			_host = redisConfiguration.Master.Host;
			_port = redisConfiguration.Master.Port;
			_password = redisConfiguration.Master.Password;

			foreach (ExcludeRequestElement exclusion in redisConfiguration.Exclusions)
			{
				_blackList.Add(exclusion.Extension);
			}

			foreach (HostElement slave in redisConfiguration.Slaves)
			{
				_slaves.Add(slave);
			}
		}

		public RedisOutputCacheProvider(string host, int port, string password = null, IEnumerable<string> exclusions = null)
		{
			_host = host;
			_port = port;

			if (!String.IsNullOrEmpty(password))
				_password = password;

			if (exclusions != null)
				_blackList = new List<string>(exclusions);
		}

		/// <summary>
		/// Inserts the specified entry into the output cache.
		/// </summary>
		/// <param name="key">A unique identifier for <paramref name="entry"/>.</param>
		/// <param name="entry">The content to add to the output cache.</param>
		/// <param name="utcExpiry">The time and date on which the cached entry expires.</param>
		/// <returns>
		/// A reference to the specified provider.
		/// </returns>
		public override object Add(string key, object entry, DateTime utcExpiry)
		{
			DateTime universalTime = utcExpiry.ToUniversalTime();

			var cacheItem = new CacheItem
				{
					Id = key,
					DataType = entry.GetType(),
					Item = SerializerHelper.Serialize(entry)
				};
			
			var now = DateTime.UtcNow;

			if (universalTime > DateTime.MinValue && universalTime > now && universalTime < DateTime.MaxValue)
				cacheItem.Expiration = universalTime;

			RedisClient redis = null;

			try
			{
				redis = !String.IsNullOrEmpty(_password) ? new RedisClient(_host, _port, _password) : new RedisClient(_host, _port);

				using (var trans = redis.CreateTransaction())
				{
					CacheItem item = cacheItem;
					trans.QueueCommand(r => r.Set(key, item));

					if (cacheItem.Expiration.HasValue)
						trans.QueueCommand(r => r.ExpireEntryAt(key, universalTime));

					trans.Commit();
				}

				cacheItem = redis.Get<CacheItem>(key);
			}
			finally
			{
				if (redis != null)
					redis.Dispose();
			}

			if (cacheItem == null || cacheItem.Item == null) return null;

			return SerializerHelper.Deserialize(cacheItem.Item);
		}

		/// <summary>
		/// Returns a reference to the specified entry in the output cache.
		/// </summary>
		/// <param name="key">A unique identifier for a cached entry in the output cache.</param>
		/// <returns>
		/// The <paramref name="key"/> entry that identifies the specified entry in the cache, or null if the specified entry is not in the cache.
		/// </returns>
		public override object Get(string key)
		{
			if (RequestBlackList(key)) 
				return null;

			HostElement hostElement =
				_slaves.FirstOrDefault() ??
				new HostElement
				{
					Host = _host,
					Port = _port,
					Password = _password
				};

			string password = hostElement.Password;
			string host = hostElement.Host;
			int port = hostElement.Port;

			RedisClient redis = null;

			try
			{
				redis = !String.IsNullOrEmpty(password) ? new RedisClient(host, port, password) : new RedisClient(host, port);

				if (!redis.ContainsKey(key)) return null;

				var cacheItem = redis.Get<CacheItem>(key);

				if (cacheItem == null || cacheItem.Item == null) return null;

				return SerializerHelper.Deserialize(cacheItem.Item);
			}
			finally
			{
				if (redis != null)
					redis.Dispose();
			}
		}

		public bool RequestBlackList(string key)
		{
			var strings = key.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var lastEntry = strings.Last();

			return _blackList.Contains(lastEntry);
		}

		/// <summary>
		/// Removes the specified entry from the output cache.
		/// </summary>
		/// <param name="key">The unique identifier for the entry to remove from the output cache.</param>
		public override void Remove(string key)
		{
			RedisClient redis = null;

			try
			{
				redis = !String.IsNullOrEmpty(_password) ? new RedisClient(_host, _port, _password) : new RedisClient(_host, _port);

				using (var trans = redis.CreateTransaction())
				{
					trans.QueueCommand(r => r.Remove(key));

					trans.Commit();
				}
			}
			finally
			{
				if (redis != null)
					redis.Dispose();
			}
		}

		/// <summary>
		/// Inserts the specified entry into the output cache, overwriting the entry if it is already cached.
		/// </summary>
		/// <param name="key">A unique identifier for <paramref name="entry"/>.</param>
		/// <param name="entry">The content to add to the output cache.</param>
		/// <param name="utcExpiry">The time and date on which the cached <paramref name="entry"/> expires.</param>
		public override void Set(string key, object entry, DateTime utcExpiry)
		{
			DateTime universalTime = utcExpiry.ToUniversalTime();

			var cacheItem = new CacheItem
				{
					Id = key,
					DataType = entry.GetType(),
					Item = SerializerHelper.Serialize(entry)
				};
			
			var now = DateTime.UtcNow;

			if (universalTime > DateTime.MinValue && universalTime > now && universalTime < DateTime.MaxValue)
				cacheItem.Expiration = universalTime;

			RedisClient redis = null;

			try
			{
				redis = !String.IsNullOrEmpty(_password) ? new RedisClient(_host, _port, _password) : new RedisClient(_host, _port);

				using (var trans = redis.CreateTransaction())
				{
					CacheItem item = cacheItem;
					trans.QueueCommand(r => r.Set(key, item));

					if (cacheItem.Expiration.HasValue)
						trans.QueueCommand(r => r.ExpireEntryAt(key, universalTime));

					trans.Commit();
				}
			}
			finally
			{
				if (redis != null)
					redis.Dispose();
			}
		}
	}
}