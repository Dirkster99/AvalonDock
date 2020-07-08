using System;
using System.Collections.Generic;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Implements a static factory pattern for instances of WPF converters
	/// and ensures that every single converter is instanciated only once.
	/// </summary>
	internal class ConverterCreater
	{
		#region Private fields

		private static readonly Dictionary<Type, object> ConverterMap = new Dictionary<Type, object>();

		#endregion Private fields

		#region Public methods

		/// <summary>
		/// Gets an available instance of the indicated type <see cref="{T}"/> or a brand new instance
		/// if the indicated type was not created, yet (to be returned on next query of instance for type <see cref="{T}"/>).
		///
		/// This method ensures that every instance per class <see cref="{T}"/> is instanciated only once.
		/// </summary>
		/// <typeparam name="T">Type of class to be returned form this factory.</typeparam>
		/// <returns></returns>
		public static T Get<T>() where T : new()
		{
			if (!ConverterMap.ContainsKey(typeof(T)))
			{
				ConverterMap.Add(typeof(T), new T());
			}
			return (T)ConverterMap[typeof(T)];
		}

		#endregion Public methods
	}
}