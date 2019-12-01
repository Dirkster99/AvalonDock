namespace MLibTest
{
	using MLib;
	using MLib.Interfaces;
	using ServiceLocator;
	using Settings;
	using Settings.Interfaces;

	/// <summary>
	/// Creates and initializes all services.
	/// </summary>
	public static class ServiceInjector
	{
		/// <summary>
		/// Loads service objects into the ServiceContainer on startup of application.
		/// </summary>
		/// <returns>Returns the current <seealso cref="ServiceContainer"/> instance
		/// to let caller work with service container items right after creation.</returns>
		public static ServiceContainer InjectServices()
		{
			var appearance = AppearanceManager.GetInstance();
			ServiceContainer.Instance.AddService<ISettingsManager>(SettingsManager.GetInstance(appearance.CreateThemeInfos()));
			ServiceContainer.Instance.AddService<IAppearanceManager>(appearance);

			return ServiceContainer.Instance;
		}
	}
}
