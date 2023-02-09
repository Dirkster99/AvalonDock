using System;
using System.Collections.Concurrent;
using System.Xml.Serialization;

namespace AvalonDock
{
	internal static class XmlSerializersCache
	{
		private static readonly object s_lock = new object();
		private static readonly ConcurrentDictionary<Type, XmlSerializer> s_cache = new ConcurrentDictionary<Type, XmlSerializer>();


		public static XmlSerializer GetSerializer(Type targetType)
		{
			if (s_cache.TryGetValue(targetType, out var serializer))
				return serializer;

			lock (s_lock)
			{
				// we can have multiple threads waiting on lock
				// one of them could create what we were looking for
				if (!s_cache.TryGetValue(targetType, out serializer))
				{
					serializer = XmlSerializer.FromTypes(new[] { targetType })[0];
					_ = s_cache.TryAdd(targetType, serializer);
				}
			}

			return serializer;
		}

		public static XmlSerializer GetSerializer<T>() => GetSerializer(typeof(T));
	}
}
