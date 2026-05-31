#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using AvalonDock;
using AvalonDock.Core;
using AvalonDock.Layout;
using AvalonDock.Mvvm;
using NUnit.Framework;

namespace AvalonDockTest
{
	/// <summary>
	/// Tests for ToggleDockingManager zone group toggle DependencyProperties and commands.
	/// Static tests verify DP registration without requiring a WPF Application context.
	/// </summary>
	[TestFixture]
	public class ToggleDockingManagerZoneGroupStaticTests
	{
		/// <summary>
		/// Verifies that the IsPrimarySideBarOpen DP is registered correctly.
		/// </summary>
		[Test]
		public void IsPrimarySideBarOpenProperty_IsRegistered()
		{
			Assert.That(ToggleDockingManager.IsPrimarySideBarOpenProperty, Is.Not.Null);
			Assert.That(ToggleDockingManager.IsPrimarySideBarOpenProperty.Name, Is.EqualTo("IsPrimarySideBarOpen"));
			Assert.That(ToggleDockingManager.IsPrimarySideBarOpenProperty.PropertyType, Is.EqualTo(typeof(bool)));
			Assert.That(ToggleDockingManager.IsPrimarySideBarOpenProperty.OwnerType, Is.EqualTo(typeof(ToggleDockingManager)));
		}

		/// <summary>
		/// Verifies that the IsBottomPanelOpen DP is registered correctly.
		/// </summary>
		[Test]
		public void IsBottomPanelOpenProperty_IsRegistered()
		{
			Assert.That(ToggleDockingManager.IsBottomPanelOpenProperty, Is.Not.Null);
			Assert.That(ToggleDockingManager.IsBottomPanelOpenProperty.Name, Is.EqualTo("IsBottomPanelOpen"));
			Assert.That(ToggleDockingManager.IsBottomPanelOpenProperty.PropertyType, Is.EqualTo(typeof(bool)));
			Assert.That(ToggleDockingManager.IsBottomPanelOpenProperty.OwnerType, Is.EqualTo(typeof(ToggleDockingManager)));
		}

		/// <summary>
		/// Verifies that the IsSecondarySideBarOpen DP is registered correctly.
		/// </summary>
		[Test]
		public void IsSecondarySideBarOpenProperty_IsRegistered()
		{
			Assert.That(ToggleDockingManager.IsSecondarySideBarOpenProperty, Is.Not.Null);
			Assert.That(ToggleDockingManager.IsSecondarySideBarOpenProperty.Name, Is.EqualTo("IsSecondarySideBarOpen"));
			Assert.That(ToggleDockingManager.IsSecondarySideBarOpenProperty.PropertyType, Is.EqualTo(typeof(bool)));
			Assert.That(ToggleDockingManager.IsSecondarySideBarOpenProperty.OwnerType, Is.EqualTo(typeof(ToggleDockingManager)));
		}

		/// <summary>
		/// All three state DPs should default to false.
		/// </summary>
		[Test]
		public void StateDPs_DefaultToFalse()
		{
			var defaultPrimary = ToggleDockingManager.IsPrimarySideBarOpenProperty.DefaultMetadata.DefaultValue;
			var defaultBottom = ToggleDockingManager.IsBottomPanelOpenProperty.DefaultMetadata.DefaultValue;
			var defaultSecondary = ToggleDockingManager.IsSecondarySideBarOpenProperty.DefaultMetadata.DefaultValue;

			Assert.That(defaultPrimary, Is.EqualTo(false));
			Assert.That(defaultBottom, Is.EqualTo(false));
			Assert.That(defaultSecondary, Is.EqualTo(false));
		}
	}

	/// <summary>
	/// Extended DockLayoutService tests covering multi-zone toolbox scenarios.
	/// </summary>
	[TestFixture]
	public class DockLayoutServiceZoneTests
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
	/// Tests for ToggleLayoutEngine zone-to-side mapping used by zone group toggle.
	/// </summary>
	[TestFixture]
	public class ZoneToSideMapTests
	{
		/// <summary>
		/// Left zones map to Left AnchorSide.
		/// </summary>
		[Test]
		public void LeftZones_MapToLeftSide()
		{
			Assert.That(ToggleLayoutEngine.ZoneToAnchorSide(DockZone.LeftTop), Is.EqualTo(AnchorSide.Left));
			Assert.That(ToggleLayoutEngine.ZoneToAnchorSide(DockZone.LeftBottom), Is.EqualTo(AnchorSide.Left));
		}

		/// <summary>
		/// Right zones map to Right AnchorSide.
		/// </summary>
		[Test]
		public void RightZones_MapToRightSide()
		{
			Assert.That(ToggleLayoutEngine.ZoneToAnchorSide(DockZone.RightTop), Is.EqualTo(AnchorSide.Right));
			Assert.That(ToggleLayoutEngine.ZoneToAnchorSide(DockZone.RightBottom), Is.EqualTo(AnchorSide.Right));
		}

		/// <summary>
		/// Bottom zones map to Bottom AnchorSide.
		/// </summary>
		[Test]
		public void BottomZones_MapToBottomSide()
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
}