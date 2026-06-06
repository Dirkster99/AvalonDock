using System;
using System.Linq;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Represents a layout document.
	/// </summary>
	[Serializable]
	public class LayoutDocument : LayoutContent, Core.Serialization.ISerializableLayoutDocument
	{
		private bool _canMove = true;
		private bool _isVisible = true;
		private string _description = null;

		/// <summary>
		/// Gets or sets a value indicating whether this instance can move.
		/// </summary>
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

		/// <summary>
		/// Gets a value indicating whether this instance can hide.
		/// </summary>
		public bool CanHide => false;

		/// <summary>
		/// Gets a value indicating whether this instance is visible.
		/// </summary>
		public bool IsVisible
		{
			get => _isVisible;
			internal set => _isVisible = value;
		}

		/// <summary>
		/// Gets or sets the description.
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

		/// <summary>
		/// Executes the close document operation.
		/// </summary>
		/// <returns><see langword="true"/> if the operation succeeds; otherwise, <see langword="false"/>.</returns>
		internal bool CloseDocument()
		{
			if (!TestCanClose()) return false;
			CloseInternal();
			return true;
		}

		/// <inheritdoc/>
		public override void Close()
		{
			if (Root?.Manager != null)
			{
				var dockingManager = Root.Manager;
				dockingManager.ExecuteCloseCommand(this);
			}
			else
			{
				CloseDocument();
			}
		}
#if TRACE
		/// <inheritdoc/>
		public override void ConsoleDump(int tab)
		{
			System.Diagnostics.Trace.Write(new string(' ', tab * 4));
			System.Diagnostics.Trace.WriteLine("Document()");
		}
#endif

		/// <inheritdoc/>
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
	}
}