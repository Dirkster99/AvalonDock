#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using AvalonDock;
using AvalonDock.Core;
using AvalonDock.Layout;
using AvalonDock.Mvvm;
using NUnit.Framework;

namespace AvalonDockTest;

/// <summary>
/// Tests for DockLayoutService anchorable visibility and zone group functionality.
/// </summary>
[TestFixture]
public class DockLayoutServiceAnchorableVisibilityTests
{
	private class LeftTopToolbox : ToolboxBase
	{
		public LeftTopToolbox()
		{
			Id = "left-top";
			Title = "Left Top";
			Zone = DockZone.LeftTop;
		}
	}

	private class LeftBottomToolbox : ToolboxBase
	{
		public LeftBottomToolbox()
		{
			Id = "left-bottom";
			Title = "Left Bottom";
			Zone = DockZone.LeftBottom;
		}
	}

	private class RightTopToolbox : ToolboxBase
	{
		public RightTopToolbox()
		{
			Id = "right-top";
			Title = "Right Top";
			Zone = DockZone.RightTop;
		}
	}

	private class BottomLeftToolbox : ToolboxBase
	{
		public BottomLeftToolbox()
		{
			Id = "bottom-left";
			Title = "Bottom Left";
			Zone = DockZone.BottomLeft;
		}
	}

	private class BottomRightToolbox : ToolboxBase
	{
		public BottomRightToolbox()
		{
			Id = "bottom-right";
			Title = "Bottom Right";
			Zone = DockZone.BottomRight;
		}
	}

	/// <summary>
	/// ShowAnchorable marks the anchorable as open in the service.
	/// </summary>
	[Test]
	public void ShowAnchorable_MarksAsOpen()
	{
		var service = new DockLayoutService(new IToolbox[] { new LeftTopToolbox() });
		var toolbox = service.GetAnchorable<LeftTopToolbox>()!;

		Assert.That(service.IsAnchorableOpen(toolbox), Is.False);
		service.ShowAnchorable(toolbox);
		Assert.That(service.IsAnchorableOpen(toolbox), Is.True);
	}

	/// <summary>
	/// HideAnchorable marks the anchorable as closed in the service.
	/// </summary>
	[Test]
	public void HideAnchorable_MarksAsClosed()
	{
		var service = new DockLayoutService(new IToolbox[] { new LeftTopToolbox() });
		var toolbox = service.GetAnchorable<LeftTopToolbox>()!;

		service.ShowAnchorable(toolbox);
		Assert.That(service.IsAnchorableOpen(toolbox), Is.True);

		service.HideAnchorable(toolbox);
		Assert.That(service.IsAnchorableOpen(toolbox), Is.False);
	}

	/// <summary>
	/// IsAnchorableOpen returns false for anchorables never shown.
	/// </summary>
	[Test]
	public void IsAnchorableOpen_ReturnsFalse_WhenNeverShown()
	{
		var service = new DockLayoutService(new IToolbox[] { new LeftTopToolbox() });
		var toolbox = service.GetAnchorable<LeftTopToolbox>()!;
		Assert.That(service.IsAnchorableOpen(toolbox), Is.False);
	}

	/// <summary>
	/// IsSideOpen returns true when any toolbox on that side is open.
	/// </summary>
	[Test]
	public void IsSideOpen_ReturnsTrue_WhenAnyToolboxOnSideIsOpen()
	{
		var lt = new LeftTopToolbox();
		var lb = new LeftBottomToolbox();
		var service = new DockLayoutService(new IToolbox[] { lt, lb });

		Assert.That(service.IsSideOpen(ToolboxSide.Left), Is.False);

		service.ShowAnchorable(lt);
		Assert.That(service.IsSideOpen(ToolboxSide.Left), Is.True);
		Assert.That(service.IsSideOpen(ToolboxSide.Right), Is.False);
	}

	/// <summary>
	/// ShowAnchorable raises AnchorableStateChanged event.
	/// </summary>
	[Test]
	public void ShowAnchorable_RaisesStateChangedEvent()
	{
		var service = new DockLayoutService(new IToolbox[] { new LeftTopToolbox() });
		bool raised = false;
		service.AnchorableStateChanged += (_, _) => raised = true;

		service.ShowAnchorable(service.GetAnchorable<LeftTopToolbox>()!);
		Assert.That(raised, Is.True);
	}

	/// <summary>
	/// HideAnchorable raises AnchorableStateChanged event.
	/// </summary>
	[Test]
	public void HideAnchorable_RaisesStateChangedEvent()
	{
		var service = new DockLayoutService(new IToolbox[] { new LeftTopToolbox() });
		var toolbox = service.GetAnchorable<LeftTopToolbox>()!;
		service.ShowAnchorable(toolbox);

		bool raised = false;
		service.AnchorableStateChanged += (_, _) => raised = true;

		service.HideAnchorable(toolbox);
		Assert.That(raised, Is.True);
	}

	/// <summary>
	/// ShowAnchorable does not raise event when already open (idempotent).
	/// </summary>
	[Test]
	public void ShowAnchorable_DoesNotRaiseEvent_WhenAlreadyOpen()
	{
		var service = new DockLayoutService(new IToolbox[] { new LeftTopToolbox() });
		var toolbox = service.GetAnchorable<LeftTopToolbox>()!;
		service.ShowAnchorable(toolbox);

		bool raised = false;
		service.AnchorableStateChanged += (_, _) => raised = true;

		service.ShowAnchorable(toolbox);
		Assert.That(raised, Is.False);
	}

	/// <summary>
	/// HideAnchorable does not raise event when already hidden (idempotent).
	/// </summary>
	[Test]
	public void HideAnchorable_DoesNotRaiseEvent_WhenAlreadyHidden()
	{
		var service = new DockLayoutService(new IToolbox[] { new LeftTopToolbox() });
		var toolbox = service.GetAnchorable<LeftTopToolbox>()!;

		bool raised = false;
		service.AnchorableStateChanged += (_, _) => raised = true;

		service.HideAnchorable(toolbox);
		Assert.That(raised, Is.False);
	}

	/// <summary>
	/// SideToggleManager hides all open toolboxes on the side.
	/// </summary>
	[Test]
	public void ToggleSide_HidesAllOpen_WhenAnyAreOpen()
	{
		var lt = new LeftTopToolbox();
		var lb = new LeftBottomToolbox();
		var service = new DockLayoutService(new IToolbox[] { lt, lb });
		var toggle = new SideToggleManager(service);
		service.ShowAnchorable(lt);
		service.ShowAnchorable(lb);

		toggle.Toggle(ToolboxSide.Left);
		Assert.That(service.IsAnchorableOpen(lt), Is.False);
		Assert.That(service.IsAnchorableOpen(lb), Is.False);
	}

	/// <summary>
	/// SideToggleManager shows first available when none are open and no history exists.
	/// </summary>
	[Test]
	public void ToggleSide_ShowsFirst_WhenNoneAreOpenAndNoHistory()
	{
		var lt = new LeftTopToolbox();
		var lb = new LeftBottomToolbox();
		var service = new DockLayoutService(new IToolbox[] { lt, lb });
		var toggle = new SideToggleManager(service);

		toggle.Toggle(ToolboxSide.Left);
		Assert.That(service.IsAnchorableOpen(lt), Is.True);
	}

	/// <summary>
	/// SideToggleManager restores previously open toolboxes after toggling off and on again.
	/// </summary>
	[Test]
	public void ToggleSide_RestoresLastOpened_WhenToggledBackOn()
	{
		var lt = new LeftTopToolbox();
		var lb = new LeftBottomToolbox();
		var service = new DockLayoutService(new IToolbox[] { lt, lb });
		var toggle = new SideToggleManager(service);

		// Open only lb (the second one)
		service.ShowAnchorable(lb);
		Assert.That(service.IsAnchorableOpen(lb), Is.True);
		Assert.That(service.IsAnchorableOpen(lt), Is.False);

		// Toggle off — should remember lb was open
		toggle.Toggle(ToolboxSide.Left);
		Assert.That(service.IsAnchorableOpen(lb), Is.False);

		// Toggle back on — should restore lb, not lt
		toggle.Toggle(ToolboxSide.Left);
		Assert.That(service.IsAnchorableOpen(lb), Is.True, "Last opened toolbox should be restored");
		Assert.That(service.IsAnchorableOpen(lt), Is.False, "Toolbox that was not previously open should stay closed");
	}

	/// <summary>
	/// SideToggleManager restores all previously open toolboxes, not just one.
	/// </summary>
	[Test]
	public void ToggleSide_RestoresAllPreviouslyOpen_WhenMultipleWereOpen()
	{
		var lt = new LeftTopToolbox();
		var lb = new LeftBottomToolbox();
		var service = new DockLayoutService(new IToolbox[] { lt, lb });
		var toggle = new SideToggleManager(service);

		// Open both
		service.ShowAnchorable(lt);
		service.ShowAnchorable(lb);

		// Toggle off
		toggle.Toggle(ToolboxSide.Left);
		Assert.That(service.IsAnchorableOpen(lt), Is.False);
		Assert.That(service.IsAnchorableOpen(lb), Is.False);

		// Toggle back on — both should restore
		toggle.Toggle(ToolboxSide.Left);
		Assert.That(service.IsAnchorableOpen(lt), Is.True);
		Assert.That(service.IsAnchorableOpen(lb), Is.True);
	}

	/// <summary>
	/// SideToggleManager does nothing when no toolboxes exist on the side.
	/// </summary>
	[Test]
	public void ToggleSide_DoesNothing_WhenNoToolboxesOnSide()
	{
		var lt = new LeftTopToolbox();
		var service = new DockLayoutService(new IToolbox[] { lt });
		var toggle = new SideToggleManager(service);

		int eventCount = 0;
		service.AnchorableStateChanged += (_, _) => eventCount++;

		toggle.Toggle(ToolboxSide.Right);
		Assert.That(eventCount, Is.EqualTo(0));
	}

	/// <summary>
	/// GetAnchorable returns the correct typed instance among many zones.
	/// </summary>
	[Test]
	public void GetAnchorable_ReturnsCorrectType_AcrossMultipleZones()
	{
		var lt = new LeftTopToolbox();
		var lb = new LeftBottomToolbox();
		var rt = new RightTopToolbox();
		var bl = new BottomLeftToolbox();
		var br = new BottomRightToolbox();
		var service = new DockLayoutService(new IToolbox[] { lt, lb, rt, bl, br });

		Assert.That(service.GetAnchorable<LeftTopToolbox>(), Is.SameAs(lt));
		Assert.That(service.GetAnchorable<LeftBottomToolbox>(), Is.SameAs(lb));
		Assert.That(service.GetAnchorable<RightTopToolbox>(), Is.SameAs(rt));
		Assert.That(service.GetAnchorable<BottomLeftToolbox>(), Is.SameAs(bl));
		Assert.That(service.GetAnchorable<BottomRightToolbox>(), Is.SameAs(br));
	}

	/// <summary>
	/// GetAnchorable returns null for unregistered type when other toolboxes exist.
	/// </summary>
	[Test]
	public void GetAnchorable_ReturnsNull_WhenTypeNotRegistered()
	{
		var service = new DockLayoutService(new IToolbox[] { new LeftTopToolbox() });
		Assert.That(service.GetAnchorable<RightTopToolbox>(), Is.Null);
	}

	/// <summary>
	/// Anchorables preserve registration order.
	/// </summary>
	[Test]
	public void Anchorables_PreservesRegistrationOrder()
	{
		var lt = new LeftTopToolbox();
		var br = new BottomRightToolbox();
		var rt = new RightTopToolbox();
		var service = new DockLayoutService(new IToolbox[] { lt, br, rt });

		var list = service.Anchorables.ToList();
		Assert.That(list[0], Is.SameAs(lt));
		Assert.That(list[1], Is.SameAs(br));
		Assert.That(list[2], Is.SameAs(rt));
	}

	/// <summary>
	/// Toolbox Zone can be changed after construction and reflects in the instance.
	/// </summary>
	[Test]
	public void Toolbox_ZoneCanBeChanged()
	{
		var toolbox = new LeftTopToolbox();
		Assert.That(toolbox.Zone, Is.EqualTo(DockZone.LeftTop));

		toolbox.Zone = DockZone.RightBottom;
		Assert.That(toolbox.Zone, Is.EqualTo(DockZone.RightBottom));
	}

	/// <summary>
	/// OpenDocument and CloseDocument with multiple docs maintains correct active state.
	/// </summary>
	[Test]
	public void OpenDocument_CloseDocument_MultipleDocuments_TracksActive()
	{
		var service = new DockLayoutService(Array.Empty<IToolbox>());
		var doc1 = new Document { Id = "d1", Title = "D1" };
		var doc2 = new Document { Id = "d2", Title = "D2" };
		var doc3 = new Document { Id = "d3", Title = "D3" };

		service.OpenDocument(doc1);
		service.OpenDocument(doc2);
		service.OpenDocument(doc3);
		Assert.That(service.ActiveDockable, Is.SameAs(doc3));
		Assert.That(service.Documents.Count(), Is.EqualTo(3));

		service.CloseDocument(doc3);
		Assert.That(service.ActiveDockable, Is.SameAs(doc2));

		service.CloseDocument(doc2);
		Assert.That(service.ActiveDockable, Is.SameAs(doc1));

		service.CloseDocument(doc1);
		Assert.That(service.ActiveDockable, Is.Null);
		Assert.That(service.Documents.Count(), Is.EqualTo(0));
	}

	/// <summary>
	/// Closing a non-active document does not change ActiveDockable.
	/// </summary>
	[Test]
	public void CloseDocument_NonActive_DoesNotChangeActive()
	{
		var service = new DockLayoutService(Array.Empty<IToolbox>());
		var doc1 = new Document { Id = "d1", Title = "D1" };
		var doc2 = new Document { Id = "d2", Title = "D2" };

		service.OpenDocument(doc1);
		service.OpenDocument(doc2);
		Assert.That(service.ActiveDockable, Is.SameAs(doc2));

		service.CloseDocument(doc1);
		Assert.That(service.ActiveDockable, Is.SameAs(doc2));
		Assert.That(service.Documents.Count(), Is.EqualTo(1));
	}

	/// <summary>
	/// FindDocument returns null when collection is empty.
	/// </summary>
	[Test]
	public void FindDocument_ReturnsNull_WhenEmpty()
	{
		var service = new DockLayoutService(Array.Empty<IToolbox>());
		Assert.That(service.FindDocument<Document>(d => d.Id == "x"), Is.Null);
	}

	/// <summary>
	/// OpenOrActivateDocument does not create duplicate when opened twice.
	/// </summary>
	[Test]
	public void OpenOrActivateDocument_NoDuplicate_WhenCalledTwice()
	{
		var service = new DockLayoutService(Array.Empty<IToolbox>());
		int factoryCalls = 0;
		var doc = new Document { Id = "unique", Title = "Unique" };

		service.OpenOrActivateDocument<Document>(
			d => d.Id == "unique",
			() => { factoryCalls++; return doc; });

		service.OpenOrActivateDocument<Document>(
			d => d.Id == "unique",
			() => { factoryCalls++; return new Document { Id = "dupe", Title = "Dupe" }; });

		Assert.That(factoryCalls, Is.EqualTo(1));
		Assert.That(service.Documents.Count(), Is.EqualTo(1));
	}

	/// <summary>
	/// ActiveDockable can be set explicitly.
	/// </summary>
	[Test]
	public void ActiveDockable_CanBeSetExplicitly()
	{
		var service = new DockLayoutService(Array.Empty<IToolbox>());
		var doc1 = new Document { Id = "d1", Title = "D1" };
		var doc2 = new Document { Id = "d2", Title = "D2" };

		service.OpenDocument(doc1);
		service.OpenDocument(doc2);
		Assert.That(service.ActiveDockable, Is.SameAs(doc2));

		service.ActiveDockable = doc1;
		Assert.That(service.ActiveDockable, Is.SameAs(doc1));
	}

	/// <summary>
	/// Empty service has empty document and anchorable collections.
	/// </summary>
	[Test]
	public void EmptyService_HasEmptyCollections()
	{
		var service = new DockLayoutService(Array.Empty<IToolbox>());
		Assert.That(service.Documents.Count(), Is.EqualTo(0));
		Assert.That(service.Anchorables.Count(), Is.EqualTo(0));
		Assert.That(service.ActiveDockable, Is.Null);
	}
}

/// <summary>
/// Tests for DockZone extension methods and ToggleLayoutEngine zone-to-side mapping.
/// </summary>
[TestFixture]
public class DockZoneExtensionTests
{
	/// <summary>
	/// Left zones map to Left ToolboxSide.
	/// </summary>
	[Test]
	public void LeftZones_MapToLeftSide()
	{
		Assert.That(DockZone.LeftTop.ToSide(), Is.EqualTo(ToolboxSide.Left));
		Assert.That(DockZone.LeftBottom.ToSide(), Is.EqualTo(ToolboxSide.Left));
	}

	/// <summary>
	/// Right zones map to Right ToolboxSide.
	/// </summary>
	[Test]
	public void RightZones_MapToRightSide()
	{
		Assert.That(DockZone.RightTop.ToSide(), Is.EqualTo(ToolboxSide.Right));
		Assert.That(DockZone.RightBottom.ToSide(), Is.EqualTo(ToolboxSide.Right));
	}

	/// <summary>
	/// Bottom zones map to Bottom ToolboxSide.
	/// </summary>
	[Test]
	public void BottomZones_MapToBottomSide()
	{
		Assert.That(DockZone.BottomLeft.ToSide(), Is.EqualTo(ToolboxSide.Bottom));
		Assert.That(DockZone.BottomRight.ToSide(), Is.EqualTo(ToolboxSide.Bottom));
	}

	/// <summary>
	/// Left zones map to Left AnchorSide (existing ToggleLayoutEngine mapping).
	/// </summary>
	[Test]
	public void LeftZones_MapToLeftAnchorSide()
	{
		Assert.That(ToggleLayoutEngine.ZoneToAnchorSide(DockZone.LeftTop), Is.EqualTo(AnchorSide.Left));
		Assert.That(ToggleLayoutEngine.ZoneToAnchorSide(DockZone.LeftBottom), Is.EqualTo(AnchorSide.Left));
	}

	/// <summary>
	/// Right zones map to Right AnchorSide.
	/// </summary>
	[Test]
	public void RightZones_MapToRightAnchorSide()
	{
		Assert.That(ToggleLayoutEngine.ZoneToAnchorSide(DockZone.RightTop), Is.EqualTo(AnchorSide.Right));
		Assert.That(ToggleLayoutEngine.ZoneToAnchorSide(DockZone.RightBottom), Is.EqualTo(AnchorSide.Right));
	}

	/// <summary>
	/// Bottom zones map to Bottom AnchorSide.
	/// </summary>
	[Test]
	public void BottomZones_MapToBottomAnchorSide()
	{
		Assert.That(ToggleLayoutEngine.ZoneToAnchorSide(DockZone.BottomLeft), Is.EqualTo(AnchorSide.Bottom));
		Assert.That(ToggleLayoutEngine.ZoneToAnchorSide(DockZone.BottomRight), Is.EqualTo(AnchorSide.Bottom));
	}

	/// <summary>
	/// IsBottomZone correctly identifies bottom zones.
	/// </summary>
	[Test]
	public void IsBottomZone_CorrectlyIdentifiesBottomZones()
	{
		Assert.That(ToggleLayoutEngine.IsBottomZone(DockZone.BottomLeft), Is.True);
		Assert.That(ToggleLayoutEngine.IsBottomZone(DockZone.BottomRight), Is.True);
		Assert.That(ToggleLayoutEngine.IsBottomZone(DockZone.LeftTop), Is.False);
		Assert.That(ToggleLayoutEngine.IsBottomZone(DockZone.LeftBottom), Is.False);
		Assert.That(ToggleLayoutEngine.IsBottomZone(DockZone.RightTop), Is.False);
		Assert.That(ToggleLayoutEngine.IsBottomZone(DockZone.RightBottom), Is.False);
	}
}