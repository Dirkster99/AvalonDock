namespace MLibTest.ViewModels
{
	using MLib.Interfaces;
	using Settings.Interfaces;
	using Settings.UserProfile;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Windows.Input;

	/// <summary>
	/// Implements application life cycle relevant properties and methods,
	/// such as: state for shutdown, shutdown_cancel, command for shutdown,
	/// and methods for save and load application configuration.
	/// </summary>
	internal class AppLifeCycleViewModel : Base.ViewModelBase
	{
		#region fields
		private bool? mDialogCloseResult = null;
		private bool mShutDownInProgress = false;
		private bool mShutDownInProgress_Cancel = false;

		private ICommand mExitApp = null;
		#endregion fields

		#region properties
		/// <summary>
		/// Gets a string for display of the application title.
		/// </summary>
		public string Application_Title
		{
			get
			{
				return Models.AppCore.Application_Title;
			}
		}

		/// <summary>
		/// Get path and file name to application specific settings file
		/// </summary>
		public string DirFileAppSettingsData
		{
			get
			{
				return System.IO.Path.Combine(Models.AppCore.DirAppData,
											  string.Format(CultureInfo.InvariantCulture, "{0}.App.settings",
											  Models.AppCore.AssemblyTitle));
			}
		}

		/// <summary>
		/// This can be used to close the attached view via ViewModel
		/// 
		/// Source: http://stackoverflow.com/questions/501886/wpf-mvvm-newbie-how-should-the-viewmodel-close-the-form
		/// </summary>
		public bool? DialogCloseResult
		{
			get
			{
				return mDialogCloseResult;
			}

			private set
			{
				if (mDialogCloseResult != value)
				{
					mDialogCloseResult = value;
					RaisePropertyChanged(() => DialogCloseResult);
				}
			}
		}

		/// <summary>
		/// Gets a command to exit (end) the application.
		/// </summary>
		public ICommand ExitApp
		{
			get
			{
				if (mExitApp == null)
				{
					mExitApp = new Base.RelayCommand<object>((p) => AppExit_CommandExecuted(),
															 (p) => Closing_CanExecute());
				}

				return mExitApp;
			}
		}

		/// <summary>
		/// Get a path to the directory where the user store his documents
		/// </summary>
		public static string MyDocumentsUserDir
		{
			get
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
		}

		public bool ShutDownInProgress_Cancel
		{
			get
			{
				return mShutDownInProgress_Cancel;
			}

			set
			{
				if (mShutDownInProgress_Cancel != value)
					mShutDownInProgress_Cancel = value;
			}
		}
		#endregion properties

		#region methods
		private void CreateDefaultsSettings(ISettingsManager settings
										  , IAppearanceManager appearance)
		{
			settings.Themes.RemoveAllThemeInfos();
			settings.Themes.AddThemeInfo("Generic", new List<Uri> { });
			settings.Themes.AddThemeInfo("VS 2013 Blue", new List<Uri> { });
			settings.Themes.AddThemeInfo("VS 2013 Dark", new List<Uri> { });
			settings.Themes.AddThemeInfo("VS 2013 Light", new List<Uri> { });

			settings.Themes.AddThemeInfo("Metro", new List<Uri> { });

			settings.Themes.AddThemeInfo("Expression Dark", new List<Uri> { });
			settings.Themes.AddThemeInfo("Expression Light", new List<Uri> { });

			try
			{
				// Adding Generic theme (which is really based on Light theme in MLib)
				// but other components may have another theme definition for Generic
				// so this is how it can be tested ...
				appearance.AddThemeResources("Generic", new List<Uri>
				{
					 new Uri("/MWindowLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/AvalonDock;component/Themes/generic.xaml", UriKind.RelativeOrAbsolute)

					,new Uri("/MLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/MLibTest;component/BindToMLib/MWindowLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)

				}, settings.Themes);
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp);
			}

			try
			{
				// Adding Generic theme (which is really based on Light theme in MLib)
				// but other components may have another theme definition for Generic
				// so this is how it can be tested ...
				appearance.AddThemeResources("VS 2013 Blue", new List<Uri>
				{
				   new Uri("/AvalonDock.Themes.VS2013;component/Themes/generic.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/AvalonDock.Themes.VS2013;component/BlueBrushs.xaml", UriKind.RelativeOrAbsolute)

				  ,new Uri("/MLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/MWindowLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute)

				  ,new Uri("/MLibTest;component/BindToMLib/MWindowLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
////				  ,new Uri("/MLibTest;component/BindToMLib/AvalonDock_Dark_LightBrushs.xaml", UriKind.RelativeOrAbsolute)

				}, settings.Themes);
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp);
			}

			try
			{
				// Add additional Dark and Light resources to those theme resources added above
				appearance.AddThemeResources("VS 2013 Dark", new List<Uri>
				{
				   new Uri("/AvalonDock.Themes.VS2013;component/Themes/generic.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/AvalonDock.Themes.VS2013;component/DarkBrushs.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/ColorPickerLib;component/Themes/DarkBrushs.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/NumericUpDownLib;component/Themes/DarkBrushs.xaml", UriKind.RelativeOrAbsolute)

				  ,new Uri("/MLib;component/Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/MWindowLib;component/Themes/DarkBrushs.xaml", UriKind.RelativeOrAbsolute)

				  ,new Uri("/MLibTest;component/BindToMLib/MWindowLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/MLibTest;component/BindToMLib/AvalonDock_Dark_LightBrushs.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/MLibTest;component/BindToMLib/ColorPickerLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/MLibTest;component/BindToMLib/NumericUpDownLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)

				}, settings.Themes);
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp);
			}

			try
			{
				appearance.AddThemeResources("VS 2013 Light", new List<Uri>
				{
				   new Uri("/AvalonDock.Themes.VS2013;component/Themes/generic.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/AvalonDock.Themes.VS2013;component/LightBrushs.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/ColorPickerLib;component/Themes/LightBrushs.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/NumericUpDownLib;component/Themes/LightBrushs.xaml", UriKind.RelativeOrAbsolute)

				  ,new Uri("/MLib;component/Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/MWindowLib;component/Themes/LightBrushs.xaml", UriKind.RelativeOrAbsolute)

				  ,new Uri("/MLibTest;component/BindToMLib/MWindowLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/MLibTest;component/BindToMLib/AvalonDock_Dark_LightBrushs.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/MLibTest;component/BindToMLib/ColorPickerLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
				  ,new Uri("/MLibTest;component/BindToMLib/NumericUpDownLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)

				}, settings.Themes);
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp);
			}

			try
			{
				// Adding Generic theme (which is really based on Light theme in MLib)
				// but other components may have another theme definition for Generic
				// so this is how it can be tested ...
				appearance.AddThemeResources("Metro", new List<Uri>
				{
					 new Uri("/MLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/MWindowLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute)

					,new Uri("/AvalonDock.Themes.Metro;component/Theme.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/MLibTest;component/BindToMLib/MWindowLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)

				}, settings.Themes);
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp);
			}

			try
			{
				// Adding Generic theme (which is really based on Light theme in MLib)
				// but other components may have another theme definition for Generic
				// so this is how it can be tested ...
				appearance.AddThemeResources("Expression Light", new List<Uri>
				{
					 new Uri("/MLib;component/Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/MWindowLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute)

					,new Uri("/AvalonDock.Themes.Expression;component/LightTheme.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/MLibTest;component/BindToMLib/MWindowLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)

				}, settings.Themes);
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp);
			}

			try
			{
				// Adding Generic theme (which is really based on Light theme in MLib)
				// but other components may have another theme definition for Generic
				// so this is how it can be tested ...
				appearance.AddThemeResources("Expression Dark", new List<Uri>
				{
					 new Uri("/MLib;component/Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/MWindowLib;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/ColorPickerLib;component/Themes/DarkBrushs.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/NumericUpDownLib;component/Themes/DarkBrushs.xaml", UriKind.RelativeOrAbsolute)

					,new Uri("/AvalonDock.Themes.Expression;component/DarkTheme.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/MLibTest;component/BindToMLib/MWindowLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/MLibTest;component/BindToMLib/ColorPickerLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)
					,new Uri("/MLibTest;component/BindToMLib/NumericUpDownLib_DarkLightBrushs.xaml", UriKind.RelativeOrAbsolute)

				}, settings.Themes);
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp);
			}

			appearance.SetDefaultTheme(settings.Themes, "VS 2013 Dark");

			try
			{
				// Create a general settings model to make sure the app is at least governed by defaults
				// if there are no customized settings on first ever start-up of application
				var options = settings.Options;

				Models.SettingDefaults.CreateGeneralSettings(options);
				Models.SettingDefaults.CreateAppearanceSettings(options, settings);

				settings.Options.SetUndirty();
			}
			catch
			{
			}
		}

		#region Save Load Application configuration
		/// <summary>
		/// Save application settings when the application is being closed down
		/// </summary>
		public void SaveConfigOnAppClosed(IViewSize win)
		{
			/***
            try
            {
                Models.AppCore.CreateAppDataFolder();

                // Save App view model fields
                var settings = base.GetService<ISettingsManager>();

                //// settings.SessionData.LastActiveSourceFile = this.mStringDiff.SourceFilePath;
                //// settings.SessionData.LastActiveTargetFile = this.mStringDiff.TargetFilePath;

                // Save program options only if there are un-saved changes that need persistence
                // This can be caused when WPF theme was changed or something else
                // but should normally not occur as often as saving session data
                if (settings.Options.IsDirty == true)
                {
                    ////settings.SaveOptions(AppCore.DirFileAppSettingsData, settings.SettingData);
                    settings.Options.WriteXML(DirFileAppSettingsData);
                }

                settings.SaveSessionData(Models.AppCore.DirFileAppSessionData, settings.SessionData);
            }
            catch (Exception exp)
            {
                var msg = GetService<IMessageBoxService>();
                msg.Show(exp, "Unexpected Error" // Local.Strings.STR_UnexpectedError_Caption
                            , MsgBoxButtons.OK, MsgBoxImage.Error);
            }
            ***/
		}

		/// <summary>
		/// Load configuration from persistence on startup of application
		/// </summary>
		public void LoadConfigOnAppStartup(ISettingsManager settings
										  , IAppearanceManager appearance)
		{
			try
			{
				CreateDefaultsSettings(settings, appearance);

				/***
                    // Re/Load program options and user profile session data to control global behaviour of program
                    ////settings.LoadOptions(AppCore.DirFileAppSettingsData);
                    settings.Options.ReadXML(DirFileAppSettingsData);
                    settings.LoadSessionData(Models.AppCore.DirFileAppSessionData);

                    settings.CheckSettingsOnLoad(SystemParameters.VirtualScreenLeft,
                                                    SystemParameters.VirtualScreenTop);
                ***/
			}
			catch
			{
			}
		}
		#endregion Save Load Application configuration

		#region StartUp/ShutDown
		private void AppExit_CommandExecuted()
		{
			try
			{
				if (Closing_CanExecute() == true)
				{
					mShutDownInProgress_Cancel = false;
					OnRequestClose();
				}
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp);

				////                var msg = GetService<IMessageBoxService>();
				////                msg.Show(exp, "Unknown Error",
				////                MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton);
			}
		}

		private bool Closing_CanExecute()
		{
			return true;
		}

		/// <summary>
		/// Check if pre-requisites for closing application are available.
		/// Save session data on closing and cancel closing process if necessary.
		/// </summary>
		/// <returns>true if application is OK to proceed closing with closed, otherwise false.</returns>
		public bool Exit_CheckConditions(object sender)
		{
			//// var msg = ServiceLocator.ServiceContainer.Instance.GetService<IMessageBoxService>();
			try
			{
				if (mShutDownInProgress == true)
					return true;

				// this return is normally computed if there are documents open with unsaved data
				return true;

				////// Do layout serialization after saving/closing files
				////// since changes implemented by shut-down process are otherwise lost
				////try
				////{
				////    App.CreateAppDataFolder();
				////    this.SerializeLayout(sender);            // Store the current layout for later retrieval
				////}
				////catch
				////{
				////}
			}
			catch (Exception exp)
			{
				Debug.WriteLine(exp);

				////                var msg = GetService<IMessageBoxService>();
				////
				////                msg.Show(exp, "Unexpected Error"//Local.Strings.STR_UnexpectedError_Caption,
				////                            , MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton);
				////                //App.IssueTrackerLink, App.IssueTrackerLink, Util.Local.Strings.STR_MSG_IssueTrackerText, null, true);
			}

			return true;
		}

		#region RequestClose [event]
		/// <summary>
		/// Raised when this workspace should be removed from the UI.
		/// </summary>
		////public event EventHandler ApplicationClosed;

		/// <summary>
		/// Method to be executed when user (or program) tries to close the application
		/// </summary>
		public void OnRequestClose(bool ShutDownAfterClosing = true)
		{
			try
			{
				if (ShutDownAfterClosing == true)
				{
					if (mShutDownInProgress == false)
					{
						if (DialogCloseResult == null)
							DialogCloseResult = true;      // Execute Closing event via attached property

						if (mShutDownInProgress_Cancel == true)
						{
							mShutDownInProgress = false;
							mShutDownInProgress_Cancel = false;
							DialogCloseResult = null;
						}
					}
				}
				else
					mShutDownInProgress = true;

				CommandManager.InvalidateRequerySuggested();

				////EventHandler handler = ApplicationClosed;
				////if (handler != null)
				////  handler(this, EventArgs.Empty);
			}
			catch (Exception exp)
			{
				mShutDownInProgress = false;

				Debug.WriteLine(exp);

				////                var msg = GetService<IMessageBoxService>();
				////                msg.Show(exp, "Unexpected Error" //Local.Strings.STR_UnexpectedError_Caption
				////                            , MsgBoxButtons.OK, MsgBoxImage.Error, MsgBoxResult.NoDefaultButton);
			}
		}

		public void CancelShutDown()
		{
			DialogCloseResult = null;
		}
		#endregion RequestClose [event]
		#endregion StartUp/ShutDown
		#endregion methods
	}
}
