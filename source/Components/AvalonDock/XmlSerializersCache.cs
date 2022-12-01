using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AvalonDock
{
	internal static class XmlSerializersCache
	{
		private static readonly Dictionary<Type, XmlSerializer> s_cache = new Dictionary<Type, XmlSerializer>();


		public static XmlSerializer GetSerializer(Type targetType)
		{
			if (!s_cache.TryGetValue(targetType, out var serializer))
			{
				serializer = XmlSerializer.FromTypes(new[] { targetType })[0];
				s_cache.Add(targetType, serializer);
			}

			return serializer;
		}

		public static XmlSerializer GetSerializer<T>() => GetSerializer(typeof(T));
	}
}
