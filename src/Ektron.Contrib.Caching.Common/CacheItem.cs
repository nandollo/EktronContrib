using System;

namespace Ektron.Contrib.Caching.Common
{
	public class CacheItem
	{
		public string Id { get; set; }
		public DateTime? Expiration { get; set; }
		public Type DataType { get; set; }
		public byte[] Item { get; set; }
	}
}