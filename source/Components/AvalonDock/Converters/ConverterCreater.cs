using System;
using System.Collections.Generic;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Represents the converter Creater.
	/// </summary>
	internal class ConverterCreater
	{
		private static readonly Dictionary<Type, object> ConverterMap = new Dictionary<Type, object>();

		/// <summary>
		/// Gets the get.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <returns>An instance of <typeparamref name="T"/>.</returns>
		public static T Get<T>()
			where T : new()
		{
			if (!ConverterMap.ContainsKey(typeof(T)))
			{
				ConverterMap.Add(typeof(T), new T());
			}

			return (T)ConverterMap[typeof(T)];
		}
	}
}