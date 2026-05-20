#nullable enable
using System.Collections.Generic;
using AvalonDock.Core;
using AvalonDock.Mvvm;
using NUnit.Framework;

namespace AvalonDockTest
{
	internal class ConcreteToolbox : ToolboxBase
	{
		public ConcreteToolbox()
		{
			Id = "test-toolbox";
			Title = "Test Toolbox";
			ToolTipText = "A test toolbox";
			Zone = DockZone.LeftTop;
			Icon = "icon-placeholder";
		}
	}

	[TestFixture]
	public class ToolboxBaseTests
	{
		[Test]
		public void ToolboxBase_ImplementsIToolbox()
		{
			var toolbox = new ConcreteToolbox();
			Assert.That(toolbox, Is.AssignableTo<IToolbox>());
		}

		[Test]
		public void ToolboxBase_ImplementsIDockable()
		{
			var toolbox = new ConcreteToolbox();
			Assert.That(toolbox, Is.AssignableTo<IDockable>());
		}

		[Test]
		public void ToolboxBase_InheritsFromDockableBase()
		{
			var toolbox = new ConcreteToolbox();
			Assert.That(toolbox, Is.AssignableTo<DockableBase>());
		}

		[Test]
		public void ToolboxBase_SetsPropertiesFromConstructor()
		{
			var toolbox = new ConcreteToolbox();

			Assert.That(toolbox.Id, Is.EqualTo("test-toolbox"));
			Assert.That(toolbox.Title, Is.EqualTo("Test Toolbox"));
			Assert.That(toolbox.ToolTipText, Is.EqualTo("A test toolbox"));
			Assert.That(toolbox.Zone, Is.EqualTo(DockZone.LeftTop));
			Assert.That(toolbox.Icon, Is.EqualTo("icon-placeholder"));
		}

		[Test]
		public void ToolboxBase_DefaultValues()
		{
			var toolbox = new ConcreteToolbox();

			Assert.That(toolbox.IsOpenByDefault, Is.False);
			Assert.That(toolbox.CanClose, Is.True);
			Assert.That(toolbox.CanPin, Is.True);
			Assert.That(toolbox.CanFloat, Is.True);
			Assert.That(toolbox.CanDrag, Is.True);
			Assert.That(toolbox.CanDrop, Is.True);
			Assert.That(toolbox.IsModified, Is.False);
			Assert.That(toolbox.IsActive, Is.False);
			Assert.That(toolbox.DockState, Is.EqualTo(DockState.Docked));
		}

		[Test]
		public void ToolboxBase_Zone_RaisesPropertyChanged()
		{
			var toolbox = new ConcreteToolbox();
			var changed = false;
			toolbox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(ToolboxBase.Zone))
					changed = true;
			};

			toolbox.Zone = DockZone.RightTop;

			Assert.That(changed, Is.True);
			Assert.That(toolbox.Zone, Is.EqualTo(DockZone.RightTop));
		}

		[Test]
		public void ToolboxBase_ToolTipText_RaisesPropertyChanged()
		{
			var toolbox = new ConcreteToolbox();
			var changed = false;
			toolbox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(ToolboxBase.ToolTipText))
					changed = true;
			};

			toolbox.ToolTipText = "Updated tooltip";

			Assert.That(changed, Is.True);
			Assert.That(toolbox.ToolTipText, Is.EqualTo("Updated tooltip"));
		}

		[Test]
		public void ToolboxBase_IsOpenByDefault_RaisesPropertyChanged()
		{
			var toolbox = new ConcreteToolbox();
			var changed = false;
			toolbox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(ToolboxBase.IsOpenByDefault))
					changed = true;
			};

			toolbox.IsOpenByDefault = true;

			Assert.That(changed, Is.True);
			Assert.That(toolbox.IsOpenByDefault, Is.True);
		}

		[Test]
		public void ToolboxBase_Icon_RaisesPropertyChanged()
		{
			var toolbox = new ConcreteToolbox();
			var changed = false;
			toolbox.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == nameof(ToolboxBase.Icon))
					changed = true;
			};

			var newIcon = new object();
			toolbox.Icon = newIcon;

			Assert.That(changed, Is.True);
			Assert.That(toolbox.Icon, Is.SameAs(newIcon));
		}

		[Test]
		public void ToolboxBase_OnClose_ReturnsTrue()
		{
			var toolbox = new ConcreteToolbox();
			Assert.That(toolbox.OnClose(), Is.True);
		}
	}

	[TestFixture]
	public class DockableBaseTests
	{
		private class SimpleDockable : DockableBase { }

		[Test]
		public void DockableBase_ImplementsIDockable()
		{
			var dockable = new SimpleDockable();
			Assert.That(dockable, Is.AssignableTo<IDockable>());
		}

		[Test]
		public void DockableBase_DefaultValues()
		{
			var dockable = new SimpleDockable();

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
		public void DockableBase_Title_RaisesPropertyChanged()
		{
			var dockable = new SimpleDockable();
			var changedProps = new List<string>();
			dockable.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName!);

			dockable.Title = "New Title";

			Assert.That(changedProps, Does.Contain("Title"));
			Assert.That(dockable.Title, Is.EqualTo("New Title"));
		}

		[Test]
		public void DockableBase_Id_RaisesPropertyChanged()
		{
			var dockable = new SimpleDockable();
			var changed = false;
			dockable.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName == "Id") changed = true;
			};

			dockable.Id = "new-id";

			Assert.That(changed, Is.True);
			Assert.That(dockable.Id, Is.EqualTo("new-id"));
		}

		[Test]
		public void DockableBase_DoesNotRaisePropertyChanged_WhenValueUnchanged()
		{
			var dockable = new SimpleDockable();
			dockable.Title = "Same";

			var raised = false;
			dockable.PropertyChanged += (_, _) => raised = true;

			dockable.Title = "Same";

			Assert.That(raised, Is.False);
		}

		[Test]
		public void DockableBase_OnClose_DefaultReturnsTrue()
		{
			var dockable = new SimpleDockable();
			Assert.That(dockable.OnClose(), Is.True);
		}
	}

	[TestFixture]
	public class MvvmDockViewModelTests
	{
		[Test]
		public void Tool_ImplementsIDockable()
		{
			var tool = new Tool();
			Assert.That(tool, Is.AssignableTo<IDockable>());
		}

		[Test]
		public void Document_ImplementsIDockable()
		{
			var doc = new Document();
			Assert.That(doc, Is.AssignableTo<IDockable>());
		}

		[Test]
		public void ToolDock_ImplementsIToolDock()
		{
			var toolDock = new ToolDock();
			Assert.That(toolDock, Is.AssignableTo<IToolDock>());
			Assert.That(toolDock.Alignment, Is.EqualTo(DockAlignment.Left));
			Assert.That(toolDock.IsExpanded, Is.False);
			Assert.That(toolDock.AutoHide, Is.False);
		}

		[Test]
		public void DocumentDock_ImplementsIDocumentDock()
		{
			var docDock = new DocumentDock();
			Assert.That(docDock, Is.AssignableTo<IDocumentDock>());
			Assert.That(docDock.CanCreateDocument, Is.True);
		}

		[Test]
		public void RootDock_ImplementsIRootDock()
		{
			var root = new RootDock();
			Assert.That(root, Is.AssignableTo<IRootDock>());
			Assert.That(root.FloatingDockables, Is.Null);
			Assert.That(root.PinnedDockables, Is.Null);
			Assert.That(root.DefaultLayout, Is.Null);
		}

		[Test]
		public void Factory_CreateRootDock_ReturnsRootDock()
		{
			var factory = new Factory();
			var root = factory.CreateRootDock();
			Assert.That(root, Is.InstanceOf<RootDock>());
		}

		[Test]
		public void Factory_CreateDocumentDock_ReturnsDocumentDock()
		{
			var factory = new Factory();
			var dock = factory.CreateDocumentDock();
			Assert.That(dock, Is.InstanceOf<DocumentDock>());
		}

		[Test]
		public void Factory_CreateToolDock_ReturnsToolDock()
		{
			var factory = new Factory();
			var dock = factory.CreateToolDock();
			Assert.That(dock, Is.InstanceOf<ToolDock>());
		}
	}
}
