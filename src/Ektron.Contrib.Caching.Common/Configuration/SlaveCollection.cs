using System.Configuration;

namespace Ektron.Contrib.Caching.Common.Configuration
{
	public class SlaveCollection : ConfigurationElementCollection
	{
		public HostElement this[int index]
		{
			get
			{
				return BaseGet(index) as HostElement;
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
			return new HostElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((HostElement)element);
		}
	}
}