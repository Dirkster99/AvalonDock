#nullable enable
using System.Collections.Generic;
using System.Linq;
using AvalonDock.Core;
using AvalonDock.Core.Events;
using AvalonDock.Core.Serialization;
using AvalonDock.DependencyInjection;
using AvalonDock.Mvvm;
using AvalonDock.Serializer.Json;
using AvalonDock.Serializer.Xml;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AvalonDockTest
{
	internal class DiTestToolbox : ToolboxBase
	{
		public DiTestToolbox()
		{
			Id = "di-test";
			Title = "DI Test";
			Zone = DockZone.LeftTop;
		}
	}

	internal class DiTestToolboxWithDeps : ToolboxBase
	{
		public string Dependency { get; }

		public DiTestToolboxWithDeps(string dependency)
		{
			Id = "di-deps";
			Title = "With Deps";
			Dependency = dependency;
		}
	}

	[TestFixture]
	public class ServiceCollectionExtensionsTests
	{
		[Test]
		public void AddDockLayoutService_Builder_AddToolbox_RegistersSingleton()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolbox>();
			});

			var provider = services.BuildServiceProvider();
			var instance1 = provider.GetRequiredService<DiTestToolbox>();
			var instance2 = provider.GetRequiredService<DiTestToolbox>();

			Assert.That(instance1, Is.Not.Null);
			Assert.That(instance1, Is.SameAs(instance2));
		}

		[Test]
		public void AddDockLayoutService_Builder_AddToolbox_WithFactory_RegistersSingleton()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolboxWithDeps>(sp =>
					new DiTestToolboxWithDeps("injected-value"));
			});

			var provider = services.BuildServiceProvider();
			var instance = provider.GetRequiredService<DiTestToolboxWithDeps>();

			Assert.That(instance, Is.Not.Null);
			Assert.That(instance.Dependency, Is.EqualTo("injected-value"));
		}

		[Test]
		public void AddDockLayoutService_Builder_AddToolbox_WithFactory_IsSingleton()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolboxWithDeps>(sp =>
					new DiTestToolboxWithDeps("test"));
			});

			var provider = services.BuildServiceProvider();
			var instance1 = provider.GetRequiredService<DiTestToolboxWithDeps>();
			var instance2 = provider.GetRequiredService<DiTestToolboxWithDeps>();

			Assert.That(instance1, Is.SameAs(instance2));
		}

		[Test]
		public void AddDockLayoutService_Builder_AddToolbox_CanBeResolvedAsIToolbox()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolbox>();
			});

			var provider = services.BuildServiceProvider();
			var toolboxes = provider.GetServices<IToolbox>().ToList();

			Assert.That(toolboxes, Has.Count.EqualTo(1));
			Assert.That(toolboxes[0], Is.InstanceOf<DiTestToolbox>());
		}

		[Test]
		public void AddDockLayoutService_Builder_MultipleToolboxes_AllResolvable()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolbox>();
				dock.AddToolbox<DiTestToolboxWithDeps>(sp => new DiTestToolboxWithDeps("v"));
			});

			var provider = services.BuildServiceProvider();
			var toolboxes = provider.GetServices<IToolbox>().ToList();

			Assert.That(toolboxes, Has.Count.EqualTo(2));
		}

		[Test]
		public void AddDockLayoutService_Builder_ConfigureToggleDock_RegistersOptions()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.ConfigureToggleDock(opts =>
				{
					opts.ButtonSize = 32;
					opts.DefaultDockWidth = 300;
					opts.DefaultDockHeight = 200;
				});
			});

			var provider = services.BuildServiceProvider();
			var options = provider.GetRequiredService<ToggleDockOptions>();

			Assert.That(options.ButtonSize, Is.EqualTo(32));
			Assert.That(options.DefaultDockWidth, Is.EqualTo(300));
			Assert.That(options.DefaultDockHeight, Is.EqualTo(200));
		}

		[Test]
		public void AddDockLayoutService_Builder_WithoutConfigureToggleDock_RegistersDefaults()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolbox>();
			});

			var provider = services.BuildServiceProvider();
			var options = provider.GetRequiredService<ToggleDockOptions>();

			Assert.That(options, Is.Not.Null);
			Assert.That(options.ButtonSize, Is.EqualTo(25));
			Assert.That(options.DefaultDockWidth, Is.EqualTo(250));
			Assert.That(options.DefaultDockHeight, Is.EqualTo(200));
		}

		[Test]
		public void AddDockLayoutService_Builder_ReturnsSameServiceCollection_ForChaining()
		{
			var services = new ServiceCollection();
			var result = services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolbox>();
			});
			Assert.That(result, Is.SameAs(services));
		}

		[Test]
		public void AddDockLayoutService_Builder_RegistersLayoutServiceAndSideToggleManager()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolbox>();
			});

			var provider = services.BuildServiceProvider();
			var layoutSvc = provider.GetRequiredService<IDockLayoutService>();
			var sideToggle = provider.GetRequiredService<SideToggleManager>();

			Assert.That(layoutSvc, Is.Not.Null);
			Assert.That(sideToggle, Is.Not.Null);
		}

		[Test]
		public void AddDockLayoutService_Builder_CollectsToolboxes()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolbox>();
				dock.AddToolbox<DiTestToolboxWithDeps>(sp => new DiTestToolboxWithDeps("val"));
			});

			var provider = services.BuildServiceProvider();
			var svc = provider.GetRequiredService<IDockLayoutService>();

			Assert.That(svc.Anchorables.Count(), Is.EqualTo(2));
		}

		[Test]
		public void AddAvalonDockSerializer_XmlLayoutSerializer_Resolves()
		{
			var services = new ServiceCollection();
			var fakeDm = new FakeDockingManager();
			services.AddDockingManager(sp => fakeDm);
			services.AddAvalonDockSerializer<XmlLayoutSerializer>();

			var provider = services.BuildServiceProvider();
			var serializer = provider.GetRequiredService<ILayoutSerializer>();

			Assert.That(serializer, Is.Not.Null);
			Assert.That(serializer, Is.InstanceOf<XmlLayoutSerializer>());
		}

		[Test]
		public void AddAvalonDockSerializer_JsonLayoutSerializer_Resolves()
		{
			var services = new ServiceCollection();
			var fakeDm = new FakeDockingManager();
			services.AddDockingManager(sp => fakeDm);
			services.AddAvalonDockSerializer<JsonLayoutSerializer>();

			var provider = services.BuildServiceProvider();
			var serializer = provider.GetRequiredService<ILayoutSerializer>();

			Assert.That(serializer, Is.Not.Null);
			Assert.That(serializer, Is.InstanceOf<JsonLayoutSerializer>());
		}

		[Test]
		public void AddAvalonDockSerializer_IsSingleton()
		{
			var services = new ServiceCollection();
			var fakeDm = new FakeDockingManager();
			services.AddDockingManager(sp => fakeDm);
			services.AddAvalonDockSerializer<XmlLayoutSerializer>();

			var provider = services.BuildServiceProvider();
			var s1 = provider.GetRequiredService<ILayoutSerializer>();
			var s2 = provider.GetRequiredService<ILayoutSerializer>();

			Assert.That(s1, Is.SameAs(s2));
		}

		[Test]
		public void AddAvalonDockSerializer_ReturnsSameServiceCollection_ForChaining()
		{
			var services = new ServiceCollection();
			var result = services.AddAvalonDockSerializer<XmlLayoutSerializer>();
			Assert.That(result, Is.SameAs(services));
		}
	}

	internal class TestThemeManager : IThemeManager
	{
		private readonly List<IThemeInfo> _themes = new List<IThemeInfo>();
		public string? CurrentThemeName { get; private set; }
		public IReadOnlyList<IThemeInfo> AvailableThemes => _themes.AsReadOnly();
		public event System.EventHandler? ThemeChanged;

		public bool ApplyTheme(string themeName)
		{
			CurrentThemeName = themeName;
			ThemeChanged?.Invoke(this, System.EventArgs.Empty);
			return true;
		}
	}

	internal class TestAutoHideManager : IAutoHideManager
	{
		public int AutoHideDelayMs { get; set; } = 1000;
		public bool IsAutoHideVisible { get; private set; }
		public string? CurrentAutoHideContentId { get; private set; }

		public void ShowAutoHide(string contentId)
		{
			CurrentAutoHideContentId = contentId;
			IsAutoHideVisible = true;
		}

		public void HideAutoHide()
		{
			CurrentAutoHideContentId = null;
			IsAutoHideVisible = false;
		}
	}

	internal class TestFloatingWindowService : IFloatingWindowService
	{
		public int FloatingWindowCount { get; private set; }
		public event System.EventHandler? FloatingWindowCreated;
		public event System.EventHandler? FloatingWindowClosed;

		public bool Float(string contentId)
		{
			FloatingWindowCount++;
			FloatingWindowCreated?.Invoke(this, System.EventArgs.Empty);
			return true;
		}

		public void CloseAllFloatingWindows()
		{
			FloatingWindowCount = 0;
			FloatingWindowClosed?.Invoke(this, System.EventArgs.Empty);
		}
	}

	internal class TestDragDropHandler : IDragDropHandler
	{
		public bool IsDragDropEnabled { get; set; } = true;

		public bool CanDrop(string contentId, string targetContentId, DockPosition position)
		{
			return IsDragDropEnabled && position != DockPosition.None;
		}
	}

	[TestFixture]
	public class CoreInterfaceTests
	{
		[Test]
		[Category("Unit")]
		public void AddThemeManager_RegistersSingleton()
		{
			var services = new ServiceCollection();
			services.AddAvalonDockThemeManager<TestThemeManager>();

			var provider = services.BuildServiceProvider();
			var tm1 = provider.GetRequiredService<IThemeManager>();
			var tm2 = provider.GetRequiredService<IThemeManager>();

			Assert.That(tm1, Is.Not.Null);
			Assert.That(tm1, Is.SameAs(tm2));
			Assert.That(tm1, Is.InstanceOf<TestThemeManager>());
		}

		[Test]
		[Category("Unit")]
		public void AddThemeManager_ApplyTheme_ChangesCurrentName()
		{
			var services = new ServiceCollection();
			services.AddAvalonDockThemeManager<TestThemeManager>();

			var provider = services.BuildServiceProvider();
			var tm = provider.GetRequiredService<IThemeManager>();

			Assert.That(tm.CurrentThemeName, Is.Null);
			tm.ApplyTheme("ArcDark");
			Assert.That(tm.CurrentThemeName, Is.EqualTo("ArcDark"));
		}

		[Test]
		[Category("Unit")]
		public void AddThemeManager_RaisesThemeChanged()
		{
			var services = new ServiceCollection();
			services.AddAvalonDockThemeManager<TestThemeManager>();

			var provider = services.BuildServiceProvider();
			var tm = provider.GetRequiredService<IThemeManager>();

			bool raised = false;
			tm.ThemeChanged += (s, e) => raised = true;
			tm.ApplyTheme("Test");

			Assert.That(raised, Is.True);
		}

		[Test]
		[Category("Unit")]
		public void AddAutoHideManager_RegistersSingleton()
		{
			var services = new ServiceCollection();
			services.AddAutoHideManager<TestAutoHideManager>();

			var provider = services.BuildServiceProvider();
			var ahm1 = provider.GetRequiredService<IAutoHideManager>();
			var ahm2 = provider.GetRequiredService<IAutoHideManager>();

			Assert.That(ahm1, Is.Not.Null);
			Assert.That(ahm1, Is.SameAs(ahm2));
		}

		[Test]
		[Category("Unit")]
		public void AutoHideManager_ShowAndHide()
		{
			var services = new ServiceCollection();
			services.AddAutoHideManager<TestAutoHideManager>();

			var provider = services.BuildServiceProvider();
			var ahm = provider.GetRequiredService<IAutoHideManager>();

			Assert.That(ahm.IsAutoHideVisible, Is.False);
			ahm.ShowAutoHide("explorer");
			Assert.That(ahm.IsAutoHideVisible, Is.True);
			Assert.That(ahm.CurrentAutoHideContentId, Is.EqualTo("explorer"));

			ahm.HideAutoHide();
			Assert.That(ahm.IsAutoHideVisible, Is.False);
			Assert.That(ahm.CurrentAutoHideContentId, Is.Null);
		}

		[Test]
		[Category("Unit")]
		public void AddFloatingWindowService_RegistersSingleton()
		{
			var services = new ServiceCollection();
			services.AddFloatingWindowService<TestFloatingWindowService>();

			var provider = services.BuildServiceProvider();
			var fws1 = provider.GetRequiredService<IFloatingWindowService>();
			var fws2 = provider.GetRequiredService<IFloatingWindowService>();

			Assert.That(fws1, Is.Not.Null);
			Assert.That(fws1, Is.SameAs(fws2));
		}

		[Test]
		[Category("Unit")]
		public void FloatingWindowService_FloatAndClose()
		{
			var services = new ServiceCollection();
			services.AddFloatingWindowService<TestFloatingWindowService>();

			var provider = services.BuildServiceProvider();
			var fws = provider.GetRequiredService<IFloatingWindowService>();

			Assert.That(fws.FloatingWindowCount, Is.EqualTo(0));
			fws.Float("doc1");
			Assert.That(fws.FloatingWindowCount, Is.EqualTo(1));
			fws.Float("doc2");
			Assert.That(fws.FloatingWindowCount, Is.EqualTo(2));

			fws.CloseAllFloatingWindows();
			Assert.That(fws.FloatingWindowCount, Is.EqualTo(0));
		}

		[Test]
		[Category("Unit")]
		public void AddDragDropHandler_RegistersSingleton()
		{
			var services = new ServiceCollection();
			services.AddDragDropHandler<TestDragDropHandler>();

			var provider = services.BuildServiceProvider();
			var ddh1 = provider.GetRequiredService<IDragDropHandler>();
			var ddh2 = provider.GetRequiredService<IDragDropHandler>();

			Assert.That(ddh1, Is.Not.Null);
			Assert.That(ddh1, Is.SameAs(ddh2));
		}

		[Test]
		[Category("Unit")]
		public void DragDropHandler_CanDrop_Rules()
		{
			var ddh = new TestDragDropHandler();

			Assert.That(ddh.CanDrop("doc1", "target1", DockPosition.Left), Is.True);
			Assert.That(ddh.CanDrop("doc1", "target1", DockPosition.None), Is.False);

			ddh.IsDragDropEnabled = false;
			Assert.That(ddh.CanDrop("doc1", "target1", DockPosition.Left), Is.False);
		}

		[Test]
		[Category("Unit")]
		public void AddDockingManager_WithFactory_RegistersSingleton()
		{
			var services = new ServiceCollection();
			var fakeDm = new FakeDockingManager();
			services.AddDockingManager(sp => fakeDm);

			var provider = services.BuildServiceProvider();
			var dm1 = provider.GetRequiredService<IDockingManager>();
			var dm2 = provider.GetRequiredService<IDockingManager>();

			Assert.That(dm1, Is.Not.Null);
			Assert.That(dm1, Is.SameAs(dm2));
			Assert.That(dm1, Is.SameAs(fakeDm));
		}

		[Test]
		[Category("Unit")]
		public void CoreEventArgs_CanBeCreated()
		{
			var args1 = new DocumentCancelEventArgs(new FakeDocument());
			Assert.That(args1.Document, Is.Not.Null);
			Assert.That(args1.Cancel, Is.False);

			var args2 = new AnchorableCancelEventArgs(new FakeAnchorable());
			Assert.That(args2.Anchorable, Is.Not.Null);
			Assert.That(args2.CloseInsteadOfHide, Is.False);

			var args3 = new ContentEventArgs(new FakeDocument());
			Assert.That(args3.Content, Is.Not.Null);
		}

		[Test]
		[Category("Unit")]
		public void AddDockLayoutService_RegistersSingleton()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolbox>();
			});

			var provider = services.BuildServiceProvider();
			var svc1 = provider.GetRequiredService<IDockLayoutService>();
			var svc2 = provider.GetRequiredService<IDockLayoutService>();

			Assert.That(svc1, Is.SameAs(svc2));
			Assert.That(svc1.Layout, Is.Not.Null);
		}

		[Test]
		[Category("Unit")]
		public void DockLayoutService_CollectsToolboxes()
		{
			var services = new ServiceCollection();
			services.AddDockLayoutService(dock =>
			{
				dock.AddToolbox<DiTestToolbox>();
			});

			var provider = services.BuildServiceProvider();
			var svc = provider.GetRequiredService<IDockLayoutService>();

			Assert.That(svc.Anchorables.Count(), Is.EqualTo(1));
			Assert.That(svc.Anchorables.First().Title, Is.EqualTo("DI Test"));
		}

		[Test]
		[Category("Unit")]
		public void DockLayoutService_OpenAndCloseDocument()
		{
			var svc = new DockLayoutService(System.Array.Empty<IToolbox>());
			var doc = new Document { Id = "doc1", Title = "Test Doc" };

			svc.OpenDocument(doc);
			Assert.That(svc.Documents.Count(), Is.EqualTo(1));
			Assert.That(svc.ActiveDockable, Is.SameAs(doc));

			svc.CloseDocument(doc);
			Assert.That(svc.Documents.Count(), Is.EqualTo(0));
			Assert.That(svc.ActiveDockable, Is.Null);
		}

		[Test]
		[Category("Unit")]
		public void DockLayoutService_OpenOrActivateDocument()
		{
			var svc = new DockLayoutService(System.Array.Empty<IToolbox>());
			var doc1 = new Document { Id = "doc1", Title = "Doc 1" };

			var result1 = svc.OpenOrActivateDocument<Document>(
				d => d.Id == "doc1", () => doc1);
			Assert.That(result1, Is.SameAs(doc1));
			Assert.That(svc.Documents.Count(), Is.EqualTo(1));

			// Call again — should return existing, not create new
			var result2 = svc.OpenOrActivateDocument<Document>(
				d => d.Id == "doc1", () => new Document { Id = "doc1b", Title = "Doc 1b" });
			Assert.That(result2, Is.SameAs(doc1));
			Assert.That(svc.Documents.Count(), Is.EqualTo(1));
		}

		[Test]
		[Category("Unit")]
		public void DockLayoutService_FindDocument()
		{
			var svc = new DockLayoutService(System.Array.Empty<IToolbox>());
			var doc = new Document { Id = "findme", Title = "Find Me" };
			svc.OpenDocument(doc);

			var found = svc.FindDocument<Document>(d => d.Id == "findme");
			Assert.That(found, Is.SameAs(doc));

			var notFound = svc.FindDocument<Document>(d => d.Id == "missing");
			Assert.That(notFound, Is.Null);
		}
	}

	internal class FakeDockingManager : IDockingManager
	{
		public IRootDock? DockLayout { get; set; }
		public object? ActiveContent { get; set; }
		public System.Collections.IEnumerable? DocumentsSource { get; set; }
		public System.Collections.IEnumerable? AnchorablesSource { get; set; }
		public int AutoHideDelay { get; set; }
		public bool AllowMixedOrientation { get; set; }
		public ISerializableLayoutRoot? Layout
		{
			get => null;
			set { }
		}

		ISerializableLayoutRoot ISerializableDockingManager.Layout
		{
			get => Layout!;
			set => Layout = value;
		}

		public bool SuspendDocumentsSourceBinding { get; set; }
		public bool SuspendAnchorablesSourceBinding { get; set; }
		public ILayoutDtoMapper DtoMapper => new AvalonDock.Serialization.LayoutDtoMapper();
		public event System.EventHandler? ActiveContentChanged;
		public event System.EventHandler? LayoutChanged;
		public event System.EventHandler? LayoutChanging;
		public event System.EventHandler<DocumentCancelEventArgs>? DocumentClosing;
		public event System.EventHandler<DocumentEventArgs>? DocumentClosed;
		public event System.EventHandler<AnchorableCancelEventArgs>? AnchorableClosing;
		public event System.EventHandler<AnchorableEventArgs>? AnchorableClosed;
		public event System.EventHandler<AnchorableCancelEventArgs>? AnchorableHiding;
		public event System.EventHandler<AnchorableEventArgs>? AnchorableHidden;
		public event System.EventHandler<ContentCancelEventArgs>? ContentFloating;
		public event System.EventHandler<ContentEventArgs>? ContentFloated;
		public event System.EventHandler<ContentCancelEventArgs>? ContentDocking;
		public event System.EventHandler<ContentEventArgs>? ContentDocked;

		// Suppress unused event warnings
		internal void SuppressWarnings()
		{
			ActiveContentChanged?.Invoke(this, System.EventArgs.Empty);
			LayoutChanged?.Invoke(this, System.EventArgs.Empty);
			LayoutChanging?.Invoke(this, System.EventArgs.Empty);
			DocumentClosing?.Invoke(this, null!);
			DocumentClosed?.Invoke(this, null!);
			AnchorableClosing?.Invoke(this, null!);
			AnchorableClosed?.Invoke(this, null!);
			AnchorableHiding?.Invoke(this, null!);
			AnchorableHidden?.Invoke(this, null!);
			ContentFloating?.Invoke(this, null!);
			ContentFloated?.Invoke(this, null!);
			ContentDocking?.Invoke(this, null!);
			ContentDocked?.Invoke(this, null!);
		}
	}

	internal class FakeDocument : ISerializableLayoutDocument
	{
		public string ContentId { get; set; } = "fake-doc";
		public string Title { get; set; } = "Fake Doc";
		public object Content { get; set; } = new object();
		public object IconSource { get; set; } = null!;
		public void Close() { }
		public void FixCachedRootOnDeserialize() { }
	}

	internal class FakeAnchorable : ISerializableLayoutAnchorable
	{
		public string ContentId { get; set; } = "fake-anc";
		public string Title { get; set; } = "Fake Anchorable";
		public object Content { get; set; } = new object();
		public object IconSource { get; set; } = null!;
		public void Close() { }
		public void FixCachedRootOnDeserialize() { }
		public bool HideAnchorable(bool cancelable) => false;
	}
}
