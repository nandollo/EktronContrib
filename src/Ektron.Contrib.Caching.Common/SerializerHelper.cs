using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Ektron.Contrib.Caching.Common
{
	public static class SerializerHelper
	{
		public static byte[] Serialize(object items)
		{
			using (var output = new MemoryStream())
			{
				var binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(output, items);
				return output.ToArray();
			}
		}
		
		public static object Deserialize(byte[] data)
		{
			using (var stream = new MemoryStream(data))
			{
				var binaryFormatter = new BinaryFormatter();
				return binaryFormatter.Deserialize(stream);
			}
		}
	}
}