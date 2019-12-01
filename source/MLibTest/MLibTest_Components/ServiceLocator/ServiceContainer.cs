namespace ServiceLocator
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Source: http://www.codeproject.com/Articles/70223/Using-a-Service-Locator-to-Work-with-MessageBoxes
	/// </summary>
	public class ServiceContainer
	{
		#region fields
		public static readonly ServiceContainer Instance = new ServiceContainer();

		readonly Dictionary<Type, object> _serviceMap;
		readonly object _serviceMapLock;
		#endregion fields

		#region constructors
		/// <summary>
		/// Class Constructor
		/// </summary>
		private ServiceContainer()
		{
			_serviceMap = new Dictionary<Type, object>();
			_serviceMapLock = new object();
		}
		#endregion constructors

		#region methods
		public void AddService<TServiceContract>(TServiceContract implementation)
			where TServiceContract : class
		{
			lock (_serviceMapLock)
			{
				_serviceMap[typeof(TServiceContract)] = implementation;
			}
		}

		public TServiceContract GetService<TServiceContract>()
			where TServiceContract : class
		{
			object service;
			lock (_serviceMapLock)
			{
				_serviceMap.TryGetValue(typeof(TServiceContract), out service);
			}

			return service as TServiceContract;
		}
		#endregion methods
	}
}
