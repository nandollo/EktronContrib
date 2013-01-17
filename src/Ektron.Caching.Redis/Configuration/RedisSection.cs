﻿using System;
using System.Configuration;

namespace Storm.Caching.Redis.Configuration
{
	public class RedisSection : ConfigurationSection
	{
		[ConfigurationProperty("host", DefaultValue = "localhost", IsRequired = true)]
		public string Host
		{
			get { return (String)this["host"]; }
			set { this["host"] = value; }
		}

		[ConfigurationProperty("port", DefaultValue = 6379, IsRequired = false)]
		public int Port
		{
			get
			{
				if (this["port"] != null)
				{
					int port;
					if (int.TryParse(this["port"].ToString(), out port))
						return port;
				}

				return 0;
			}
			set
			{
				this["port"] = value;
			}
		}

		[ConfigurationProperty("password", IsRequired = false)]
		public string Password
		{
			get { return (String)this["password"]; }
			set { this["password"] = value; }
		}

		[ConfigurationProperty("exclusions", IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(ExclusionsCollection), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
		public ExclusionsCollection Exclusions
		{
			get
			{
				return (ExclusionsCollection)this["exclusions"] ??
				   new ExclusionsCollection();
			}
		}
	}
}