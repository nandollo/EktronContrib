using System.Configuration;

namespace Ektron.Contrib.Caching.Common.Configuration
{
	public class RedisConfiguration : ConfigurationSection
	{
		[ConfigurationProperty("master")]
		public HostElement Master
		{
			get { return (HostElement)this["master"]; }
			set { this["master"] = value; }
		}

		[ConfigurationProperty("slaves", IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(SlaveCollection), AddItemName = "slave", ClearItemsName = "clear", RemoveItemName = "remove")]
		public SlaveCollection Slaves
		{
			get
			{
				return (SlaveCollection)this["slaves"] ??
				       new SlaveCollection();
			}
		}

		[ConfigurationProperty("outputCacheExclusions", IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(ExclusionsCollection), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
		public ExclusionsCollection Exclusions
		{
			get
			{
				return (ExclusionsCollection)this["outputCacheExclusions"] ??
				       new ExclusionsCollection();
			}
		}
	}
}