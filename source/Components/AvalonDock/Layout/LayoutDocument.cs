/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Linq;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Implements the layout model for the <see cref="Controls.LayoutDocumentControl"/>.
	/// </summary>
	[Serializable]
	public class LayoutDocument : LayoutContent
	{
		#region fields
		private bool _canMove = true;
		private bool _isVisible = true;
		private string _description = null;
		#endregion fields

		#region Properties

		/// <summary>Gets/sets whether a document can be dragged (to be dropped in a different location) or not. Use this property in conjunction with <see cref="CanMove"/> and <see cref="CanClose"/> to lock a document in its layout position.</summary>
		public bool CanMove
		{
			get => _canMove;
			set
			{
				if (value == _canMove) return;
				_canMove = value;
				RaisePropertyChanged(nameof(CanMove));
			}
		}

		/// <summary>Gets whether a document is visible or not.</summary>
		public bool IsVisible
		{
			get => _isVisible;
			internal set => _isVisible = value;
		}

		/// <summary>Gets/sets the document's description.
		/// Indicates the description to display (in the <see cref="NavigatorWindow"/>) for the document item.
		/// </summary>
		public string Description
		{
			get => _description;
			set
			{
				if (_description == value) return;
				_description = value;
				RaisePropertyChanged(nameof(Description));
			}
		}

		#endregion Properties

		#region Internal Methods

		internal bool CloseDocument()
		{
			if (!TestCanClose()) return false;
			CloseInternal();
			return true;
		}

		#endregion

		#region Overrides

		/// <inheritdoc />
		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			base.WriteXml(writer);
			if (!string.IsNullOrWhiteSpace(Description)) writer.WriteAttributeString(nameof(Description), Description);
			if (!CanMove) writer.WriteAttributeString(nameof(CanMove), CanMove.ToString());
		}

		/// <inheritdoc />
		public override void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.MoveToAttribute(nameof(Description))) Description = reader.Value;
			if (reader.MoveToAttribute(nameof(CanMove))) CanMove = bool.Parse(reader.Value);
			base.ReadXml(reader);
		}

		/// <inheritdoc />
		public override void Close()
		{
			if (Root?.Manager != null)
			{
				var dockingManager = Root.Manager;
				dockingManager._ExecuteCloseCommand(this);
			}
			else
				CloseDocument();
		}

#if TRACE
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("Document()");
		}
#endif

		protected override void InternalDock()
		{
			var root = Root as LayoutRoot;
			LayoutDocumentPane documentPane = null;
			if (root?.LastFocusedDocument != null && root.LastFocusedDocument != this) documentPane = root.LastFocusedDocument.Parent as LayoutDocumentPane;
			if (documentPane == null) documentPane = root.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
			var added = false;
			if (root?.Manager.LayoutUpdateStrategy != null) added = root.Manager.LayoutUpdateStrategy.BeforeInsertDocument(root, this, documentPane);
			if (!added)
			{
				if (documentPane == null) throw new InvalidOperationException("Layout must contains at least one LayoutDocumentPane in order to host documents");
				documentPane.Children.Add(this);
			}
			root?.Manager.LayoutUpdateStrategy?.AfterInsertDocument(root, this);
			base.InternalDock();
		}

		#endregion Overrides
	}
}
