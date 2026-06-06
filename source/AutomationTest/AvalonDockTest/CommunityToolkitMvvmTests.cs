#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AvalonDock.Core;
using AvalonDock.Mvvm;
using AvalonDock.Mvvm.CommunityToolkit;
using NUnit.Framework;

namespace AvalonDockTest;

/// <summary>
/// Tests for <see cref="ObservableDockableBase"/> verifying it implements
/// <see cref="IDockable"/> and supports property change notifications.
/// </summary>
[TestFixture]
public class ObservableDockableBaseTests
{
	private class TestDockable : ObservableDockableBase { }

	[Test]
	public void ImplementsIDockable()
	{
		var dockable = new TestDockable();
		Assert.That(dockable, Is.AssignableTo<IDockable>());
	}

	[Test]
	public void ImplementsINotifyPropertyChanged()
	{
		var dockable = new TestDockable();
		Assert.That(dockable, Is.AssignableTo<INotifyPropertyChanged>());
	}

	[Test]
	public void DefaultValues()
	{
		var dockable = new TestDockable();

		Assert.That(dockable.Id, Is.EqualTo(string.Empty));
		Assert.That(dockable.Title, Is.EqualTo(string.Empty));
		Assert.That(dockable.Context, Is.Null);
		Assert.That(dockable.Owner, Is.Null);
		Assert.That(dockable.Factory, Is.Null);
		Assert.That(dockable.CanClose, Is.True);
		Assert.That(dockable.CanPin, Is.True);
		Assert.That(dockable.CanFloat, Is.True);
		Assert.That(dockable.CanDrag, Is.True);
		Assert.That(dockable.CanDrop, Is.True);
		Assert.That(dockable.IsModified, Is.False);
		Assert.That(dockable.IsActive, Is.False);
		Assert.That(dockable.DockState, Is.EqualTo(DockState.Docked));
	}

	[Test]
	public void Title_RaisesPropertyChanged()
	{
		var dockable = new TestDockable();
		var changedProps = new List<string>();
		dockable.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName!);

		dockable.Title = "New Title";

		Assert.That(changedProps, Does.Contain("Title"));
		Assert.That(dockable.Title, Is.EqualTo("New Title"));
	}

	[Test]
	public void DoesNotRaisePropertyChanged_WhenValueUnchanged()
	{
		var dockable = new TestDockable();
		dockable.Title = "Same";

		var raised = false;
		dockable.PropertyChanged += (_, _) => raised = true;

		dockable.Title = "Same";

		Assert.That(raised, Is.False);
	}

	[Test]
	public void OnClose_DefaultReturnsTrue()
	{
		var dockable = new TestDockable();
		Assert.That(dockable.OnClose(), Is.True);
	}
}

/// <summary>
/// Tests for <see cref="ObservableToolboxBase"/> verifying it implements
/// <see cref="IToolbox"/> and supports property change notifications.
/// </summary>
[TestFixture]
public class ObservableToolboxBaseTests
{
	private class TestToolbox : ObservableToolboxBase
	{
		public TestToolbox()
		{
			Id = "test";
			Title = "Test Toolbox";
			Zone = DockZone.RightTop;
		}
	}

	[Test]
	public void ImplementsIToolbox()
	{
		var toolbox = new TestToolbox();
		Assert.That(toolbox, Is.AssignableTo<IToolbox>());
	}

	[Test]
	public void ImplementsIDockable()
	{
		var toolbox = new TestToolbox();
		Assert.That(toolbox, Is.AssignableTo<IDockable>());
	}

	[Test]
	public void SetsPropertiesFromConstructor()
	{
		var toolbox = new TestToolbox();

		Assert.That(toolbox.Id, Is.EqualTo("test"));
		Assert.That(toolbox.Title, Is.EqualTo("Test Toolbox"));
		Assert.That(toolbox.Zone, Is.EqualTo(DockZone.RightTop));
	}

	[Test]
	public void DefaultValues()
	{
		var toolbox = new TestToolbox();

		Assert.That(toolbox.IsOpenByDefault, Is.False);
		Assert.That(toolbox.IsOpen, Is.False);
		Assert.That(toolbox.Icon, Is.Null);
		Assert.That(toolbox.ToolTipText, Is.Null);
	}

	[Test]
	public void Zone_RaisesPropertyChanged()
	{
		var toolbox = new TestToolbox();
		var changed = false;
		toolbox.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ObservableToolboxBase.Zone))
				changed = true;
		};

		toolbox.Zone = DockZone.BottomLeft;

		Assert.That(changed, Is.True);
		Assert.That(toolbox.Zone, Is.EqualTo(DockZone.BottomLeft));
	}

	[Test]
	public void IsOpen_RaisesPropertyChanged()
	{
		var toolbox = new TestToolbox();
		var changed = false;
		toolbox.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ObservableToolboxBase.IsOpen))
				changed = true;
		};

		toolbox.IsOpen = true;

		Assert.That(changed, Is.True);
		Assert.That(toolbox.IsOpen, Is.True);
	}

	[Test]
	public void Icon_RaisesPropertyChanged()
	{
		var toolbox = new TestToolbox();
		var changed = false;
		toolbox.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ObservableToolboxBase.Icon))
				changed = true;
		};

		var icon = new object();
		toolbox.Icon = icon;

		Assert.That(changed, Is.True);
		Assert.That(toolbox.Icon, Is.SameAs(icon));
	}
}

/// <summary>
/// Tests for <see cref="ObservableDockBase"/> verifying it implements
/// <see cref="IDock"/> correctly.
/// </summary>
[TestFixture]
public class ObservableDockBaseTests
{
	private class TestDock : ObservableDockBase { }

	[Test]
	public void ImplementsIDock()
	{
		var dock = new TestDock();
		Assert.That(dock, Is.AssignableTo<IDock>());
	}

	[Test]
	public void DefaultValues()
	{
		var dock = new TestDock();

		Assert.That(dock.VisibleDockables, Is.Null);
		Assert.That(dock.ActiveDockable, Is.Null);
		Assert.That(dock.DefaultDockable, Is.Null);
		Assert.That(dock.FocusedDockable, Is.Null);
		Assert.That(dock.CanGoBack, Is.False);
		Assert.That(dock.CanGoForward, Is.False);
	}
}

/// <summary>
/// Tests for <see cref="ObservableDocument"/> and <see cref="ObservableTool"/>.
/// </summary>
[TestFixture]
public class ObservableLeafDockableTests
{
	[Test]
	public void ObservableDocument_ImplementsIDockable()
	{
		var doc = new ObservableDocument();
		Assert.That(doc, Is.AssignableTo<IDockable>());
	}

	[Test]
	public void ObservableTool_ImplementsIDockable()
	{
		var tool = new ObservableTool();
		Assert.That(tool, Is.AssignableTo<IDockable>());
	}

	[Test]
	public void ObservableDocument_Title_RaisesPropertyChanged()
	{
		var doc = new ObservableDocument();
		var changed = false;
		doc.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == "Title") changed = true;
		};

		doc.Title = "Doc1";

		Assert.That(changed, Is.True);
		Assert.That(doc.Title, Is.EqualTo("Doc1"));
	}
}

/// <summary>
/// Tests verifying that <see cref="DockableBase"/> (from AvalonDock.Mvvm,
/// without CommunityToolkit) still works correctly after removing the
/// ObservableObject base class.
/// </summary>
[TestFixture]
public class DockableBaseWithoutCommunityToolkitTests
{
	private class PlainDockable : DockableBase { }

	[Test]
	public void ImplementsINotifyPropertyChanged()
	{
		var d = new PlainDockable();
		Assert.That(d, Is.AssignableTo<INotifyPropertyChanged>());
	}

	[Test]
	public void ImplementsIDockable()
	{
		var d = new PlainDockable();
		Assert.That(d, Is.AssignableTo<IDockable>());
	}

	[Test]
	public void SetProperty_RaisesPropertyChanged()
	{
		var d = new PlainDockable();
		var props = new List<string>();
		d.PropertyChanged += (_, e) => props.Add(e.PropertyName!);

		d.Id = "x";
		d.Title = "t";
		d.CanClose = false;

		Assert.That(props, Is.EqualTo(new[] { "Id", "Title", "CanClose" }));
	}

	[Test]
	public void SetProperty_DoesNotRaise_WhenSameValue()
	{
		var d = new PlainDockable { Id = "same" };

		var raised = false;
		d.PropertyChanged += (_, _) => raised = true;

		d.Id = "same";
		Assert.That(raised, Is.False);
	}

	[Test]
	public void DefaultValues_MatchExpected()
	{
		var d = new PlainDockable();

		Assert.That(d.Id, Is.EqualTo(string.Empty));
		Assert.That(d.Title, Is.EqualTo(string.Empty));
		Assert.That(d.CanClose, Is.True);
		Assert.That(d.CanPin, Is.True);
		Assert.That(d.CanFloat, Is.True);
		Assert.That(d.DockState, Is.EqualTo(DockState.Docked));
	}
}

/// <summary>
/// Tests verifying that <see cref="ToolboxBase"/> (from AvalonDock.Mvvm,
/// without CommunityToolkit) still works correctly.
/// </summary>
[TestFixture]
public class ToolboxBaseWithoutCommunityToolkitTests
{
	private class PlainToolbox : ToolboxBase
	{
		public PlainToolbox()
		{
			Id = "plain";
			Title = "Plain";
			Zone = DockZone.LeftTop;
		}
	}

	[Test]
	public void ImplementsIToolbox()
	{
		var t = new PlainToolbox();
		Assert.That(t, Is.AssignableTo<IToolbox>());
	}

	[Test]
	public void Zone_RaisesPropertyChanged()
	{
		var t = new PlainToolbox();
		var changed = false;
		t.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ToolboxBase.Zone))
				changed = true;
		};

		t.Zone = DockZone.BottomRight;

		Assert.That(changed, Is.True);
		Assert.That(t.Zone, Is.EqualTo(DockZone.BottomRight));
	}

	[Test]
	public void IsOpen_RaisesPropertyChanged()
	{
		var t = new PlainToolbox();
		var changed = false;
		t.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(ToolboxBase.IsOpen))
				changed = true;
		};

		t.IsOpen = true;

		Assert.That(changed, Is.True);
	}
}

/// <summary>
/// Tests that <see cref="DockLayoutService"/> works correctly with both
/// <see cref="ToolboxBase"/> and <see cref="ObservableToolboxBase"/> toolboxes,
/// since both implement <see cref="IToolbox"/>.
/// </summary>
[TestFixture]
public class DockLayoutServiceMixedBaseClassTests
{
	private class PlainLeft : ToolboxBase
	{
		public PlainLeft() { Id = "plain-left"; Title = "PL"; Zone = DockZone.LeftTop; }
	}

	private class ObservableRight : ObservableToolboxBase
	{
		public ObservableRight() { Id = "obs-right"; Title = "OR"; Zone = DockZone.RightTop; }
	}

	[Test]
	public void MixedToolboxTypes_AreGroupedByAlignment()
	{
		var left = new PlainLeft();
		var right = new ObservableRight();
		var service = new DockLayoutService(new IToolbox[] { left, right });

		var toolDocks = service.Layout.VisibleDockables!
			.OfType<IToolDock>().ToList();

		Assert.That(toolDocks.Count, Is.EqualTo(2));
		Assert.That(toolDocks.Any(td => td.Alignment == DockAlignment.Left), Is.True);
		Assert.That(toolDocks.Any(td => td.Alignment == DockAlignment.Right), Is.True);
	}

	[Test]
	public void Anchorables_ContainsBothBaseTypes()
	{
		var left = new PlainLeft();
		var right = new ObservableRight();
		var service = new DockLayoutService(new IToolbox[] { left, right });

		var all = service.Anchorables.ToList();
		Assert.That(all, Does.Contain(left));
		Assert.That(all, Does.Contain(right));
	}

	[Test]
	public void GetAnchorable_FindsBothTypes()
	{
		var left = new PlainLeft();
		var right = new ObservableRight();
		var service = new DockLayoutService(new IToolbox[] { left, right });

		Assert.That(service.GetAnchorable<PlainLeft>(), Is.SameAs(left));
		Assert.That(service.GetAnchorable<ObservableRight>(), Is.SameAs(right));
	}
}