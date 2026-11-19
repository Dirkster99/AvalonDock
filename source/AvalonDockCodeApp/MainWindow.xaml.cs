using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Core;
using AvalonDock.DependencyInjection;
using AvalonDock.Layout;
using AvalonDock.Serializer.Xml;
using AvalonDock.Themes;
using AvalonDock.Themes.VS;
#if DEVFLOW
using LeXtudio.DevFlow.Agent.Core;
using Microsoft.Maui.DevFlow.Agent.Core;
#endif
using ToggleTestApp.ViewModels;

namespace ToggleTestApp
{
	public partial class MainWindow : Window
	{
		public MainWindow(MainViewModel viewModel, ToggleDockOptions? dockOptions = null)
		{
			DataContext = viewModel;
			InitializeComponent();

			if (dockOptions != null)
			{
				dockManager.ButtonSize = dockOptions.ButtonSize;
				dockManager.DefaultDockWidth = dockOptions.DefaultDockWidth;
				dockManager.DefaultDockHeight = dockOptions.DefaultDockHeight;
				dockManager.ShowHeaderMinimizeButton = dockOptions.ShowHeaderMinimizeButton;
				dockManager.ShowHeaderOptionsButton = dockOptions.ShowHeaderOptionsButton;

				if (Enum.TryParse<DockLayoutPriority>(dockOptions.LayoutPriority, out var priority))
				{
					dockManager.LayoutPriority = priority;
				}
			}

			ContentRendered += (_, _) => UpdateTitleBarColor();
		}

		private void OnLayoutPriorityChanged(object sender, RoutedEventArgs e)
		{
			menuBottomFullWidth.IsChecked = sender == menuBottomFullWidth;
			menuSidesFullHeight.IsChecked = sender == menuSidesFullHeight;
			menuDefaultPriority.IsChecked = sender == menuDefaultPriority;

			if (menuBottomFullWidth.IsChecked)
				dockManager.LayoutPriority = DockLayoutPriority.BottomFullWidth;
			else if (menuSidesFullHeight.IsChecked)
				dockManager.LayoutPriority = DockLayoutPriority.SidesFullHeight;
			else
				dockManager.LayoutPriority = DockLayoutPriority.Default;
		}

		private void OnThemeChanged(object sender, RoutedEventArgs e)
		{
			menuArcDark.IsChecked = sender == menuArcDark;
			menuArcLight.IsChecked = sender == menuArcLight;
			menuVs2015Dark.IsChecked = sender == menuVs2015Dark;
			menuVs2015Light.IsChecked = sender == menuVs2015Light;
			menuVs2015Blue.IsChecked = sender == menuVs2015Blue;

			Theme theme;
			bool isDark;

			if (sender == menuVs2015Dark)
			{
				theme = new VS2015DarkTheme();
				isDark = true;
			}
			else if (sender == menuVs2015Light)
			{
				theme = new VS2015LightTheme();
				isDark = false;
			}
			else if (sender == menuVs2015Blue)
			{
				theme = new VS2015BlueTheme();
				isDark = true;
			}
			else if (sender == menuArcLight)
			{
				theme = new ArcLightTheme();
				isDark = false;
			}
			else
			{
				theme = new ArcDarkTheme();
				isDark = true;
			}

			dockManager.Theme = theme;
			SetAppThemeResources(isDark);
			UpdateThemeColors();
		}

		private void SetAppThemeResources(bool isDark)
		{
			if (isDark)
			{
				Resources["AppPanelBg"] = Brush("#252526");
				Resources["AppEditorBg"] = Brush("#1E1E1E");
				Resources["AppInputBg"] = Brush("#3C3C3C");
				Resources["AppInputBarBg"] = Brush("#2D2D2D");
				Resources["AppText"] = Brush("#CCCCCC");
				Resources["AppSubText"] = Brush("#808080");
				Resources["AppDimText"] = Brush("#555555");
				Resources["AppEditorText"] = Brush("#D4D4D4");
				Resources["AppLineNumbers"] = Brush("#858585");
				Resources["AppScrollbarBg"] = Brush("#2B2B2B");
				Resources["AppSelection"] = Brush("#094771");
			}
			else
			{
				Resources["AppPanelBg"] = Brush("#F5F5F5");
				Resources["AppEditorBg"] = Brush("#FFFFFF");
				Resources["AppInputBg"] = Brush("#FFFFFF");
				Resources["AppInputBarBg"] = Brush("#E8E8E8");
				Resources["AppText"] = Brush("#1E1E1E");
				Resources["AppSubText"] = Brush("#616161");
				Resources["AppDimText"] = Brush("#999999");
				Resources["AppEditorText"] = Brush("#1E1E1E");
				Resources["AppLineNumbers"] = Brush("#858585");
				Resources["AppScrollbarBg"] = Brush("#E0E0E0");
				Resources["AppSelection"] = Brush("#B4D8FD");
			}
		}

		private void UpdateThemeColors()
		{
			Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
			{
				bool isDark = IsDarkBrush(dockManager.Background);
				var foreground = isDark ? Brushes.White : Brushes.Black;
				Foreground = foreground;

				UpdateTitleBarColor();
			}));
		}

		[DllImport("dwmapi.dll", PreserveSig = true)]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

		private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
		private const int DWMWA_CAPTION_COLOR = 35;

		private void UpdateTitleBarColor()
		{
			var hwnd = new WindowInteropHelper(this).Handle;
			if (hwnd == IntPtr.Zero)
				return;

			bool isDark = IsDarkBrush(dockManager.Background);
			int darkMode = isDark ? 1 : 0;
			DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

			if (dockManager.Background is SolidColorBrush scb)
			{
				var c = scb.Color;
				int colorRef = c.R | (c.G << 8) | (c.B << 16);
				DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref colorRef, sizeof(int));
			}
		}

		private static bool IsDarkBrush(Brush brush)
		{
			if (brush is SolidColorBrush scb)
			{
				var c = scb.Color;
				var luminance = (0.299 * c.R) + (0.587 * c.G) + (0.114 * c.B);
				return luminance < 128;
			}

			return true;
		}

		private static SolidColorBrush Brush(string hex)
		{
			var color = (Color)ColorConverter.ConvertFromString(hex);
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}

		private void OnExit(object sender, RoutedEventArgs e) => Close();

		// ===== Layout persistence =====

		/// <summary>Title of the tool window that the "unknown toolbox" layout carries.</summary>
		public const string UnknownToolboxTitle = "Ghost Toolbox";

		/// <summary>Content id of that tool window, which no toolbox of this application answers to.</summary>
		private const string UnknownToolboxContentId = "ghostToolbox";

		private static string LayoutFilePath =>
			Path.Combine(AppContext.BaseDirectory, "ToggleDockLayout.config");

		private static string UnknownToolboxLayoutFilePath =>
			Path.Combine(AppContext.BaseDirectory, "ToggleDockLayout.Unknown.config");

		private void OnSaveLayout(object sender, RoutedEventArgs e)
		{
			new XmlLayoutSerializer(dockManager).Serialize(LayoutFilePath);
		}

		private void OnLoadLayout(object sender, RoutedEventArgs e)
		{
			LoadLayout(LayoutFilePath);
		}

		/// <summary>
		/// Writes a copy of the saved layout with one extra tool window whose content id no toolbox
		/// answers to, then loads it. This is what a layout stored by an earlier version of an
		/// application looks like after one of its toolboxes has been removed.
		/// </summary>
		/// <param name="sender">The menu item.</param>
		/// <param name="e">The event arguments.</param>
		private void OnLoadLayoutWithUnknownToolbox(object sender, RoutedEventArgs e)
		{
			if (!File.Exists(LayoutFilePath))
			{
				OnSaveLayout(sender, e);
			}

			var document = XDocument.Load(LayoutFilePath);
			var root = document.Root;
			if (root == null)
			{
				return;
			}

			var leftSide = root.Element("LeftSide");
			if (leftSide == null)
			{
				leftSide = new XElement("LeftSide");
				root.Add(leftSide);
			}

			var group = leftSide.Element("LayoutAnchorGroup");
			if (group == null)
			{
				group = new XElement("LayoutAnchorGroup");
				leftSide.Add(group);
			}

			group.Add(new XElement(
				"LayoutAnchorable",
				new XAttribute("Title", UnknownToolboxTitle),
				new XAttribute("ContentId", UnknownToolboxContentId)));

			document.Save(UnknownToolboxLayoutFilePath);
			LoadLayout(UnknownToolboxLayoutFilePath);
		}

		/// <summary>
		/// Restores a stored layout, reconnecting each tool window to the toolbox that carries its
		/// content id. A stored item no toolbox answers to is left without content, which is the
		/// signal the serializer uses to keep it out of the restored layout.
		/// </summary>
		// ===== DevFlow surface =====
		//
		// The integration tests drive this app through the DevFlow agent. Everything the tests ask
		// about is read from the live control tree here - the sidebar buttons are enumerated as the
		// ToggleDockButton instances the user would click, not from a model the tests could not
		// otherwise reach - and every mutation goes through the same method the control itself calls
		// (ToggleDockingManager.ToggleAnchorable), so a test failure points at the app's behaviour
		// rather than at a test-only reimplementation of it.

		/// <summary>Queries the sidebar toggle buttons, grouped by zone.</summary>
		public string QueryToggleBars()
		{
			var buttons = FindToggleButtons();
			return JsonSerializer.Serialize(new Dictionary<string, object>
			{
				["windowTitle"] = Title,
				["buttonSize"] = dockManager.ButtonSize,
				["layoutPriority"] = dockManager.LayoutPriority.ToString(),
				["count"] = buttons.Count,
				["zones"] = buttons
					.GroupBy(b => b.Zone.ToString())
					.OrderBy(g => g.Key, StringComparer.Ordinal)
					.ToDictionary(
						g => g.Key,
						g => g.Select(DescribeToggleButton).ToArray()),
				["buttons"] = buttons.Select(DescribeToggleButton).ToArray(),
			});
		}

		/// <summary>Queries the anchorables of the layout and whether they are showing or auto-hidden.</summary>
		public string QueryAnchorables()
		{
			var anchorables = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().ToArray();
			return JsonSerializer.Serialize(new Dictionary<string, object>
			{
				["count"] = anchorables.Length,
				["anchorables"] = anchorables.Select(a => new Dictionary<string, object>
				{
					["title"] = a.Title,
					["contentId"] = a.ContentId,
					["isVisible"] = a.IsVisible,
					["isAutoHidden"] = a.IsAutoHidden,
					["isActive"] = a.IsActive,
					["isFloating"] = a.IsFloating,
					["shortcut"] = a.Content is IToolbox toolbox ? toolbox.Shortcut : null,
				}).ToArray(),
				["documents"] = dockManager.Layout.Descendents().OfType<LayoutDocument>()
					.Select(d => new Dictionary<string, object>
					{
						["title"] = d.Title,
						["contentId"] = d.ContentId,
						["isActive"] = d.IsActive,
						["isVisible"] = d.IsVisible,
					}).ToArray(),
			});
		}

		/// <summary>
		/// Clicks a sidebar toggle button, by content id or by title.
		/// </summary>
		/// <remarks>
		/// This is the button's own action: <see cref="ToggleDockButton.OnClick"/> calls exactly this
		/// method, so it is what the app does when the button is pressed. A real pointer press is
		/// covered separately by the native-input tests, which cannot also be the way every state
		/// assertion here is driven - the sidebar is a plain control tree, and its behaviour is what
		/// these tests are about.
		/// </remarks>
		public string ClickToggleButton(string contentIdOrTitle)
		{
			var button = FindToggleButton(contentIdOrTitle);
			if (button == null)
				throw new InvalidOperationException($"No sidebar toggle button matches '{contentIdOrTitle}'.");

			dockManager.ToggleAnchorable(button.Anchorable, button.Zone);

			return JsonSerializer.Serialize(new Dictionary<string, object>
			{
				["clicked"] = button.Anchorable.ContentId,
				["zone"] = button.Zone.ToString(),
				["buttons"] = FindToggleButtons().Select(DescribeToggleButton).ToArray(),
				["anchorables"] = QueryAnchorables(),
			});
		}

		/// <summary>Queries the keyboard shortcuts the manager registered on the window.</summary>
		public string QueryShortcuts()
		{
			var bindings = InputBindings.OfType<KeyBinding>()
				.Select(b => new Dictionary<string, object>
				{
					["gesture"] = DescribeGesture(b.Gesture),
					["key"] = b.Gesture is KeyGesture keyGesture ? keyGesture.Key.ToString() : null,
					["modifiers"] = b.Gesture is KeyGesture modifiersGesture ? modifiersGesture.Modifiers.ToString() : null,
					["command"] = b.Command?.GetType().Name,
				})
				.ToArray();

			return JsonSerializer.Serialize(new Dictionary<string, object>
			{
				["count"] = bindings.Length,
				["bindings"] = bindings,
			});
		}

		/// <summary>
		/// Raises the shortcut for a toolbox, the way the window's key binding for it would.
		/// </summary>
		/// <remarks>
		/// WPF only matches a KeyGesture inside its input pipeline, which needs real keyboard state:
		/// the agent can type text into a control, but it cannot hold Ctrl while pressing a letter on
		/// every supported platform (XTest on Linux, cliclick on macOS, SendInput on Windows each
		/// need their own chord implementation). So this looks the binding up by gesture - which is
		/// the part that would silently be missing if the manager did not register it - and executes
		/// the command the binding carries. avd.query.shortcuts exposes the registration itself when
		/// a test needs to assert on it directly.
		/// </remarks>
		public string InvokeShortcut(string gesture)
		{
			if (string.IsNullOrWhiteSpace(gesture))
				throw new ArgumentException("A gesture such as 'Ctrl+Alt+E' is required.", nameof(gesture));

			var binding = InputBindings.OfType<KeyBinding>().FirstOrDefault(b => MatchesGesture(b.Gesture, gesture));
			if (binding == null)
			{
				throw new InvalidOperationException(
					$"No key binding for '{gesture}'. Registered: " +
					string.Join(", ", InputBindings.OfType<KeyBinding>().Select(b => DescribeGesture(b.Gesture))));
			}

			if (binding.Command?.CanExecute(binding.CommandParameter) != true)
				throw new InvalidOperationException($"The command for '{gesture}' cannot execute.");

			binding.Command.Execute(binding.CommandParameter);
			return QueryToggleBars();
		}

		/// <summary>
		/// Returns the app to its startup layout: no saved layout file, every sidebar section closed.
		/// </summary>
		/// <remarks>
		/// The app starts with all toolboxes auto-hidden (their sidebar buttons unchecked), so closing
		/// whatever a previous test opened is enough to get back to that state without restarting the
		/// process. Documents are left alone; a test that adds them removes them itself.
		/// </remarks>
		public string ResetLayout()
		{
			foreach (var path in new[] { LayoutFilePath, UnknownToolboxLayoutFilePath })
			{
				if (File.Exists(path))
					File.Delete(path);
			}

			foreach (var button in FindToggleButtons())
			{
				if (!button.Anchorable.IsAutoHidden)
					dockManager.ToggleAnchorable(button.Anchorable, button.Zone);
			}

			return QueryToggleBars();
		}

		/// <summary>Saves the layout to the app's layout file (the _Layout > Save Layout menu).</summary>
		public string SaveLayoutToFile()
		{
			new XmlLayoutSerializer(dockManager).Serialize(LayoutFilePath);
			return JsonSerializer.Serialize(new Dictionary<string, object>
			{
				["path"] = LayoutFilePath,
				["exists"] = File.Exists(LayoutFilePath),
			});
		}

		/// <summary>Loads the saved layout (the _Layout > Load Layout menu).</summary>
		public string LoadLayoutFromFile()
		{
			LoadLayout(LayoutFilePath);
			return QueryToggleBars();
		}

		/// <summary>Loads a layout carrying a tool window no toolbox answers to.</summary>
		public string LoadLayoutWithUnknownToolbox()
		{
			OnLoadLayoutWithUnknownToolbox(this, new RoutedEventArgs());
			return QueryToggleBars();
		}

		/// <summary>Renders a gesture the way a test writes it: "Ctrl+Alt+E".</summary>
		private static string? DescribeGesture(InputGesture? gesture)
		{
			if (gesture is not KeyGesture keyGesture)
				return gesture?.ToString();

			var parts = new List<string>();
			if (keyGesture.Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
			if (keyGesture.Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
			if (keyGesture.Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
			if (keyGesture.Modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
			parts.Add(keyGesture.Key.ToString());
			return string.Join("+", parts);
		}

		/// <summary>Compares a registered gesture with a test-supplied "Ctrl+Alt+E" style string.</summary>
		private static bool MatchesGesture(InputGesture? gesture, string text)
			=> gesture is KeyGesture keyGesture
				&& string.Equals(DescribeGesture(keyGesture), text.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase);

		private List<ToggleDockButton> FindToggleButtons()
		{
			var buttons = new List<ToggleDockButton>();
			CollectToggleButtons(dockManager, buttons);
			return buttons;
		}

		private static void CollectToggleButtons(DependencyObject parent, List<ToggleDockButton> buttons)
		{
			var count = VisualTreeHelper.GetChildrenCount(parent);
			for (var i = 0; i < count; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is ToggleDockButton button && button.Anchorable != null)
					buttons.Add(button);
				else
					CollectToggleButtons(child, buttons);
			}
		}

		private ToggleDockButton? FindToggleButton(string contentIdOrTitle)
			=> FindToggleButtons().FirstOrDefault(b =>
				string.Equals(b.Anchorable.ContentId, contentIdOrTitle, StringComparison.Ordinal)
				|| string.Equals(b.Content?.ToString(), contentIdOrTitle, StringComparison.Ordinal));

		private static Dictionary<string, object> DescribeToggleButton(ToggleDockButton button)
			=> new()
			{
				["contentId"] = button.Anchorable.ContentId,
				["title"] = button.Content?.ToString(),
				["zone"] = button.Zone.ToString(),
				["isChecked"] = button.IsChecked,
				["isEnabled"] = button.IsEnabled,
				["toolTip"] = button.ToolTip?.ToString(),
				["isVisible"] = button.IsVisible,
				["isAnchorableAutoHidden"] = button.Anchorable.IsAutoHidden,
				["isAnchorableVisible"] = button.Anchorable.IsVisible,
			};

		/// <param name="path">The layout file to load.</param>
		private void LoadLayout(string path)
		{
			if (!File.Exists(path) || DataContext is not MainViewModel viewModel)
			{
				return;
			}

			var serializer = new XmlLayoutSerializer(dockManager);
			serializer.LayoutSerializationCallback += (_, args) =>
			{
				var toolbox = viewModel.LayoutService.Anchorables
					.OfType<IToolbox>()
					.FirstOrDefault(t => t.Id == args.Model.ContentId);

				// Only overwrite when this application owns the content id. Leaving the value alone
				// keeps the content the previous layout supplied, which is what restores documents.
				if (toolbox != null)
				{
					args.Content = toolbox;
				}
			};

			serializer.Deserialize(path);
		}
	}
}
