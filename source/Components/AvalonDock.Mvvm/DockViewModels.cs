using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using AvalonDock.Core;

namespace AvalonDock.Mvvm
{
	/// <summary>
	/// Root dock view model.
	/// </summary>
	[DataContract]
	public class RootDock : DockBase, IRootDock
	{
		private IList<IDockable>? _floatingDockables;
		private IList<IDockable>? _pinnedDockables;
		private IDockable? _defaultLayout;

		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public IList<IDockable>? FloatingDockables
		{
			get => _floatingDockables;
			set => SetProperty(ref _floatingDockables, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public IList<IDockable>? PinnedDockables
		{
			get => _pinnedDockables;
			set => SetProperty(ref _pinnedDockables, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public IDockable? DefaultLayout
		{
			get => _defaultLayout;
			set => SetProperty(ref _defaultLayout, value);
		}

		public virtual void ShowWindows()
		{
		}

		public virtual void HideWindows()
		{
		}
	}

	/// <summary>
	/// Document dock view model.
	/// </summary>
	[DataContract]
	public class DocumentDock : DockBase, IDocumentDock
	{
		private bool _canCreateDocument = true;

		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public bool CanCreateDocument
		{
			get => _canCreateDocument;
			set => SetProperty(ref _canCreateDocument, value);
		}
	}

	/// <summary>
	/// Tool dock view model.
	/// </summary>
	[DataContract]
	public class ToolDock : DockBase, IToolDock
	{
		private DockAlignment _alignment = DockAlignment.Left;
		private bool _isExpanded;
		private bool _autoHide;

		[DataMember(IsRequired = false, EmitDefaultValue = true)]
		public DockAlignment Alignment
		{
			get => _alignment;
			set => SetProperty(ref _alignment, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public bool IsExpanded
		{
			get => _isExpanded;
			set => SetProperty(ref _isExpanded, value);
		}

		[DataMember(IsRequired = false, EmitDefaultValue = false)]
		public bool AutoHide
		{
			get => _autoHide;
			set => SetProperty(ref _autoHide, value);
		}
	}

	/// <summary>
	/// Leaf dockable view model for tools/anchorables.
	/// </summary>
	[DataContract]
	public class Tool : DockableBase
	{
	}

	/// <summary>
	/// Leaf dockable view model for documents.
	/// </summary>
	[DataContract]
	public class Document : DockableBase
	{
	}
}