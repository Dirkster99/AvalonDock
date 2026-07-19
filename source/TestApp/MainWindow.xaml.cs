/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.ProGPU;
using System.Windows.Threading;
using AvalonDock.Core;
using AvalonDock.Controls;
using AvalonDock.Layout;
using System.Diagnostics;
using System.IO;
using AvalonDock.Serializer.Xml;
using AvalonDock;
using AvalonDock.Themes;
using AvalonDock.Themes.VS;
using System.Diagnostics.CodeAnalysis;
using LeXtudio.DevFlow.Agent.Core;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace TestApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "MainWindow intentionally orchestrates many UI framework types in this sample app.")]
	[DevFlowUIThread]
	public partial class MainWindow : Window
	{
		private const string ObjC = "/usr/lib/libobjc.dylib";

		private enum GetWindowCmd : uint
		{
			GW_HWNDLAST = 1,
			GW_HWNDPREV = 3,
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

		[DllImport(ObjC, EntryPoint = "objc_getClass")]
		private static extern IntPtr ObjCGetClass(string name);

		[DllImport(ObjC, EntryPoint = "sel_registerName")]
		private static extern IntPtr Sel(string name);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern IntPtr ObjCMsgSend(IntPtr receiver, IntPtr selector);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern nuint ObjCMsgSendRetNUInt(IntPtr receiver, IntPtr selector);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		private static extern IntPtr ObjCMsgSendNUInt(IntPtr receiver, IntPtr selector, nuint arg);

		private static readonly IntPtr _nsApplicationClass = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			? ObjCGetClass("NSApplication")
			: IntPtr.Zero;
		private static readonly IntPtr _selSharedApplication = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			? Sel("sharedApplication")
			: IntPtr.Zero;
		private static readonly IntPtr _selOrderedWindows = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			? Sel("orderedWindows")
			: IntPtr.Zero;
		private static readonly IntPtr _selCount = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			? Sel("count")
			: IntPtr.Zero;
		private static readonly IntPtr _selObjectAtIndex = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
			? Sel("objectAtIndex:")
			: IntPtr.Zero;

		private readonly Dictionary<string, int> _inputEventCounts = new Dictionary<string, int>();
		private readonly HashSet<UIElement> _inputDiagnosticElements = new HashSet<UIElement>();
		private Point _lastDockManagerMousePosition;
		private MouseButtonState _lastDockManagerLeftButton;
		private string _lastInputOriginalSource;

		public MainWindow()
		{
			InitializeComponent();
			InstallInputDiagnostics();

			DispatcherTimer timer = new DispatcherTimer();
			Random rnd = new Random();
			timer.Interval = TimeSpan.FromSeconds(1.0);
			timer.Tick += (s, e) =>
				{
					TestTimer++;

					TestBackground = new SolidColorBrush(Color.FromRgb(
						(byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255)));

					FocusedElement = Keyboard.FocusedElement == null ? string.Empty : Keyboard.FocusedElement.ToString();
					//Debug.WriteLine(string.Format("ActiveContent = {0}", dockManager.ActiveContent));

				};
			timer.Start();

			this.DataContext = this;

			UpdateThemeColors();

		}

		private void InstallInputDiagnostics()
		{
			RefreshInputDiagnostics();
			dockManager.Loaded += (s, e) => RefreshInputDiagnostics();
		}

		private void RefreshInputDiagnostics()
		{
			AddInputDiagnostics(dockManager, "manager");
			var hitRoots = dockManager.FloatingWindows.Cast<DependencyObject>()
				.Concat(new DependencyObject[] { dockManager });
			foreach (var root in hitRoots)
			{
				foreach (var title in FindVisualDescendants<AnchorablePaneTitle>(root))
					AddInputDiagnostics(title, $"anchorable-title:{title.Model?.ContentId}");
				foreach (var tab in FindVisualDescendants<LayoutAnchorableTabItem>(root))
					AddInputDiagnostics(tab, $"anchorable-tab:{tab.Model?.ContentId}");
				foreach (var resizer in FindVisualDescendants<LayoutGridResizerControl>(root))
					AddInputDiagnostics(resizer, "anchorable-resizer");
			}
		}

		private void AddInputDiagnostics(UIElement element, string name)
		{
			if (!_inputDiagnosticElements.Add(element))
				return;

			element.AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler((s, e) => RecordInputEvent(name, "preview-down", e)), true);
			element.AddHandler(Mouse.PreviewMouseMoveEvent, new MouseEventHandler((s, e) => RecordInputEvent(name, "preview-move", e)), true);
			element.AddHandler(Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler((s, e) => RecordInputEvent(name, "preview-up", e)), true);
			element.AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler((s, e) => RecordInputEvent(name, "down", e)), true);
			element.AddHandler(Mouse.MouseMoveEvent, new MouseEventHandler((s, e) => RecordInputEvent(name, "move", e)), true);
			element.AddHandler(Mouse.MouseUpEvent, new MouseButtonEventHandler((s, e) => RecordInputEvent(name, "up", e)), true);
			element.AddHandler(Mouse.MouseLeaveEvent, new MouseEventHandler((s, e) => RecordInputEvent(name, "leave", e)), true);
		}

		private void RecordInputEvent(string name, string kind, MouseEventArgs e)
		{
			var key = $"{name}.{kind}";
			_inputEventCounts.TryGetValue(key, out var count);
			_inputEventCounts[key] = count + 1;
			_lastDockManagerMousePosition = Mouse.GetPosition(dockManager);
			_lastDockManagerLeftButton = Mouse.LeftButton;
			_lastInputOriginalSource = e.OriginalSource?.GetType().FullName;
		}


		/// <summary>
		/// TestTimer Dependency Property
		/// </summary>
		public static readonly DependencyProperty TestTimerProperty =
			DependencyProperty.Register(nameof(TestTimer), typeof(int), typeof(MainWindow),
				new FrameworkPropertyMetadata((int)0));

		/// <summary>
		/// Gets or sets the TestTimer property.  This dependency property 
		/// indicates a test timer that elapses evry one second (just for binding test).
		/// </summary>
		public int TestTimer
		{
			get => (int)GetValue(TestTimerProperty);
			set => SetValue(TestTimerProperty, value);
		}



		/// <summary>
		/// TestBackground Dependency Property
		/// </summary>
		public static readonly DependencyProperty TestBackgroundProperty =
			DependencyProperty.Register(nameof(TestBackground), typeof(Brush), typeof(MainWindow),
				new FrameworkPropertyMetadata((Brush)null));

		/// <summary>
		/// Gets or sets the TestBackground property.  This dependency property 
		/// indicates a randomly changing brush (just for testing).
		/// </summary>
		public Brush TestBackground
		{
			get => (Brush)GetValue(TestBackgroundProperty);
			set => SetValue(TestBackgroundProperty, value);
		}



		/// <summary>
		/// FocusedElement Dependency Property
		/// </summary>
		public static readonly DependencyProperty FocusedElementProperty =
			DependencyProperty.Register(nameof(FocusedElement), typeof(string), typeof(MainWindow),
				new FrameworkPropertyMetadata((IInputElement)null));

		/// <summary>
		/// Gets or sets the FocusedElement property.  This dependency property 
		/// indicates ....
		/// </summary>
		public string FocusedElement
		{
			get => (string)GetValue(FocusedElementProperty);
			set => SetValue(FocusedElementProperty, value);
		}


		private void OnLayoutRootPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var activeContent = ((LayoutRoot)sender).ActiveContent;
			if (e.PropertyName == "ActiveContent")
			{
				Debug.WriteLine(string.Format("ActiveContent-> {0}", activeContent));
			}
		}

        [SuppressMessage("Style", "IDE0063:使用简单的 \"using\" 语句", Justification = "<挂起>")]
        private void OnLoadLayout(object sender, RoutedEventArgs e)
		{
			var currentContentsList = dockManager.Layout.Descendents().OfType<LayoutContent>().Where(c => c.ContentId != null).ToArray();

			string fileName = (sender as MenuItem).Header.ToString();
			var serializer = new XmlLayoutSerializer(dockManager);
			//serializer.LayoutSerializationCallback += (s, args) =>
			//    {
			//        var prevContent = currentContentsList.FirstOrDefault(c => c.ContentId == args.Model.ContentId);
			//        if (prevContent != null)
			//            args.Content = prevContent.Content;
			//    };
			using (var stream = new StreamReader(string.Format(@".\AvalonDock_{0}.config", fileName)))
				serializer.Deserialize(stream);
		}

		[SuppressMessage("Style", "IDE0063:使用简单的 \"using\" 语句", Justification = "<挂起>")]
		private void OnSaveLayout(object sender, RoutedEventArgs e)
		{
			string fileName = (sender as MenuItem).Header.ToString();
			var serializer = new XmlLayoutSerializer(dockManager);
			using (var stream = new StreamWriter(string.Format(@".\AvalonDock_{0}.config", fileName)))
				serializer.Serialize(stream);
		}

		private void OnShowWinformsWindow(object sender, RoutedEventArgs e)
		{
			var winFormsWindow = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "WinFormsWindow");
			if (winFormsWindow.IsHidden)
				winFormsWindow.Show();
			else if (winFormsWindow.IsVisible)
				winFormsWindow.IsActive = true;
			else
				winFormsWindow.AddToLayout(dockManager, AnchorableShowStrategy.Bottom | AnchorableShowStrategy.Most);
		}

		private void AddTwoDocuments_click(object sender, RoutedEventArgs e)
		{
			var firstDocumentPane = dockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
			if (firstDocumentPane != null)
			{
                LayoutDocument doc = new LayoutDocument
                {
                    Title = "Test1"
                };
                firstDocumentPane.Children.Add(doc);

                LayoutDocument doc2 = new LayoutDocument
                {
                    Title = "Test2"
                };
                firstDocumentPane.Children.Add(doc2);
			}

			var leftAnchorGroup = dockManager.Layout.LeftSide.Children.FirstOrDefault();
			if (leftAnchorGroup == null)
			{
				leftAnchorGroup = new LayoutAnchorGroup();
				dockManager.Layout.LeftSide.Children.Add(leftAnchorGroup);
			}

			leftAnchorGroup.Children.Add(new LayoutAnchorable() { Title = "New Anchorable" });

		}

		private void OnShowToolWindow1(object sender, RoutedEventArgs e)
		{
			var toolWindow1 = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "toolWindow1");
			if (toolWindow1.IsHidden)
				toolWindow1.Show();
			else if (toolWindow1.IsVisible)
				toolWindow1.IsActive = true;
			else
				toolWindow1.AddToLayout(dockManager, AnchorableShowStrategy.Bottom | AnchorableShowStrategy.Most);
		}

		private void DockManager_DocumentClosing(object sender, DocumentClosingEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to close the document?", "AvalonDock Sample", MessageBoxButton.YesNo) == MessageBoxResult.No)
				e.Cancel = true;
		}

		private void OnDumpToConsole(object sender, RoutedEventArgs e)
		{
			// Uncomment when TRACE is activated on AvalonDock project
			// dockManager.Layout.ConsoleDump(0);
		}

		private void OnReloadManager(object sender, RoutedEventArgs e)
		{
		}

		private void OnUnloadManager(object sender, RoutedEventArgs e)
		{
			if (layoutRoot.Children.Contains(dockManager))
				layoutRoot.Children.Remove(dockManager);
		}

		private void OnLoadManager(object sender, RoutedEventArgs e)
		{
			if (!layoutRoot.Children.Contains(dockManager))
				layoutRoot.Children.Add(dockManager);
		}

		private void OnToolWindow1Hiding(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to hide this tool?", "AvalonDock", MessageBoxButton.YesNo) == MessageBoxResult.No)
				e.Cancel = true;
		}

		private void OnShowHeader(object sender, RoutedEventArgs e)
		{
			////            LayoutDocumentPane.ShowHeader = !LayoutDocumentPane.ShowHeader;
		}

		/// <summary>
		/// Method create a new anchorable window to test whether a floating window will auto-adjust its size to the
		/// containing control. See <see cref="DockingManager.AutoWindowSizeWhenOpened"/> dependency property.
		/// and TestUserControl in this demo App for more details.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNewFloatingWindow(object sender, RoutedEventArgs e)
        {
            var view = new TestUserControl();
            var anchorable = new LayoutAnchorable()
            {
                Title = "Floating window with initial usercontrol size",
				Content = view
			};
            anchorable.AddToLayout(dockManager,AnchorableShowStrategy.Most);
            anchorable.Float();
        }

		private void OnSwitchTheme(object sender, RoutedEventArgs e)
		{
			var menuItem = sender as MenuItem;
			if (menuItem == null)
				return;

			var themeTag = menuItem.Tag as string;
			if (themeTag == null)
				return;

			Theme theme;
			switch (themeTag)
			{
				case "ArcDark": theme = new ArcDarkTheme(); break;
				case "ArcLight": theme = new ArcLightTheme(); break;
				case "VS2013Dark": theme = new Vs2013DarkTheme(); break;
				case "VS2013Light": theme = new Vs2013LightTheme(); break;
				case "VS2013Blue": theme = new Vs2013BlueTheme(); break;
				case "VS2026Dark": theme = new VS2026DarkTheme(); break;
				case "VS2026Light": theme = new VS2026LightTheme(); break;
				case "VS2026Blue": theme = new VS2026BlueTheme(); break;
				case "VS2022Dark": theme = new VS2022DarkTheme(); break;
				case "VS2022Light": theme = new VS2022LightTheme(); break;
				case "VS2022Blue": theme = new VS2022BlueTheme(); break;
				case "VS2015Dark": theme = new VS2015DarkTheme(); break;
				case "VS2015Light": theme = new VS2015LightTheme(); break;
				case "VS2015Blue": theme = new VS2015BlueTheme(); break;
				case "VS2010": theme = new VS2010Theme(); break;
				case "ExpressionDark": theme = new ExpressionDarkTheme(); break;
				case "ExpressionLight": theme = new ExpressionLightTheme(); break;
				case "Metro": theme = new MetroTheme(); break;
				case "Aero": theme = new AeroTheme(); break;
				case "Generic": theme = new GenericTheme(); break;
				default: theme = new ArcDarkTheme(); break;
			}

			dockManager.Theme = theme;

			// Mark only the selected theme in the menu
			foreach (var item in themeMenu.Items)
			{
				if (item is MenuItem mi && mi.IsCheckable)
					mi.IsChecked = mi == menuItem;
			}

			// Adapt UI colors to match the theme
			UpdateThemeColors();
		}

		private void UpdateThemeColors()
		{
			Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
			{
				bool isDark = IsDarkBrush(dockManager.Background);
				var foreground = isDark ? Brushes.White : Brushes.Black;

				// Set foreground on content that needs adaptive text
				Foreground = foreground;
				mainMenu.Foreground = foreground;
				tbToolWindow1Timer.Foreground = foreground;
				tbAutoHide1Timer.Foreground = foreground;
				tbDocument2Timer.Foreground = foreground;
				TextElement.SetForeground(spAutoHide2, foreground);

				// Update window title bar color via DWM
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

			// Windows 11+: set exact caption color
			var scb = dockManager.Background as SolidColorBrush;
			if (scb != null)
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
				double luminance = 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
				return luminance < 128;
			}
			return false;
		}

		[DevFlowAction("avd.float", Description = "Float an anchorable by ContentId")]
		public string FloatAnchorable(string contentId)
		{
			var anchorable = dockManager.Layout.Descendents()
				.OfType<LayoutAnchorable>()
				.FirstOrDefault(a => a.ContentId == contentId && !a.IsFloating);
			if (anchorable == null)
				return $"Anchorable '{contentId}' not found";
			anchorable.Float();
			return $"Floated '{contentId}'";
		}

		[DevFlowAction("avd.position-floating",
			Description = "Move the floating window containing the given ContentId to a fixed screen " +
			              "position (default: clear of the main window and screen origin, where other " +
			              "app windows commonly sit and can steal synthetic clicks aimed at the tester).")]
		public string PositionFloatingWindow(string contentId, double left = 900, double top = 200)
		{
			var floating = FindVisibleFloatingWindow(contentId);
			if (floating == null)
				return $"No floating window found for '{contentId}'";

			floating.Left = left;
			floating.Top = top;
			floating.UpdateLayout();
			return $"Positioned floating window for '{contentId}' at {left},{top}";
		}

		[DevFlowAction("avd.query.floating-zorder", Description = "Compare a floating window's OS z-order against the main window")]
		public string QueryFloatingZOrder(string contentId)
		{
			var floating = FindVisibleFloatingWindow(contentId);
			if (floating == null)
			{
				return System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
				{
					["found"] = false,
					["contentId"] = contentId,
				});
			}

			var mainHandle = new WindowInteropHelper(this).Handle;
			var floatingHandle = new WindowInteropHelper(floating).Handle;
			var floatingContentTitle = floating.Model?.Descendents()
				.OfType<LayoutAnchorable>()
				.FirstOrDefault(a => string.Equals(a.ContentId, contentId, StringComparison.Ordinal))
				?.Title;
			var mainFound = ProGpuWpfDiagnostics.TryGetWindowZOrder(this, out var mainZ) ||
				TryGetPlatformWindowZOrder(mainHandle, out mainZ);
			var floatingFound = ProGpuWpfDiagnostics.TryGetWindowZOrder(floating, out var floatingZ) ||
				TryGetPlatformWindowZOrder(floatingHandle, out floatingZ);
			return System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
			{
				["found"] = true,
				["contentId"] = contentId,
				["mainTitle"] = Title,
				["floatingTitle"] = string.IsNullOrWhiteSpace(floating.Title) ? floatingContentTitle : floating.Title,
				["mainHandle"] = mainHandle.ToInt64(),
				["floatingHandle"] = floatingHandle.ToInt64(),
				["mainZOrderFound"] = mainFound,
				["floatingZOrderFound"] = floatingFound,
				["mainZOrder"] = mainZ,
				["floatingZOrder"] = floatingZ,
				["isFloatingAboveMain"] = mainFound && floatingFound && IsPlatformZOrderAbove(floatingZ, mainZ),
				["zOrderConvention"] = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
					? "lower index is frontmost in NSApplication.orderedWindows"
					: "higher z-order is frontmost in Win32 GetWindow walk",
				["floatingLeft"] = floating.Left,
				["floatingTop"] = floating.Top,
				["floatingWidth"] = floating.ActualWidth,
				["floatingHeight"] = floating.ActualHeight,
				["mainIsActive"] = IsActive,
				["floatingIsActive"] = floating.IsActive,
				["floatingTopmost"] = floating.Topmost,
			});
		}

		[DevFlowAction("avd.dock", Description = "Dock a floating anchorable back to main layout")]
		public string DockAnchorable(string contentId)
		{
			var anchorable = dockManager.Layout.FloatingWindows
				.SelectMany(f => f.Descendents())
				.OfType<LayoutAnchorable>()
				.FirstOrDefault(a => a.ContentId == contentId && a.IsFloating);
			if (anchorable == null)
				return $"Anchorable '{contentId}' not found";
			anchorable.Dock();
			return $"Docked '{contentId}'";
		}

		[DevFlowAction("avd.hide", Description = "Hide an anchorable")]
		public string HideAnchorable(string contentId)
		{
			var anchorable = dockManager.Layout.Descendents()
				.OfType<LayoutAnchorable>()
				.FirstOrDefault(a => a.ContentId == contentId);
			if (anchorable == null)
				return $"Anchorable '{contentId}' not found";
			anchorable.Hide();
			return $"Hidden '{contentId}'";
		}

		[DevFlowAction("avd.show", Description = "Show a hidden anchorable")]
		public string ShowAnchorable(string contentId)
		{
			var anchorable = dockManager.Layout.Descendents()
				.OfType<LayoutAnchorable>()
				.FirstOrDefault(a => a.ContentId == contentId);
			if (anchorable == null)
				return $"Anchorable '{contentId}' not found";
			if (!anchorable.IsHidden)
				return $"'{contentId}' is not hidden";
			anchorable.Show();
			return $"Shown '{contentId}'";
		}

		[DevFlowAction("avd.add-documents", Description = "Add two test documents to the first document pane")]
		public string AddDocuments()
		{
			var firstPane = dockManager.Layout.Descendents()
				.OfType<LayoutDocumentPane>()
				.FirstOrDefault();
			if (firstPane == null)
				return "No document pane found";

			var id1 = $"doc-{Guid.NewGuid():N}";
			var id2 = $"doc-{Guid.NewGuid():N}";
			firstPane.Children.Add(new LayoutDocument { Title = "TestDoc1", ContentId = id1 });
			firstPane.Children.Add(new LayoutDocument { Title = "TestDoc2", ContentId = id2 });
			return $"Added documents '{id1}', '{id2}'";
		}

		[DevFlowAction("avd.add-anchorable", Description = "Add a new anchorable to the layout")]
		public string AddAnchorable(string title = null)
		{
			var anchorable = new LayoutAnchorable
			{
				Title = title ?? "New Anchorable",
				ContentId = $"anch-{Guid.NewGuid():N}"
			};
			anchorable.AddToLayout(dockManager, AnchorableShowStrategy.Most);
			return $"Added anchorable '{anchorable.ContentId}'";
		}

		[DevFlowAction("avd.test-layout.reset", Description = "Reset to a deterministic AvalonDock drag/drop test layout")]
		public string ResetDragDropTestLayout()
		{
			foreach (var floatingWindow in dockManager.FloatingWindows.ToArray())
			{
				floatingWindow.Close();
			}

			foreach (Window floatingWindow in Application.Current.Windows
				.OfType<Window>()
				.Where(w => w != this && w.GetType().Name.Contains("FloatingWindowControl", StringComparison.Ordinal))
				.ToArray())
			{
				floatingWindow.Close();
			}

			var tool = new LayoutAnchorable
			{
				Title = "Drag Test Tool",
				ContentId = "dragTestTool",
				CanHide = true,
				CanClose = false,
				Content = new Border
				{
					Background = Brushes.Transparent,
					Child = new TextBlock
					{
						Text = "Drag Test Tool",
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center
					}
				}
			};
				var toolPane = new LayoutAnchorablePane(tool)
				{
					DockWidth = new GridLength(260)
				};
				toolPane.SelectedContentIndex = 0;
			var documentPane = new LayoutDocumentPane();
			documentPane.Children.Add(new LayoutDocument
			{
				Title = "Drag Test Document",
				ContentId = "dragTestDocument",
				Content = new Grid { Background = Brushes.Transparent }
			});
			// A second, separate DOCKED anchorable pane. AnchorablePaneDock{Left,Top,Right,Bottom,Inside}
			// compass indicators only appear while the pointer hovers an EXISTING anchorable pane, and
			// with only one anchorable (dragTestTool, which is what gets floated/dragged in these tests)
			// there would be no remaining docked anchorable pane left to host them once it floats away.
			var tool2 = new LayoutAnchorable
			{
				Title = "Drag Test Tool 2",
				ContentId = "dragTestTool2",
				CanHide = true,
				CanClose = false,
				Content = new Border
				{
					Background = Brushes.Transparent,
					Child = new TextBlock
					{
						Text = "Drag Test Tool 2",
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center
					}
				}
			};
				var toolPane2 = new LayoutAnchorablePane(tool2)
				{
					DockWidth = new GridLength(260)
				};
				toolPane2.SelectedContentIndex = 0;
			var root = new LayoutRoot
			{
				RootPanel = new LayoutPanel
				{
					Orientation = Orientation.Horizontal,
					Children =
					{
						toolPane,
						documentPane,
						toolPane2
					}
				}
			};

				dockManager.Layout = root;
				tool.IsSelected = true;
				dockManager.UpdateLayout();
			RefreshInputDiagnostics();
			return QueryLayout();
		}

		[DevFlowAction("avd.switch-theme", Description = "Switch AvalonDock theme by tag name")]
		public string SwitchTheme(string themeTag)
		{
			Theme theme = themeTag switch
			{
				"ArcDark" => new ArcDarkTheme(),
				"ArcLight" => new ArcLightTheme(),
				"VS2013Dark" => new Vs2013DarkTheme(),
				"VS2013Light" => new Vs2013LightTheme(),
				"VS2013Blue" => new Vs2013BlueTheme(),
				"VS2026Dark" => new VS2026DarkTheme(),
				"VS2026Light" => new VS2026LightTheme(),
				"VS2026Blue" => new VS2026BlueTheme(),
				"VS2022Dark" => new VS2022DarkTheme(),
				"VS2022Light" => new VS2022LightTheme(),
				"VS2022Blue" => new VS2022BlueTheme(),
				"VS2015Dark" => new VS2015DarkTheme(),
				"VS2015Light" => new VS2015LightTheme(),
				"VS2015Blue" => new VS2015BlueTheme(),
				"VS2010" => new VS2010Theme(),
				"ExpressionDark" => new ExpressionDarkTheme(),
				"ExpressionLight" => new ExpressionLightTheme(),
				"Metro" => new MetroTheme(),
				"Aero" => new AeroTheme(),
				"Generic" => new GenericTheme(),
				_ => null
			};
			if (theme == null) return $"Unknown theme '{themeTag}'";
			dockManager.Theme = theme;
			UpdateThemeColors();
			return $"Switched to '{themeTag}'";
		}

		[DevFlowAction("avd.layout.serialize", Description = "Serialize current layout to XML and return it")]
		public string SerializeLayout()
		{
			var serializer = new XmlLayoutSerializer(dockManager);
			using var ms = new MemoryStream();
			using (var writer = new StreamWriter(ms, leaveOpen: true))
				serializer.Serialize(writer);
			ms.Position = 0;
			using var reader = new StreamReader(ms);
			return reader.ReadToEnd();
		}

		[DevFlowAction("avd.layout.restore", Description = "Restore layout from XML string")]
		public string RestoreLayout(string xml)
		{
			var serializer = new XmlLayoutSerializer(dockManager);
			using var reader = new StringReader(xml);
			serializer.Deserialize(reader);
			return "Layout restored";
		}

		[DevFlowAction("avd.query.layout", Description = "Query current layout state as JSON")]
		public string QueryLayout()
		{
			var anchorables = dockManager.Layout.Descendents()
				.OfType<LayoutAnchorable>()
				.Select(a => new Dictionary<string, object>
				{
					["contentId"] = a.ContentId,
					["title"] = a.Title,
					["isVisible"] = a.IsVisible,
					["isHidden"] = a.IsHidden,
					["isFloat"] = a.IsFloating,
					["canClose"] = a.CanClose,
					["canHide"] = a.CanHide,
				}).ToList();

			var documents = dockManager.Layout.Descendents()
				.OfType<LayoutDocument>()
				.Select(d => new Dictionary<string, object>
				{
					["contentId"] = d.ContentId,
					["title"] = d.Title,
					["isVisible"] = d.IsVisible,
				}).ToList();

			var floatingWindows = dockManager.Layout.Descendents()
				.OfType<LayoutFloatingWindow>()
				.Select(f =>
				{
					var contents = f.Descendents().OfType<LayoutContent>().ToArray();
					var firstContent = contents.FirstOrDefault();
					return new Dictionary<string, object>
					{
						["type"] = f.GetType().Name,
						["contentIds"] = contents.Select(c => c.ContentId).ToArray(),
						["floatingLeft"] = firstContent?.FloatingLeft,
						["floatingTop"] = firstContent?.FloatingTop,
						["floatingWidth"] = firstContent?.FloatingWidth,
						["floatingHeight"] = firstContent?.FloatingHeight,
					};
				}).ToList();

			var result = new Dictionary<string, object>
			{
				["anchorables"] = anchorables,
				["documents"] = documents,
				["floatingWindows"] = floatingWindows,
				["activeContent"] = dockManager.ActiveContent?.ToString(),
				["activeContentId"] = dockManager.Layout.ActiveContent?.ContentId,
			};

			return System.Text.Json.JsonSerializer.Serialize(result);
		}

			[DevFlowAction("avd.query.bounds", Description = "Query screen bounds for a dock test target")]
			public string QueryBounds(string target, string contentId = null)
			{
				FrameworkElement element = target switch
				{
				"main-window" => this,
				"menu" => mainMenu,
				"manager" => dockManager,
					"anchorable-title" => FindAnchorableTitle(contentId),
					"anchorable-tab" => FindVisualDescendants<LayoutAnchorableTabItem>(dockManager)
							.Where(x => MatchesAnchorableContent(x, contentId))
							.FirstOrDefault(x => x.IsVisible && x.ActualWidth > 0 && x.ActualHeight > 0),
					"document-tab" => FindVisualDescendants<LayoutDocumentTabItem>(dockManager)
						.Where(x => string.Equals(x.Model?.ContentId, contentId, StringComparison.Ordinal))
						.FirstOrDefault(x => x.IsVisible && x.ActualWidth > 0 && x.ActualHeight > 0),
					"document-pane" => FindVisualDescendant<LayoutDocumentPaneControl>(dockManager, _ => true),
					"anchorable-pane" => FindVisualDescendant<LayoutAnchorablePaneControl>(
						dockManager,
						x => x.Model?.Descendents().OfType<LayoutAnchorable>()
						.Any(a => string.Equals(a.ContentId, contentId, StringComparison.Ordinal)) == true),
				"anchorable-resizer" => FindAnchorablePaneResizer(contentId),
					"floating-window" => string.IsNullOrEmpty(contentId)
						? dockManager.FloatingWindows.FirstOrDefault()
						: FindVisibleFloatingWindow(contentId),
				_ => null
			};

				return System.Text.Json.JsonSerializer.Serialize(CreateBoundsPayload(target, contentId, element));
			}

			[DevFlowAction("avd.query.drag-handle", Description = "Query a verified screen drag start point for an AvalonDock target")]
			public string QueryDragHandle(string target, string contentId = null)
			{
				if (target == "floating-caption")
					return QueryFloatingCaptionDragHandle(contentId);

				FrameworkElement element = target switch
				{
					"docked-anchorable" => FindDockedAnchorableDragHandle(contentId),
					"anchorable-resizer" => FindAnchorablePaneResizer(contentId),
					"document-body" => FindVisualDescendant<LayoutDocumentPaneControl>(dockManager, _ => true),
					"manager" => dockManager,
					_ => null
				};

				var payload = CreateBoundsPayload(target, contentId, element);
				if (element != null)
				{
					payload["handleKind"] = target;
					payload["hitTest"] = HitTest((double)payload["centerX"], (double)payload["centerY"]);
					payload["managerBounds"] = CreateBoundsPayload("manager", null, dockManager);
					var window = Window.GetWindow(element);
					if (window != null)
						payload["windowBounds"] = CreateBoundsPayload("window", null, window);
				}

				return System.Text.Json.JsonSerializer.Serialize(payload);
			}

			private string QueryFloatingCaptionDragHandle(string contentId)
			{
				var floating = FindVisibleFloatingWindow(contentId);
				if (floating == null)
				{
					return System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
					{
						["target"] = "floating-caption",
						["contentId"] = contentId,
						["found"] = false,
						["reason"] = "floating window not found",
					});
				}

				var captionHeight = Math.Min(28d, Math.Max(12d, floating.ActualHeight / 6d));
				var leftInset = 8d;
				var topInset = 6d;
				var width = Math.Max(24d, Math.Min(180d, floating.ActualWidth - leftInset * 2d));
				var height = Math.Max(12d, captionHeight - topInset);
				var topLeft = PointToScreenPortable(floating, new Point(leftInset, topInset));
				var center = PointToScreenPortable(floating, new Point(leftInset + width / 2d, topInset + height / 2d));
				var result = new Dictionary<string, object>
				{
					["target"] = "floating-caption",
					["contentId"] = contentId,
					["found"] = true,
					["x"] = topLeft.X,
					["y"] = topLeft.Y,
					["width"] = width,
					["height"] = height,
					["centerX"] = center.X,
					["centerY"] = center.Y,
					["hitTestPoint"] = false,
					["handleKind"] = "floating-caption",
					["windowBounds"] = CreateBoundsPayload("floating-window", contentId, floating),
					["managerBounds"] = CreateBoundsPayload("manager", null, dockManager),
				};
				result["hitTest"] = HitTest((double)result["centerX"], (double)result["centerY"]);
				return System.Text.Json.JsonSerializer.Serialize(result);
			}

			[DevFlowAction("avd.debug-show-overlay", Description = "Force-show the DockingManager compass overlay (with a visible debug border) without a drag; optionally enter a drop-target zone to also render its blue preview box, for inspecting overlay/preview alignment vs the manager")]
			public string DebugShowOverlay(string previewZone = null)
			{
				OverlayWindow.DebugBorderEnabled = true;

				var floating = dockManager.FloatingWindows.FirstOrDefault(fw => fw.IsLoaded && fw.IsVisible);
				if (floating == null)
				{
					var anchorable = dockManager.Layout.Descendents().OfType<LayoutAnchorable>()
						.FirstOrDefault(a => !a.IsFloating && a.IsVisible);
					if (anchorable == null)
						return System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object> { ["shown"] = false, ["reason"] = "no anchorable to float" });
					anchorable.Float();
					floating = dockManager.FloatingWindows.FirstOrDefault(fw => fw.IsLoaded && fw.IsVisible);
				}

				if (floating == null)
					return System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object> { ["shown"] = false, ["reason"] = "no floating window available" });

				var host = (IOverlayWindowHost)dockManager;
				var overlay = host.ShowOverlayWindow(floating);
				overlay.DragEnter(floating);
				foreach (var area in host.GetDropAreas(floating))
					overlay.DragEnter(area);

				object previewInfo = null;
				if (!string.IsNullOrWhiteSpace(previewZone))
				{
					var target = overlay.GetTargets().FirstOrDefault(t => t.Type.ToString() == previewZone);
					if (target != null)
					{
						overlay.DragEnter(target);
						previewInfo = new Dictionary<string, object>
						{
							["zone"] = previewZone,
							["targetScreenBounds"] = RectToPayload(target.GetScreenBounds()),
						};
					}
					else
					{
						previewInfo = new Dictionary<string, object> { ["zone"] = previewZone, ["found"] = false };
					}
				}

				var payload = new Dictionary<string, object>
				{
					["shown"] = true,
					["overlay"] = (overlay is Window w)
						? new Dictionary<string, object> { ["left"] = w.Left, ["top"] = w.Top, ["width"] = w.ActualWidth, ["height"] = w.ActualHeight, ["bottom"] = w.Top + w.ActualHeight }
						: null,
					["menuBounds"] = CreateBoundsPayload("menu", null, mainMenu),
					["managerBounds"] = CreateBoundsPayload("manager", null, dockManager),
					["preview"] = previewInfo,
				};
				return System.Text.Json.JsonSerializer.Serialize(payload);
			}

			private static Dictionary<string, object> RectToPayload(Rect r) => new Dictionary<string, object>
			{
				["x"] = r.X,
				["y"] = r.Y,
				["width"] = r.Width,
				["height"] = r.Height,
			};

			[DevFlowAction("avd.debug-hide-overlay", Description = "Hide the debug compass overlay shown by avd.debug-show-overlay")]
			public string DebugHideOverlay()
			{
				((IOverlayWindowHost)dockManager).HideOverlayWindow();
				OverlayWindow.DebugBorderEnabled = false;
				return "Hidden debug overlay";
			}

			[DevFlowAction("avd.activate", Description = "Activate and foreground the AvalonDock test window")]
			public string ActivateTestWindow()
			{
				if (WindowState == WindowState.Minimized)
					WindowState = WindowState.Normal;
			Activate();
			Focus();
			return System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
			{
				["isActive"] = IsActive,
					["isKeyboardFocusWithin"] = IsKeyboardFocusWithin,
				});
			}

			[DevFlowAction("avd.position-main-window", Description = "Move the AvalonDock test window to the primary-screen test area")]
			public string PositionMainWindow(double left = 50, double top = 40)
			{
				if (WindowState == WindowState.Minimized)
					WindowState = WindowState.Normal;
				WindowStartupLocation = WindowStartupLocation.Manual;
				Left = left;
				Top = top;
				Width = 900;
				Height = 700;
				UpdateLayout();
				Activate();
				return System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
				{
					["left"] = Left,
					["top"] = Top,
					["width"] = ActualWidth,
					["height"] = ActualHeight,
					["isActive"] = IsActive,
				});
			}

			[DevFlowAction("avd.input.reset", Description = "Reset AvalonDock routed input diagnostics")]
			public string ResetInputDiagnostics()
			{
				if (Mouse.Captured is Menu || Mouse.Captured is MenuItem)
					Mouse.Capture(null);
				_inputEventCounts.Clear();
				_lastDockManagerMousePosition = default;
				_lastDockManagerLeftButton = Mouse.LeftButton;
			_lastInputOriginalSource = null;
			return "reset";
		}

		[DevFlowAction("avd.input.query", Description = "Query AvalonDock routed input diagnostics")]
		public string QueryInputDiagnostics()
		{
			var result = new Dictionary<string, object>
			{
				["counts"] = _inputEventCounts.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value),
				["mouseX"] = _lastDockManagerMousePosition.X,
				["mouseY"] = _lastDockManagerMousePosition.Y,
				["leftButton"] = _lastDockManagerLeftButton.ToString(),
				["captured"] = Mouse.Captured?.GetType().FullName,
				["directlyOver"] = Mouse.DirectlyOver?.GetType().FullName,
				["originalSource"] = _lastInputOriginalSource,
			};

			return System.Text.Json.JsonSerializer.Serialize(result);
		}

		[DevFlowAction("avd.query.platform", Description = "Query LibreWPF platform coordinate diagnostics")]
		public string QueryPlatformDiagnostics()
		{
			var source = PresentationSource.FromVisual(this);
			var windowOrigin = PointToScreen(new Point(0, 0));
			var managerOrigin = dockManager.PointToScreen(new Point(0, 0));
			var clientOrigin = TryReadPortableClientOrigin(source);
			var assemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => a.GetName().Name is "ProGPU.Wpf" or "PresentationCore" or "PresentationFramework")
				.Select(a => new Dictionary<string, object>
				{
					["name"] = a.GetName().Name,
					["version"] = a.GetName().Version?.ToString(),
					["location"] = a.Location,
				})
				.ToArray();

			var result = new Dictionary<string, object>
			{
				["windowLeft"] = Left,
				["windowTop"] = Top,
				["actualWidth"] = ActualWidth,
				["actualHeight"] = ActualHeight,
				["windowPointToScreenX"] = windowOrigin.X,
				["windowPointToScreenY"] = windowOrigin.Y,
				["managerPointToScreenX"] = managerOrigin.X,
				["managerPointToScreenY"] = managerOrigin.Y,
				["sourceType"] = source?.GetType().FullName,
				["clientOrigin"] = clientOrigin,
				["assemblies"] = assemblies,
			};

			return System.Text.Json.JsonSerializer.Serialize(result);
		}

		private static object TryReadPortableClientOrigin(PresentationSource source)
		{
			if (source == null)
				return null;

			try
			{
				var portableSource = source;
				var portableOwnerProperty = source.GetType().GetProperty(
					"PortableOwner",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				if (portableOwnerProperty?.GetValue(source) is PresentationSource portableOwner)
					portableSource = portableOwner;

				var property = portableSource.GetType().GetProperty(
					"ClientOrigin",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				if (property?.GetValue(portableSource) is Point point)
					return new Dictionary<string, object> { ["x"] = point.X, ["y"] = point.Y };
			}
			catch
			{
			}

			return null;
		}

		[DevFlowAction("avd.query.active-drop-targets",
			Description = "During an active drag (a floating window whose caption is currently being " +
			              "dragged), returns every currently-visible compass drop-target indicator as " +
			              "{type, x, y, width, height, centerX, centerY} - type is a DropTargetType name " +
			              "(e.g. DockingManagerDockLeft, AnchorablePaneDockInside). Which indicators are " +
			              "visible depends on which host area the pointer is currently over, matching " +
			              "real AvalonDock drag behavior, so move the pointer near the desired target's " +
			              "pane/edge before calling this.")]
		public string QueryActiveDropTargets()
		{
			var results = new List<Dictionary<string, object>>();
			foreach (var floating in dockManager.FloatingWindows)
			{
				var overlay = floating.CurrentDragService?.CurrentOverlayWindow;
				if (overlay == null)
					continue;

				foreach (var target in overlay.GetTargets())
				{
					var bounds = target.GetScreenBounds();
					results.Add(new Dictionary<string, object>
					{
						["type"] = target.Type.ToString(),
						["x"] = bounds.X,
						["y"] = bounds.Y,
						["width"] = bounds.Width,
						["height"] = bounds.Height,
						["centerX"] = bounds.X + bounds.Width / 2d,
						["centerY"] = bounds.Y + bounds.Height / 2d,
					});
				}
			}

			return System.Text.Json.JsonSerializer.Serialize(results);
		}

		[DevFlowAction("avd.query.drag-state", Description = "Query live floating-window drag state")]
		public string QueryDragState()
		{
			return System.Text.Json.JsonSerializer.Serialize(dockManager.FloatingWindows.Select(floating => new
			{
				overlayLeft = (floating.CurrentDragService?.CurrentOverlayWindow as Window)?.Left,
				overlayTop = (floating.CurrentDragService?.CurrentOverlayWindow as Window)?.Top,
				overlayWidth = (floating.CurrentDragService?.CurrentOverlayWindow as Window)?.ActualWidth,
				overlayHeight = (floating.CurrentDragService?.CurrentOverlayWindow as Window)?.ActualHeight,
				overlayBackground = (floating.CurrentDragService?.CurrentOverlayWindow as Window)?.Background?.ToString(),
				overlayAllowsTransparency = (floating.CurrentDragService?.CurrentOverlayWindow as Window)?.AllowsTransparency,
				menuBounds = CreateBoundsPayload("menu", null, mainMenu),
				managerBounds = CreateBoundsPayload("manager", null, dockManager),
				title = floating.Title,
					currentDropTarget = floating.CurrentDragService?.CurrentDropTargetType,
					left = floating.Left,
					top = floating.Top,
					dragOffset = floating.PortableDragOffsetForDiagnostics,
					currentPointer = floating.CurrentPointerScreenPosition,
				}).ToArray());
		}

		[DevFlowAction("avd.hit-test", Description = "Hit test a screen point against the AvalonDock manager")]
		public string HitTest(double screenX, double screenY)
		{
			var screenPoint = new Point(screenX, screenY);
			var managerPoint = dockManager.PointFromScreen(screenPoint);
			DependencyObject hit = null;
			DependencyObject hitRoot = null;
			foreach (var root in GetAvalonDockVisualRoots())
			{
				if (root is not FrameworkElement rootElement || root is not Visual rootVisual)
					continue;

				Point rootPoint;
				try
				{
					rootPoint = PointFromScreenPortable(rootElement, screenPoint);
				}
				catch (InvalidOperationException)
				{
					continue;
				}

				if (rootPoint.X < 0 || rootPoint.Y < 0 || rootPoint.X > rootElement.ActualWidth || rootPoint.Y > rootElement.ActualHeight)
					continue;

				// InputHitTest does not descend past the DockingManager template's ContentPresenter in
				// this portable (LibreWPF/ProGPU) rendering backend - it returns the same shallow
				// ContentPresenter for every point, even pane centers. Resolve the hit geometrically
				// instead, using each element's own PointToScreen bounds (the same transform path
				// avd.query.bounds uses and which is reliable here, unlike TransformToDescendant): find
				// the deepest visible visual descendant whose screen rect contains the point. This
				// mirrors how drag handles are computed (from real element bounds), so a caller can
				// reliably verify a computed drag-start point actually lands on the intended AvalonDock
				// element (resizer, pane, caption) rather than a random position.
				hit = FindDeepestVisualContaining(rootElement, screenPoint) ?? rootElement;
				hitRoot = root;
				break;
			}

			var ancestors = new List<string>();
			for (var current = hit; current != null; current = VisualTreeHelper.GetParent(current))
			{
				ancestors.Add(current.GetType().FullName);
				if (ReferenceEquals(current, hitRoot))
					break;
			}

			return System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
			{
				["screenX"] = screenX,
				["screenY"] = screenY,
				["managerX"] = managerPoint.X,
				["managerY"] = managerPoint.Y,
				["root"] = hitRoot?.GetType().FullName,
				["hit"] = hit?.GetType().FullName,
				["ancestors"] = ancestors,
			});
		}

		[DevFlowAction("avd.query.tabs", Description = "Query visible anchorable tabs and diagnostic bounds")]
			public string QueryAnchorableTabs()
			{
				var tabs = FindVisualDescendants<LayoutAnchorableTabItem>(dockManager)
				.Select(tab => new Dictionary<string, object>
				{
					["contentId"] = tab.Model?.ContentId,
					["title"] = tab.Model?.Title,
					["isVisible"] = tab.IsVisible,
					["actualWidth"] = tab.ActualWidth,
					["actualHeight"] = tab.ActualHeight,
					["isHitTestableAtCenter"] = IsHitTestableAtCenter(tab),
					["bounds"] = CreateBoundsPayload("anchorable-tab", tab.Model?.ContentId, tab),
				})
				.ToArray();

				return System.Text.Json.JsonSerializer.Serialize(tabs);
			}

			[DevFlowAction("avd.query.anchorable-drag-surfaces", Description = "Query visible anchorable title/control drag surface diagnostics")]
			public string QueryAnchorableDragSurfaces()
			{
				var roots = GetAvalonDockVisualRoots().ToArray();
				var titles = roots.SelectMany(FindVisualDescendants<AnchorablePaneTitle>)
					.Select(title => new Dictionary<string, object>
					{
						["contentId"] = title.Model?.ContentId,
						["title"] = title.Model?.Title,
						["isVisible"] = title.IsVisible,
						["isHitTestVisible"] = title.IsHitTestVisible,
						["actualWidth"] = title.ActualWidth,
						["actualHeight"] = title.ActualHeight,
						["isHitTestableAtCenter"] = IsHitTestableAtCenter(title),
						["bounds"] = CreateBoundsPayload("anchorable-title", title.Model?.ContentId, title),
					})
					.ToArray();
				var controls = roots.SelectMany(FindVisualDescendants<LayoutAnchorableControl>)
					.Select(control => new Dictionary<string, object>
					{
						["contentId"] = control.Model?.ContentId,
						["title"] = control.Model?.Title,
						["isVisible"] = control.IsVisible,
						["actualWidth"] = control.ActualWidth,
						["actualHeight"] = control.ActualHeight,
						["bounds"] = CreateBoundsPayload("anchorable-control", control.Model?.ContentId, control),
					})
					.ToArray();
				return System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
				{
					["titles"] = titles,
					["controls"] = controls,
				});
			}

			private FrameworkElement FindAnchorablePaneResizer(string contentId)
		{
			var pane = FindVisualDescendant<LayoutAnchorablePaneControl>(
				dockManager,
				x => x.Model?.Descendents().OfType<LayoutAnchorable>()
					.Any(a => string.Equals(a.ContentId, contentId, StringComparison.Ordinal)) == true);
			if (pane == null)
			{
				return contentId == "dragTestTool"
					? FindVisualDescendants<LayoutGridResizerControl>(dockManager)
						.FirstOrDefault(r => r.IsVisible && r.ActualWidth > 0 && r.ActualHeight > 0)
					: null;
			}

			var paneLeft = pane.PointToScreen(new Point(0, 0)).X;
			var resizers = FindVisualDescendants<LayoutGridResizerControl>(dockManager)
				.Select(r => new
				{
					Resizer = r,
					CenterX = r.PointToScreen(new Point(r.ActualWidth / 2d, r.ActualHeight / 2d)).X
				})
				.Where(x => x.CenterX <= paneLeft + 2d)
				.OrderByDescending(x => x.CenterX)
				.Select(x => x.Resizer)
				.FirstOrDefault();
			return resizers ?? FindVisualDescendants<LayoutGridResizerControl>(dockManager)
				.FirstOrDefault(r => r.IsVisible && r.ActualWidth > 0 && r.ActualHeight > 0);
		}

		private static bool IsHitTestableAtCenter(FrameworkElement element)
		{
			if (element == null || !element.IsVisible || element.ActualWidth <= 0 || element.ActualHeight <= 0)
				return false;

			var center = new Point(element.ActualWidth / 2d, element.ActualHeight / 2d);
			var root = FindVisualRoot(element);
			if (root is not UIElement rootElement || root is not Visual rootVisual)
				return false;

			var pointInRoot = element.TransformToAncestor(rootVisual).Transform(center);
			var hit = rootElement.InputHitTest(pointInRoot) as DependencyObject;
			for (var current = hit; current != null; current = VisualTreeHelper.GetParent(current))
			{
				if (ReferenceEquals(current, element))
					return true;
			}

			return false;
		}

		private static DependencyObject FindVisualRoot(DependencyObject element)
		{
			var current = element;
			while (VisualTreeHelper.GetParent(current) is { } parent)
				current = parent;
			return current;
		}

		private static Dictionary<string, object> CreateBoundsPayload(string target, string contentId, FrameworkElement element)
		{
			var result = new Dictionary<string, object>
			{
				["target"] = target,
				["contentId"] = contentId,
				["found"] = element != null,
			};

			if (element == null)
				return result;

				var topLeft = PointToScreenPortable(element, new Point(0, 0));
				var bottomRight = PointToScreenPortable(element, new Point(element.ActualWidth, element.ActualHeight));
			result["x"] = topLeft.X;
			result["y"] = topLeft.Y;
			result["width"] = bottomRight.X - topLeft.X;
			result["height"] = bottomRight.Y - topLeft.Y;
			if (TryFindHitTestableScreenPoint(element, out var hitPoint))
			{
				result["centerX"] = hitPoint.X;
				result["centerY"] = hitPoint.Y;
				result["hitTestPoint"] = true;
			}
			else
			{
				result["centerX"] = topLeft.X + (bottomRight.X - topLeft.X) / 2d;
				result["centerY"] = topLeft.Y + (bottomRight.Y - topLeft.Y) / 2d;
				result["hitTestPoint"] = false;
			}
			return result;
		}

			private static bool TryFindHitTestableScreenPoint(FrameworkElement element, out Point screenPoint)
		{
			screenPoint = default;
			var root = FindVisualRoot(element);
			if (root is not UIElement rootElement || root is not Visual rootVisual)
				return false;

			var xs = new[] { 0.5, 0.25, 0.75, 0.1, 0.9 };
			var ys = new[] { 0.5, 0.25, 0.75, 0.1, 0.9 };
			foreach (var yRatio in ys)
			{
				foreach (var xRatio in xs)
				{
					var local = new Point(element.ActualWidth * xRatio, element.ActualHeight * yRatio);
					var pointInRoot = element.TransformToAncestor(rootVisual).Transform(local);
					var hit = rootElement.InputHitTest(pointInRoot) as DependencyObject;
					for (var current = hit; current != null; current = VisualTreeHelper.GetParent(current))
					{
						if (!ReferenceEquals(current, element))
							continue;

						if (rootElement is FrameworkElement rootFrameworkElement)
						{
							screenPoint = PointToScreenPortable(rootFrameworkElement, pointInRoot);
							return true;
						}
					}
				}
			}

				return false;
			}

			private static Point PointToScreenPortable(FrameworkElement element, Point point)
			{
				try
				{
					return element.PointToScreen(point);
				}
				catch (InvalidOperationException)
				{
				}

				var window = Window.GetWindow(element);
				if (window is LayoutFloatingWindowControl && !ReferenceEquals(window, element))
				{
					try
					{
						var pointInWindow = element.TransformToAncestor(window).Transform(point);
						return window.PointToScreen(pointInWindow);
					}
					catch (InvalidOperationException)
					{
					}
				}

				if (element is Window directWindow)
					return new Point(directWindow.Left + point.X, directWindow.Top + point.Y);

				throw new InvalidOperationException("Could not convert element point to screen coordinates.");
			}

			private static Point PointFromScreenPortable(UIElement element, Point point)
			{
				try
				{
					return element.PointFromScreen(point);
				}
				catch (InvalidOperationException)
				{
				}

				if (element is FrameworkElement frameworkElement)
				{
					var window = Window.GetWindow(frameworkElement);
					if (window is LayoutFloatingWindowControl && !ReferenceEquals(window, frameworkElement))
					{
						try
						{
							var pointInWindow = window.PointFromScreen(point);
							return window.TransformToDescendant(frameworkElement).Transform(pointInWindow);
						}
						catch (InvalidOperationException)
						{
						}
					}
				}

				if (element is Window directWindow)
					return new Point(point.X - directWindow.Left, point.Y - directWindow.Top);

				throw new InvalidOperationException("Could not convert screen point to element coordinates.");
			}

		// Deepest FrameworkElement whose own on-screen rect contains the screen point, across the whole
		// subtree. Uses each element's own PointToScreen (the transform path that is reliable in this
		// rendering backend) rather than TransformToDescendant. Preferring the DEEPEST match (rather
		// than the topmost z-order sibling) is deliberate: full-size but content-empty overlays such as
		// LayoutAutoHideWindowControl sit on top of the real panes and contain every point shallowly,
		// so a topmost-first walk would always resolve to the overlay and never the pane/resizer the
		// caller is actually verifying. Returns null when nothing qualifies.
		private static FrameworkElement FindDeepestVisualContaining(DependencyObject node, Point screenPoint)
		{
			return FindDeepestVisualContaining(node, screenPoint, 0).Element;
		}

		private static (FrameworkElement Element, int Depth) FindDeepestVisualContaining(
			DependencyObject node, Point screenPoint, int depth)
		{
			(FrameworkElement Element, int Depth) best = (null, -1);
			var count = VisualTreeHelper.GetChildrenCount(node);
			for (var i = 0; i < count; i++)
			{
				var childBest = FindDeepestVisualContaining(VisualTreeHelper.GetChild(node, i), screenPoint, depth + 1);
				if (childBest.Element != null && childBest.Depth > best.Depth)
					best = childBest;
			}

			if (best.Element != null)
				return best;

			if (node is FrameworkElement fe && fe.ActualWidth > 0 && fe.ActualHeight > 0)
			{
				try
				{
					var topLeft = PointToScreenPortable(fe, new Point(0, 0));
					var bottomRight = PointToScreenPortable(fe, new Point(fe.ActualWidth, fe.ActualHeight));
					if (new Rect(topLeft, bottomRight).Contains(screenPoint))
						return (fe, depth);
				}
				catch (InvalidOperationException)
				{
				}
			}

			return (null, -1);
		}

		private static T FindVisualDescendant<T>(DependencyObject root, Func<T, bool> predicate)
			where T : DependencyObject
		{
			if (root is T current && predicate(current))
				return current;

			var count = VisualTreeHelper.GetChildrenCount(root);
			for (var i = 0; i < count; i++)
			{
				var found = FindVisualDescendant(VisualTreeHelper.GetChild(root, i), predicate);
				if (found != null)
					return found;
			}

			return null;
		}

		private static IEnumerable<T> FindVisualDescendants<T>(DependencyObject root)
			where T : DependencyObject
		{
			if (root is T current)
				yield return current;

			var count = VisualTreeHelper.GetChildrenCount(root);
			for (var i = 0; i < count; i++)
			{
				foreach (var found in FindVisualDescendants<T>(VisualTreeHelper.GetChild(root, i)))
					yield return found;
			}
		}

		private IEnumerable<DependencyObject> GetAvalonDockVisualRoots()
		{
			yield return dockManager;
			foreach (var floatingWindow in dockManager.FloatingWindows)
				yield return floatingWindow;
		}

		// Drag handle for a single-content floating anchorable's caption, which is a
		// DropDownControlArea in the floating window's own template rather than an
		// AnchorablePaneTitle (that control only exists for multi-tab docked panes).
		private FrameworkElement FindFloatingWindowCaption(string contentId)
		{
			var floating = FindVisibleFloatingWindow(contentId) as LayoutAnchorableFloatingWindowControl;
			if (floating == null)
				return null;

			return FindVisualDescendants<DropDownControlArea>(floating)
				.FirstOrDefault(x => x.IsVisible && x.ActualWidth > 0 && x.ActualHeight > 0);
		}

			private FrameworkElement FindAnchorableTitle(string contentId)
			{
				return FindFloatingWindowCaption(contentId) ??
					GetAvalonDockVisualRoots()
						.SelectMany(FindVisualDescendants<AnchorablePaneTitle>)
					.Where(x => string.Equals(x.Model?.ContentId, contentId, StringComparison.Ordinal) ||
						x.FindVisualAncestor<LayoutAnchorablePaneControl>()?.Model?.Descendents().OfType<LayoutAnchorable>()
						.Any(a => string.Equals(a.ContentId, contentId, StringComparison.Ordinal)) == true)
						.FirstOrDefault(x => x.IsVisible && x.ActualWidth > 0 && x.ActualHeight > 0);
			}

				private FrameworkElement FindDockedAnchorableDragHandle(string contentId)
				{
					var tab = FindVisualDescendants<LayoutAnchorableTabItem>(dockManager)
						.Where(x => MatchesAnchorableContent(x, contentId))
						.FirstOrDefault(x => x.IsVisible && x.ActualWidth > 0 && x.ActualHeight > 0);
					if (tab != null)
						return tab;

					var title = FindVisualDescendants<AnchorablePaneTitle>(dockManager)
						.Where(x => string.Equals(x.Model?.ContentId, contentId, StringComparison.Ordinal) ||
							x.FindVisualAncestor<LayoutAnchorablePaneControl>()?.Model?.Descendents().OfType<LayoutAnchorable>()
								.Any(a => string.Equals(a.ContentId, contentId, StringComparison.Ordinal)) == true)
						.FirstOrDefault(x => x.IsVisible && x.ActualWidth > 0 && x.ActualHeight > 0);
					if (title != null)
						return title;

					return FindVisualDescendants<LayoutAnchorableTabItem>(dockManager)
						.Where(x => MatchesAnchorableContent(x, contentId))
						.FirstOrDefault(x => x.IsVisible && x.ActualWidth > 0 && x.ActualHeight > 0);
				}

				private static bool MatchesAnchorableContent(LayoutAnchorableTabItem tab, string contentId)
				{
					return string.Equals((tab.Model as LayoutAnchorable)?.ContentId, contentId, StringComparison.Ordinal) ||
						tab.FindVisualAncestor<LayoutAnchorablePaneControl>()?.Model?.Descendents().OfType<LayoutAnchorable>()
							.Any(a => string.Equals(a.ContentId, contentId, StringComparison.Ordinal)) == true;
				}

			private LayoutFloatingWindowControl FindVisibleFloatingWindow(string contentId)
		{
			return dockManager.FloatingWindows
				.Where(fw => fw.IsLoaded && fw.IsVisible &&
					fw.Model?.Descendents().OfType<LayoutAnchorable>()
						.Any(a => string.Equals(a.ContentId, contentId, StringComparison.Ordinal)) == true)
				.LastOrDefault();
		}

		[DevFlowAction("avd.new-floating", Description = "Create a new floating anchorable window")]
		public string NewFloatingWindow(string title = null)
		{
			var anchorable = new LayoutAnchorable
			{
				Title = title ?? "Floating window",
				ContentId = $"float-{Guid.NewGuid():N}",
				Content = new TestUserControl()
			};
			anchorable.AddToLayout(dockManager, AnchorableShowStrategy.Most);
			anchorable.Float();
			return $"Created floating '{anchorable.ContentId}'";
		}

		private static bool TryGetPlatformWindowZOrder(IntPtr hwnd, out int zOrder)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return TryGetMacWindowOrder(hwnd, out zOrder);

			return TryGetWindowZOrder(hwnd, out zOrder);
		}

		private static bool IsPlatformZOrderAbove(int candidateZOrder, int referenceZOrder)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return candidateZOrder < referenceZOrder;

			return candidateZOrder > referenceZOrder;
		}

		private static bool TryGetMacWindowOrder(IntPtr window, out int zOrder)
		{
			try
			{
				var app = ObjCMsgSend(_nsApplicationClass, _selSharedApplication);
				var windows = ObjCMsgSend(app, _selOrderedWindows);
				var count = ObjCMsgSendRetNUInt(windows, _selCount);
				for (nuint i = 0; i < count; i++)
				{
					if (ObjCMsgSendNUInt(windows, _selObjectAtIndex, i) == window)
					{
						zOrder = checked((int)i);
						return true;
					}
				}
			}
			catch
			{
			}

			zOrder = int.MinValue;
			return false;
		}

		private static bool TryGetWindowZOrder(IntPtr hwnd, out int zOrder)
		{
			var lowestHwnd = GetWindow(hwnd, (uint)GetWindowCmd.GW_HWNDLAST);
			var z = 0;
			var current = lowestHwnd;
			while (current != IntPtr.Zero)
			{
				if (current == hwnd)
				{
					zOrder = z;
					return true;
				}

				current = GetWindow(current, (uint)GetWindowCmd.GW_HWNDPREV);
				z++;
			}

			zOrder = int.MinValue;
			return false;
		}

		[DevFlowAction("avd.close-document", Description = "Close a document by ContentId")]
		public string CloseDocument(string contentId)
		{
			var doc = dockManager.Layout.Descendents()
				.OfType<LayoutDocument>()
				.FirstOrDefault(d => d.ContentId == contentId);
			if (doc == null)
				return $"Document '{contentId}' not found";
			doc.Close();
			return $"Closed '{contentId}'";
		}
    }
}
