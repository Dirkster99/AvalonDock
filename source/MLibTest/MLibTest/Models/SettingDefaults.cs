namespace MLibTest.Models
{
	using Settings.Interfaces;
	using SettingsModel.Interfaces;
	using System.Windows.Media;

	/// <summary>
	/// Class contains all methods necessary to initialize the applications settings model.
	/// </summary>
	internal static class SettingDefaults
	{
		/// <summary>
		/// Create the minimal settings model that should be used for every application.
		/// This model does not include advanced features like theming etc...
		/// </summary>
		/// <param name="settings"></param>
		public static void CreateGeneralSettings(IEngine options)
		{
			const string groupName = "Options";

			options.AddOption(groupName, "ReloadOpenFilesFromLastSession", typeof(bool), false, true);
			options.AddOption(groupName, "SourceFilePath", typeof(string), false, @"C:\");
			options.AddOption(groupName, "LanguageSelected", typeof(string), false, "en-US");
		}

		/// <summary>
		/// Create the minimal settings model that should be used for every application.
		/// </summary>
		/// <param name="settings"></param>
		public static void CreateAppearanceSettings(IEngine options, ISettingsManager settings)
		{
			const string groupName = "Appearance";

			options.AddOption(groupName, "ThemeDisplayName", typeof(string), false, "VS 2013 Dark");
			options.AddOption(groupName, "ApplyWindowsDefaultAccent", typeof(bool), false, true);
			options.AddOption(groupName, "AccentColor", typeof(Color), false, Color.FromRgb(0x33, 0x99, 0xff));
		}
	}
}
