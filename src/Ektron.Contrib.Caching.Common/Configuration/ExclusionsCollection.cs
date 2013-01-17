using System.Configuration;

namespace Ektron.Contrib.Caching.Common.Configuration
{
	public class ExclusionsCollection : ConfigurationElementCollection
	{
		public ExcludeRequestElement this[int index]
		{
			get
			{
				return BaseGet(index) as ExcludeRequestElement;
			}
			set
			{
				if (BaseGet(index) != null)
					BaseRemoveAt(index);

				BaseAdd(index, value);
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ExcludeRequestElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ExcludeRequestElement)element).Extension;
		}
	}
}