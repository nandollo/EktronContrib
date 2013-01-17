using System.Configuration;

namespace Storm.Caching.Redis.Configuration
{
	public class ExcludeRequestElement : ConfigurationElement
	{
		[ConfigurationProperty("extension", IsRequired = true)]
		public string Extension
		{
			get{ return this["extension"] as string; }
		}		
	}
}