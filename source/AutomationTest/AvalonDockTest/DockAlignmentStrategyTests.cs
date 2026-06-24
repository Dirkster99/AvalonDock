#nullable enable
using System.Collections.Generic;
using System.Linq;
using AvalonDock;
using AvalonDock.Core;
using AvalonDock.Layout;
using AvalonDock.Mvvm;
using NUnit.Framework;

namespace AvalonDockTest;

/// <summary>
/// Tests for <see cref="DockAlignmentStrategy"/> which places anchorables
/// into the correct anchor side based on a content-to-side map.
/// </summary>
[TestFixture]
public class DockAlignmentStrategyTests
{
	private static LayoutRoot CreateLayoutRoot()
	{
		var root = new LayoutRoot();
		root.RootPanel = new LayoutPanel();
		return root;
	}

	[Test]
	public void BeforeInsertAnchorable_PlacesLeft_WhenMappedToLeft()
	{
		var content = new object();
		var map = new Dictionary<object, AnchorSide> { { content, AnchorSide.Left } };
		var strategy = new DockAlignmentStrategy(map);
		var layout = CreateLayoutRoot();
		var anchorable = new LayoutAnchorable { Content = content };

		bool handled = strategy.BeforeInsertAnchorable(layout, anchorable, null!);

		Assert.That(handled, Is.True);
		Assert.That(layout.LeftSide, Is.Not.Null);
		Assert.That(layout.LeftSide.Children.SelectMany(g => g.Children), Does.Contain(anchorable));
	}

	[Test]
	public void BeforeInsertAnchorable_PlacesRight_WhenMappedToRight()
	{
		var content = new object();
		var map = new Dictionary<object, AnchorSide> { { content, AnchorSide.Right } };
		var strategy = new DockAlignmentStrategy(map);
		var layout = CreateLayoutRoot();
		var anchorable = new LayoutAnchorable { Content = content };

		bool handled = strategy.BeforeInsertAnchorable(layout, anchorable, null!);

		Assert.That(handled, Is.True);
		Assert.That(layout.RightSide, Is.Not.Null);
		Assert.That(layout.RightSide.Children.SelectMany(g => g.Children), Does.Contain(anchorable));
	}

	[Test]
	public void BeforeInsertAnchorable_PlacesBottom_WhenMappedToBottom()
	{
		var content = new object();
		var map = new Dictionary<object, AnchorSide> { { content, AnchorSide.Bottom } };
		var strategy = new DockAlignmentStrategy(map);
		var layout = CreateLayoutRoot();
		var anchorable = new LayoutAnchorable { Content = content };

		bool handled = strategy.BeforeInsertAnchorable(layout, anchorable, null!);

		Assert.That(handled, Is.True);
		Assert.That(layout.BottomSide, Is.Not.Null);
		Assert.That(layout.BottomSide.Children.SelectMany(g => g.Children), Does.Contain(anchorable));
	}

	[Test]
	public void BeforeInsertAnchorable_PlacesTop_WhenMappedToTop()
	{
		var content = new object();
		var map = new Dictionary<object, AnchorSide> { { content, AnchorSide.Top } };
		var strategy = new DockAlignmentStrategy(map);
		var layout = CreateLayoutRoot();
		var anchorable = new LayoutAnchorable { Content = content };

		bool handled = strategy.BeforeInsertAnchorable(layout, anchorable, null!);

		Assert.That(handled, Is.True);
		Assert.That(layout.TopSide, Is.Not.Null);
		Assert.That(layout.TopSide.Children.SelectMany(g => g.Children), Does.Contain(anchorable));
	}

	[Test]
	public void BeforeInsertAnchorable_ReturnsFalse_WhenContentNotInMap()
	{
		var map = new Dictionary<object, AnchorSide>();
		var strategy = new DockAlignmentStrategy(map);
		var layout = CreateLayoutRoot();
		var anchorable = new LayoutAnchorable { Content = new object() };

		bool handled = strategy.BeforeInsertAnchorable(layout, anchorable, null!);

		Assert.That(handled, Is.False);
	}

	[Test]
	public void BeforeInsertAnchorable_ReturnsFalse_WhenContentIsNull()
	{
		var map = new Dictionary<object, AnchorSide>();
		var strategy = new DockAlignmentStrategy(map);
		var layout = CreateLayoutRoot();
		var anchorable = new LayoutAnchorable { Content = null };

		bool handled = strategy.BeforeInsertAnchorable(layout, anchorable, null!);

		Assert.That(handled, Is.False);
	}

	[Test]
	public void BeforeInsertAnchorable_ReusesExistingAnchorGroup()
	{
		var content1 = new object();
		var content2 = new object();
		var map = new Dictionary<object, AnchorSide>
		{
			{ content1, AnchorSide.Left },
			{ content2, AnchorSide.Left }
		};
		var strategy = new DockAlignmentStrategy(map);
		var layout = CreateLayoutRoot();
		var anc1 = new LayoutAnchorable { Content = content1 };
		var anc2 = new LayoutAnchorable { Content = content2 };

		strategy.BeforeInsertAnchorable(layout, anc1, null!);
		strategy.BeforeInsertAnchorable(layout, anc2, null!);

		Assert.That(layout.LeftSide.Children.Count, Is.EqualTo(1), "Should reuse existing group");
		Assert.That(layout.LeftSide.Children[0].Children.Count, Is.EqualTo(2));
	}

	[Test]
	public void BeforeInsertAnchorable_PlacesMultipleSidesCorrectly()
	{
		var leftContent = new object();
		var rightContent = new object();
		var bottomContent = new object();
		var map = new Dictionary<object, AnchorSide>
		{
			{ leftContent, AnchorSide.Left },
			{ rightContent, AnchorSide.Right },
			{ bottomContent, AnchorSide.Bottom }
		};
		var strategy = new DockAlignmentStrategy(map);
		var layout = CreateLayoutRoot();

		strategy.BeforeInsertAnchorable(layout, new LayoutAnchorable { Content = leftContent }, null!);
		strategy.BeforeInsertAnchorable(layout, new LayoutAnchorable { Content = rightContent }, null!);
		strategy.BeforeInsertAnchorable(layout, new LayoutAnchorable { Content = bottomContent }, null!);

		Assert.That(layout.LeftSide.Children.SelectMany(g => g.Children).Count(), Is.EqualTo(1));
		Assert.That(layout.RightSide.Children.SelectMany(g => g.Children).Count(), Is.EqualTo(1));
		Assert.That(layout.BottomSide.Children.SelectMany(g => g.Children).Count(), Is.EqualTo(1));
	}

	[Test]
	public void BeforeInsertDocument_ReturnsFalse()
	{
		var map = new Dictionary<object, AnchorSide>();
		var strategy = new DockAlignmentStrategy(map);
		var layout = CreateLayoutRoot();

		bool handled = strategy.BeforeInsertDocument(layout, new LayoutDocument(), null!);

		Assert.That(handled, Is.False);
	}
}

/// <summary>
/// Tests for <see cref="DockLayoutService"/> grouping toolboxes by alignment
/// so the <see cref="LayoutSyncBridge"/> can map them to the correct anchor side.
/// </summary>
[TestFixture]
public class DockLayoutServiceAlignmentTests
{
	private class LeftToolbox : ToolboxBase
	{
		public LeftToolbox() { Id = "left"; Title = "Left"; Zone = DockZone.LeftTop; }
	}

	private class LeftBottomToolbox : ToolboxBase
	{
		public LeftBottomToolbox() { Id = "left-bottom"; Title = "Left Bottom"; Zone = DockZone.LeftBottom; }
	}

	private class RightToolbox : ToolboxBase
	{
		public RightToolbox() { Id = "right"; Title = "Right"; Zone = DockZone.RightTop; }
	}

	private class BottomToolbox : ToolboxBase
	{
		public BottomToolbox() { Id = "bottom"; Title = "Bottom"; Zone = DockZone.BottomLeft; }
	}

	private class BottomRightToolbox : ToolboxBase
	{
		public BottomRightToolbox() { Id = "bottom-right"; Title = "Bottom Right"; Zone = DockZone.BottomRight; }
	}

	[Test]
	public void Layout_HasSeparateToolDocks_PerAlignment()
	{
		var left = new LeftToolbox();
		var right = new RightToolbox();
		var bottom = new BottomToolbox();
		var service = new DockLayoutService(new IToolbox[] { left, right, bottom });

		var root = service.Layout;
		var toolDocks = root.VisibleDockables!.OfType<IToolDock>().ToList();

		Assert.That(toolDocks.Count, Is.EqualTo(3), "Should have 3 tool docks (Left, Right, Bottom)");
		Assert.That(toolDocks.Any(td => td.Alignment == DockAlignment.Left), Is.True);
		Assert.That(toolDocks.Any(td => td.Alignment == DockAlignment.Right), Is.True);
		Assert.That(toolDocks.Any(td => td.Alignment == DockAlignment.Bottom), Is.True);
	}

	[Test]
	public void Layout_GroupsSameAlignmentToolboxes_IntoOneToolDock()
	{
		var bottom1 = new BottomToolbox();
		var bottom2 = new BottomRightToolbox();
		var service = new DockLayoutService(new IToolbox[] { bottom1, bottom2 });

		var root = service.Layout;
		var toolDocks = root.VisibleDockables!.OfType<IToolDock>().ToList();

		Assert.That(toolDocks.Count, Is.EqualTo(1), "Both bottom zones share the Bottom alignment");
		Assert.That(toolDocks[0].Alignment, Is.EqualTo(DockAlignment.Bottom));
		Assert.That(toolDocks[0].VisibleDockables!.Count, Is.EqualTo(2));
	}

	[Test]
	public void Layout_GroupsLeftZones_IntoOneToolDock()
	{
		var leftTop = new LeftToolbox();
		var leftBottom = new LeftBottomToolbox();
		var service = new DockLayoutService(new IToolbox[] { leftTop, leftBottom });

		var root = service.Layout;
		var toolDocks = root.VisibleDockables!.OfType<IToolDock>().ToList();

		Assert.That(toolDocks.Count, Is.EqualTo(1));
		Assert.That(toolDocks[0].Alignment, Is.EqualTo(DockAlignment.Left));
		Assert.That(toolDocks[0].VisibleDockables, Does.Contain(leftTop));
		Assert.That(toolDocks[0].VisibleDockables, Does.Contain(leftBottom));
	}

	[Test]
	public void Layout_LeftToolDock_ContainsLeftToolbox()
	{
		var left = new LeftToolbox();
		var right = new RightToolbox();
		var service = new DockLayoutService(new IToolbox[] { left, right });

		var root = service.Layout;
		var leftDock = root.VisibleDockables!.OfType<IToolDock>()
			.First(td => td.Alignment == DockAlignment.Left);

		Assert.That(leftDock.VisibleDockables, Does.Contain(left));
		Assert.That(leftDock.VisibleDockables, Does.Not.Contain(right));
	}

	[Test]
	public void Layout_RightToolDock_ContainsRightToolbox()
	{
		var left = new LeftToolbox();
		var right = new RightToolbox();
		var service = new DockLayoutService(new IToolbox[] { left, right });

		var root = service.Layout;
		var rightDock = root.VisibleDockables!.OfType<IToolDock>()
			.First(td => td.Alignment == DockAlignment.Right);

		Assert.That(rightDock.VisibleDockables, Does.Contain(right));
		Assert.That(rightDock.VisibleDockables, Does.Not.Contain(left));
	}

	[Test]
	public void Anchorables_ContainsAllToolboxes_AcrossGroups()
	{
		var left = new LeftToolbox();
		var right = new RightToolbox();
		var bottom = new BottomToolbox();
		var service = new DockLayoutService(new IToolbox[] { left, right, bottom });

		var anchorables = service.Anchorables.ToList();

		Assert.That(anchorables, Does.Contain(left));
		Assert.That(anchorables, Does.Contain(right));
		Assert.That(anchorables, Does.Contain(bottom));
		Assert.That(anchorables.Count, Is.EqualTo(3));
	}

	[Test]
	public void EmptyToolboxes_ProducesNoToolDocks()
	{
		var service = new DockLayoutService(System.Array.Empty<IToolbox>());

		var root = service.Layout;
		var toolDocks = root.VisibleDockables!.OfType<IToolDock>().ToList();

		Assert.That(toolDocks.Count, Is.EqualTo(0));
		Assert.That(service.Anchorables.Count(), Is.EqualTo(0));
	}

	[Test]
	public void SingleToolbox_ProducesSingleToolDock()
	{
		var left = new LeftToolbox();
		var service = new DockLayoutService(new IToolbox[] { left });

		var root = service.Layout;
		var toolDocks = root.VisibleDockables!.OfType<IToolDock>().ToList();

		Assert.That(toolDocks.Count, Is.EqualTo(1));
		Assert.That(toolDocks[0].Alignment, Is.EqualTo(DockAlignment.Left));
		Assert.That(toolDocks[0].VisibleDockables, Does.Contain(left));
	}
}