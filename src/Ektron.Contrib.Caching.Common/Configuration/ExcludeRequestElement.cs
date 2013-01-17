using System.Configuration;

namespace Ektron.Contrib.Caching.Common.Configuration
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