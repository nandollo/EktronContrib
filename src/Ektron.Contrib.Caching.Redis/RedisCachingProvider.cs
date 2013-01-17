using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Caching;
using Ektron.Cms.Caching.Provider;
using ServiceStack.Redis;
using Ektron.Contrib.Caching.Common;
using Ektron.Contrib.Caching.Common.Configuration;

namespace Ektron.Contrib.Caching.Redis
{
	public class RedisCachingProvider : CacheProvider
	{
		private readonly string _host;
		private readonly int _port;
		private readonly string _password;
		private readonly string _writeHost;
		private readonly List<HostElement> _slaves = new List<HostElement>();

		public RedisCachingProvider()
		{
			var redisConfiguration = ConfigurationManager.GetSection("redis") as RedisConfiguration;

			if (redisConfiguration == null)
				throw new ArgumentException("Invalid Redis Configuration");

			_host = redisConfiguration.Master.Host;
			_port = redisConfiguration.Master.Port;
			_password = redisConfiguration.Master.Password;

			_writeHost = !String.IsNullOrEmpty(_password) ?
				String.Format("{0}@{1}:{2}", _password, _host, _port) :
				String.Format("{0}:{1}", _host, _port);

			foreach (HostElement slave in redisConfiguration.Slaves)
			{
				_slaves.Add(slave);
			}
		}

		public RedisCachingProvider(string host)
			: this(host, 6379)
		{
		}

		public RedisCachingProvider(string host, int port, string password = null)
		{
			_host = host;
			_port = port;
			_password = password;

			_writeHost = !String.IsNullOrEmpty(_password) ?
				String.Format("{0}@{1}:{2}", _password, _host, _port) :
				String.Format("{0}:{1}", _host, _port);
		}

		private void Command(Action<IRedisClient, string, CacheItem, TimeSpan> redisCommand, string key, CacheItem cacheItem, TimeSpan timeSpanExpiration)
		{
			if (Environment.MachineName.Equals(_host))
			{
				using (IRedisClientsManager redisManager = new BasicRedisClientManager(_writeHost))
				{					
					using (IRedisClient client = redisManager.GetClient())
					{
						redisCommand.Invoke(client, key, cacheItem, timeSpanExpiration);
					}
				}
			}
			else
			{
				using (PooledRedisClientManager redisManager = new PooledRedisClientManager(_writeHost))
				{
					using (IRedisClient client = redisManager.GetClient())
					{
						redisCommand.Invoke(client, key, cacheItem, timeSpanExpiration);
					}
				}
			}
		}

		private object Retrieve(Func<IRedisClient, string, object> redisCommand, string key)
		{
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

			var readOnlyHosts = !String.IsNullOrEmpty(password) ?
				String.Format("{0}@{1}:{2}", password, host, port) :
				String.Format("{0}:{1}", host, port);

			if (Environment.MachineName.Equals(host))
			{
				using (IRedisClientsManager redisManager = new BasicRedisClientManager(readOnlyHosts))
				{
					using (IRedisClient client = redisManager.GetClient())
					{
						return redisCommand.Invoke(client, key);
					}
				}
			}

			using (var redisManager = new PooledRedisClientManager(readOnlyHosts))
			{
				using (IRedisClient client = redisManager.GetClient())
				{
					return redisCommand.Invoke(client, key);
				}
			}
		}


		/// <summary>
		/// Adds an item to the cache.  If the cache already contains the supplied key, an exception will be thrown.
		/// </summary>
		/// <param name="key">key of item to add to cache.</param><param name="entry">item to add to cache</param>
		public override void Add(string key, object entry)
		{
			Add(key, entry, TimeSpan.Zero);
		}

		/// <summary>
		/// Adds an item to the cache.  If the cache already contains the supplied key, an exception will be thrown.
		/// </summary>
		/// <param name="key">key of item to add to cache.</param><param name="entry">item to add to cache</param><param name="timeSpanExpiration">The amount of time the item should be cached for.</param>
		public override void Add(string key, object entry, TimeSpan timeSpanExpiration)
		{
			var cacheItem =
				new CacheItem
				{
					Id = key,
					DataType = entry.GetType(),
					Item = SerializerHelper.Serialize(entry)
				};

			if (timeSpanExpiration != TimeSpan.Zero)
				cacheItem.Expiration = DateTime.UtcNow.Add(timeSpanExpiration);

			Command(TransactionalAdd, key, cacheItem, timeSpanExpiration);
			
			// TODO: (TimeoutException tex)
			// Retry
		}

		private void TransactionalAdd(IRedisClient client, string key, CacheItem cacheItem, TimeSpan timeSpanExpiration)
		{
			if (client.ContainsKey(key)) throw new KeyAlreadyExistsException();

			using (var trans = client.CreateTransaction())
			{
				trans.QueueCommand(r => r.Set(key, cacheItem));

				if (timeSpanExpiration != TimeSpan.Zero)
					trans.QueueCommand(r => r.ExpireEntryIn(key, timeSpanExpiration));

				trans.Commit();
			}
		}

		/// <summary>
		/// Adds an item to the cache. If the cache already contains the supplied key, the item will be replaced.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="entry"></param>
		public override void Put(string key, object entry)
		{
			Put(key, entry, TimeSpan.Zero);
		}

		/// <summary>
		/// Adds an item to the cache.  If the cache already contains the supplied key, the item will be replaced.
		/// </summary>
		/// <param name="key">key of item to add to cache.</param><param name="entry">item to add to cache</param><param name="timeSpanExpiration">The amount of time the item should be cached for.</param>
		public override void Put(string key, object entry, TimeSpan timeSpanExpiration)
		{
			var cacheItem =
				new CacheItem
				{
					Id = key,
					DataType = entry.GetType(),
					Item = SerializerHelper.Serialize(entry)
				};

			if (timeSpanExpiration != TimeSpan.Zero)
				cacheItem.Expiration = DateTime.Now.Add(timeSpanExpiration);

			Command((redis, s, cache, timeSpan) =>
				{
					using (var trans = redis.CreateTransaction())
					{
						trans.QueueCommand(r => r.Set(s, cache));

						if (cacheItem.Expiration.HasValue)
							trans.QueueCommand(r => r.ExpireEntryAt(key, cacheItem.Expiration.Value));

						trans.Commit();
					}
				}, key, cacheItem, timeSpanExpiration);
		}

		/// <summary>
		/// Adds an item to the cache.  If the cache already contains the supplied key, the item will be replaced.
		/// </summary>
		/// <param name="key">key of item to add to cache.</param>
		/// <param name="entry">item to add to cache</param>
		/// <param name="absoluteExpiration"></param>
		public override void Put(string key, object entry, DateTime absoluteExpiration)
		{
			var cacheItem =
				new CacheItem
				{
					Id = key,
					DataType = entry.GetType(),
					Item = SerializerHelper.Serialize(entry)
				};

			if (absoluteExpiration != DateTime.MinValue)
				cacheItem.Expiration = absoluteExpiration;

			Command((redis, s, cache, timeSpan) =>
			{
				using (var trans = redis.CreateTransaction())
				{
					trans.QueueCommand(r => r.Set(s, cache));

					if (cacheItem.Expiration.HasValue)
						trans.QueueCommand(r => r.ExpireEntryAt(key, cacheItem.Expiration.Value));

					trans.Commit();
				}
			}, key, cacheItem, new TimeSpan(absoluteExpiration.Ticks));
		}

		/// <summary>
		/// cacheSegmentKeys not used.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="entry"></param>
		/// <param name="cacheSegmentKeys"></param>
		public override void Put(string key, object entry, string[] cacheSegmentKeys)
		{
			Put(key, entry, TimeSpan.Zero);
		}

		/// <summary>
		/// cacheSegmentKeys not used.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="entry"></param>
		/// <param name="cacheSegmentKeys"></param>
		/// <param name="timeSpanExpiration"></param>
		public override void Put(string key, object entry, string[] cacheSegmentKeys, TimeSpan timeSpanExpiration)
		{
			Put(key, entry, timeSpanExpiration);
		}

		/// <summary>
		/// Gets an item from the cache.  If the key doesn't exist, null will be returned.
		/// </summary>
		/// <param name="key">key of item to retrieve from cache.</param>
		/// <returns>
		/// Object corresponding to supplied key or null if the key isn't in the cache.
		/// </returns>
		public override object Get(string key)
		{
			int numberOfTries = 3;

			for (int i = 0; i < numberOfTries; i++)
			{
				try
				{
					return get(key);
				}
				catch (TimeoutException)
				{

				}	
			}

			return null;
		}

		private object get(string key)
		{
			return Retrieve((client, s) =>
				{
					try
					{
						var cacheItem = client.Get<CacheItem>(s);

						if (cacheItem == null || cacheItem.Item == null) return null;

						var obj = SerializerHelper.Deserialize(cacheItem.Item);

						return obj;
					}
					catch (NullReferenceException)
					{
						return null;
					}
				}, key);
		}

		/// <summary>
		/// Removes an item from the cache.
		/// </summary>
		/// <param name="key">key of the item to remove from cache.</param>
		public override void Remove(string key)
		{
			Command((redis, ky, cache, timeSpan) =>
				{
					using (var trans = redis.CreateTransaction())
					{
						trans.QueueCommand(r => r.Remove(key));

						trans.Commit();
					}	
				}, key, null, TimeSpan.Zero);
		}

		/// <summary>
		/// Not Implemented because it can cause a performance issues
		/// </summary>
		public override void Clear()
		{
		}

		public override int Count
		{
			get { return 0; }
		}

		/// <summary>
		///	CacheDependency Not Implemented
		/// </summary>
		/// <param name="key"></param>
		/// <param name="entry"></param>
		/// <param name="dependency"></param>
		/// <param name="timeSpanExpiration"></param>
		public override void Put(string key, object entry, CacheDependency dependency, TimeSpan timeSpanExpiration)
		{
			Put(key, entry, timeSpanExpiration);

		}

		/// <summary>
		/// CacheDependency Not Implemented
		/// </summary>
		/// <param name="key"></param>
		/// <param name="entry"></param>
		/// <param name="dependency"></param>
		public override void Put(string key, object entry, CacheDependency dependency)
		{
			Put(key, entry);
		}

		/// <summary>
		/// Not Implemented
		/// </summary>
		/// <param name="key"></param>
		/// <param name="entry"></param>
		/// <param name="priority"></param>
		public override void Put(string key, object entry, CacheItemPriority priority)
		{
			Put(key, entry);
		}
	}
}
