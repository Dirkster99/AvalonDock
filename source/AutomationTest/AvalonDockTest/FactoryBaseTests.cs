#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvalonDock.Core;
using NUnit.Framework;

namespace AvalonDockTest
{
	#region Test Helpers

	internal class TestFactory : FactoryBase
	{
		public List<(IDockable Dockable, IDock Source, IDock Target)> MovedItems { get; } = new();
		public List<IDockable> ClosedItems { get; } = new();
		public List<IDockable> PinnedItems { get; } = new();
		public List<IDockable> FloatedItems { get; } = new();

		public override IRootDock CreateRootDock() => new TestRootDock();
		public override IDocumentDock CreateDocumentDock() => new TestDocumentDock();
		public override IToolDock CreateToolDock() => new TestToolDock();
		public override IList<T> CreateList<T>(params T[] items) => new ObservableCollection<T>(items);
		public override IRootDock CreateLayout() => CreateRootDock();

		protected override void OnDockableMoved(IDockable dockable, IDock source, IDock target)
			=> MovedItems.Add((dockable, source, target));

		protected override void OnDockableClosed(IDockable dockable)
			=> ClosedItems.Add(dockable);

		protected override void OnDockablePinned(IDockable dockable)
			=> PinnedItems.Add(dockable);

		protected override void OnDockableFloated(IDockable dockable)
			=> FloatedItems.Add(dockable);
	}

	internal class TestDockable : IDockable
	{
		public string Id { get; set; } = "";
		public string Title { get; set; } = "";
		public object? Context { get; set; }
		public IDockable? Owner { get; set; }
		public IFactory? Factory { get; set; }
		public bool CanClose { get; set; } = true;
		public bool CanPin { get; set; } = true;
		public bool CanFloat { get; set; } = true;
		public bool CanDrag { get; set; } = true;
		public bool CanDrop { get; set; } = true;
		public bool IsModified { get; set; }
		public bool IsActive { get; set; }
		public DockState DockState { get; set; }

		public bool AllowClose { get; set; } = true;
		public bool OnClose() => AllowClose;
		public void OnSelected() { }
	}

	internal class TestDock : TestDockable, IDock
	{
		public IList<IDockable>? VisibleDockables { get; set; } = new ObservableCollection<IDockable>();
		public IDockable? ActiveDockable { get; set; }
		public IDockable? DefaultDockable { get; set; }
		public IDockable? FocusedDockable { get; set; }
		public bool CanGoBack => false;
		public bool CanGoForward => false;
	}

	internal class TestRootDock : TestDock, IRootDock
	{
		public IList<IDockable>? FloatingDockables { get; set; }
		public IList<IDockable>? PinnedDockables { get; set; }
		public IDockable? DefaultLayout { get; set; }
		public void ShowWindows() { }
		public void HideWindows() { }
	}

	internal class TestDocumentDock : TestDock, IDocumentDock
	{
		public bool CanCreateDocument { get; set; } = true;
	}

	internal class TestToolDock : TestDock, IToolDock
	{
		public DockAlignment Alignment { get; set; }
		public bool IsExpanded { get; set; }
		public bool AutoHide { get; set; }
	}

	#endregion

	[TestFixture]
	public class FactoryBaseTests
	{
		[Test]
		public void MoveDockable_RemovesFromSourceAndAddsToTarget()
		{
			var factory = new TestFactory();
			var source = new TestDock();
			var target = new TestDock();
			var dockable = new TestDockable { Id = "test" };

			source.VisibleDockables!.Add(dockable);
			factory.MoveDockable(source, target, dockable);

			Assert.That(source.VisibleDockables, Does.Not.Contain(dockable));
			Assert.That(target.VisibleDockables, Does.Contain(dockable));
		}

		[Test]
		public void MoveDockable_SetsOwnerToTarget()
		{
			var factory = new TestFactory();
			var source = new TestDock();
			var target = new TestDock();
			var dockable = new TestDockable { Owner = source };

			source.VisibleDockables!.Add(dockable);
			factory.MoveDockable(source, target, dockable);

			Assert.That(dockable.Owner, Is.SameAs(target));
		}

		[Test]
		public void MoveDockable_SetsActiveDockableOnTarget()
		{
			var factory = new TestFactory();
			var source = new TestDock();
			var target = new TestDock();
			var dockable = new TestDockable();

			source.VisibleDockables!.Add(dockable);
			factory.MoveDockable(source, target, dockable);

			Assert.That(target.ActiveDockable, Is.SameAs(dockable));
		}

		[Test]
		public void MoveDockable_RaisesEvent()
		{
			var factory = new TestFactory();
			var source = new TestDock();
			var target = new TestDock();
			var dockable = new TestDockable();

			source.VisibleDockables!.Add(dockable);
			factory.MoveDockable(source, target, dockable);

			Assert.That(factory.MovedItems, Has.Count.EqualTo(1));
			Assert.That(factory.MovedItems[0].Dockable, Is.SameAs(dockable));
		}

		[Test]
		public void CloseDockable_RemovesFromOwner()
		{
			var factory = new TestFactory();
			var dock = new TestDock();
			var dockable = new TestDockable { Owner = dock };

			dock.VisibleDockables!.Add(dockable);
			factory.CloseDockable(dockable);

			Assert.That(dock.VisibleDockables, Does.Not.Contain(dockable));
		}

		[Test]
		public void CloseDockable_CancelsIfOnCloseReturnsFalse()
		{
			var factory = new TestFactory();
			var dock = new TestDock();
			var dockable = new TestDockable { Owner = dock, AllowClose = false };

			dock.VisibleDockables!.Add(dockable);
			factory.CloseDockable(dockable);

			Assert.That(dock.VisibleDockables, Does.Contain(dockable));
			Assert.That(factory.ClosedItems, Is.Empty);
		}

		[Test]
		public void CloseDockable_ResetsActiveDockableToDefault()
		{
			var factory = new TestFactory();
			var dock = new TestDock();
			var defaultDockable = new TestDockable { Id = "default" };
			var dockable = new TestDockable { Owner = dock };

			dock.DefaultDockable = defaultDockable;
			dock.ActiveDockable = dockable;
			dock.VisibleDockables!.Add(dockable);

			factory.CloseDockable(dockable);

			Assert.That(dock.ActiveDockable, Is.SameAs(defaultDockable));
		}

		[Test]
		public void CloseDockable_RaisesEvent()
		{
			var factory = new TestFactory();
			var dock = new TestDock();
			var dockable = new TestDockable { Owner = dock };

			dock.VisibleDockables!.Add(dockable);
			factory.CloseDockable(dockable);

			Assert.That(factory.ClosedItems, Has.Count.EqualTo(1));
			Assert.That(factory.ClosedItems[0], Is.SameAs(dockable));
		}

		[Test]
		public void PinDockable_RaisesEvent()
		{
			var factory = new TestFactory();
			var dockable = new TestDockable();

			factory.PinDockable(dockable);

			Assert.That(factory.PinnedItems, Has.Count.EqualTo(1));
		}

		[Test]
		public void FloatDockable_RaisesEvent()
		{
			var factory = new TestFactory();
			var dockable = new TestDockable();

			factory.FloatDockable(dockable);

			Assert.That(factory.FloatedItems, Has.Count.EqualTo(1));
		}

		[Test]
		public void InitLayout_SetsFactoryOnDockable()
		{
			var factory = new TestFactory();
			var dockable = new TestDockable { Id = "root" };

			factory.InitLayout(dockable);

			Assert.That(dockable.Factory, Is.SameAs(factory));
		}

		[Test]
		public void InitLayout_SetsContextFromLocator()
		{
			var factory = new TestFactory();
			var context = new object();
			factory.ContextLocator = new Dictionary<string, Func<object?>>
			{
				["myId"] = () => context
			};

			var dockable = new TestDockable { Id = "myId" };
			factory.InitLayout(dockable);

			Assert.That(dockable.Context, Is.SameAs(context));
		}

		[Test]
		public void InitLayout_InitializesChildDockables()
		{
			var factory = new TestFactory();
			var child = new TestDockable { Id = "child" };
			var dock = new TestDock { Id = "parent" };
			dock.VisibleDockables!.Add(child);

			factory.InitLayout(dock);

			Assert.That(child.Factory, Is.SameAs(factory));
			Assert.That(child.Owner, Is.SameAs(dock));
		}

		[Test]
		public void CreateList_ReturnsObservableCollection()
		{
			var factory = new TestFactory();
			var list = factory.CreateList("a", "b", "c");

			Assert.That(list, Is.InstanceOf<ObservableCollection<string>>());
			Assert.That(list.Count, Is.EqualTo(3));
		}
	}
}
