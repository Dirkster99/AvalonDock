using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;

namespace AvalonDock.Controls
{
	/// <summary>
	/// A standalone, top level <see cref="Window"/> that hosts the content of a single
	/// <see cref="LayoutAnchorable"/> outside of the docking layout.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Unlike <see cref="LayoutFloatingWindowControl"/>, which is a borderless window owned by the
	/// main window and which takes part in drag docking, this is an ordinary WPF window. It uses the
	/// operating system chrome, owns a taskbar entry, minimizes and restores independently of the main
	/// window and may be moved behind it. This corresponds to the "Window" view mode that IDEs offer for their tool windows.
	/// </para>
	/// <para>
	/// The window hosts the <see cref="LayoutItem.View"/> presenter of the anchorable. That presenter is
	/// the single owner of the user supplied content, so it is moved into this window rather than being
	/// duplicated. The caller must disconnect the presenter from its previous visual and logical parent
	/// before constructing the window; see
	/// <see cref="DockingManager.DetachAnchorableToWindow(LayoutAnchorable)"/>, which performs the
	/// whole sequence.
	/// </para>
	/// </remarks>
	public class DetachedAnchorableWindow : Window
	{
		/// <summary>The width used when the model does not carry a usable floating width.</summary>
		private const double DefaultDetachedWidth = 400d;

		/// <summary>The height used when the model does not carry a usable floating height.</summary>
		private const double DefaultDetachedHeight = 500d;

		private readonly LayoutAnchorable _model;
		private readonly Grid _root;
		private ContentPresenter _hostedView;
		private ResourceDictionary _currentThemeResourceDictionary;

		/// <summary>
		/// Initializes a new instance of the <see cref="DetachedAnchorableWindow"/> class.
		/// </summary>
		/// <param name="model">The anchorable whose content is hosted by this window.</param>
		/// <param name="hostedView">
		/// The presenter that holds the content of <paramref name="model"/>. It must already be
		/// disconnected from any previous visual and logical parent.
		/// </param>
		/// <param name="header">
		/// The title control shown above the content, so the window keeps the caption and menu the
		/// anchorable had while docked. May be <see langword="null"/> for a bare window.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="model"/> or <paramref name="hostedView"/> is <see langword="null"/>.
		/// </exception>
		public DetachedAnchorableWindow(LayoutAnchorable model, ContentPresenter hostedView, FrameworkElement header = null)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			_hostedView = hostedView ?? throw new ArgumentNullException(nameof(hostedView));

			// An ordinary, independent top level window - see the remarks on this class. These are the
			// WPF defaults, but they are set explicitly because they are the point of this window type.
			Owner = null;
			ShowInTaskbar = true;
			WindowStyle = WindowStyle.SingleBorderWindow;
			ResizeMode = ResizeMode.CanResize;

			_root = new Grid();
			_root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			_root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

			if (header != null)
			{
				Grid.SetRow(header, 0);
				_root.Children.Add(header);
			}

			Grid.SetRow(_hostedView, 1);
			_root.Children.Add(_hostedView);
			Content = _root;

			ApplyModelBounds();
			UpdateTitle();
			UpdateIcon();

			_model.PropertyChanged += OnModelPropertyChanged;
		}

		/// <summary>
		/// Gets the anchorable whose content is hosted by this window.
		/// </summary>
		public LayoutAnchorable Model => _model;

		/// <summary>
		/// Gets a value indicating whether the hosted presenter is still owned by this window.
		/// </summary>
		public bool HasView => _hostedView != null;

		/// <summary>
		/// Gets a value indicating whether this window has already been closed.
		/// </summary>
		/// <remarks>
		/// <see cref="Window.Close()"/> throws when it is called on a window that is already closing, which
		/// is exactly the situation when the docking manager reacts to <see cref="Window.Closed"/>. This
		/// flag lets callers skip that second, redundant close.
		/// </remarks>
		public bool IsClosed { get; private set; }

		/// <summary>
		/// Releases the hosted presenter so that it can be inserted back into the docking layout.
		/// </summary>
		/// <returns>
		/// The presenter that was hosted, or <see langword="null"/> when it has already been released.
		/// </returns>
		public ContentPresenter ReleaseView()
		{
			var view = _hostedView;
			_hostedView = null;

			if (view != null && _root.Children.Contains(view))
			{
				_root.Children.Remove(view);
			}

			return view;
		}

		/// <summary>
		/// Merges the resources of the given theme into this window so that the hosted content is styled
		/// like the content inside the docking manager.
		/// </summary>
		/// <param name="oldTheme">The theme whose resources should be removed, may be <see langword="null"/>.</param>
		/// <param name="newTheme">The theme whose resources should be applied, may be <see langword="null"/>.</param>
		/// <remarks>
		/// A detached window is the root of its own visual tree, so it does not inherit the resources
		/// that the docking manager carries. Without this the hosted content falls back to the default
		/// WPF styling whenever the theme is not merged at application level.
		/// </remarks>
		public void UpdateThemeResources(Theme oldTheme, Theme newTheme)
		{
			if (oldTheme != null)
			{
				if (_currentThemeResourceDictionary != null)
				{
					Resources.MergedDictionaries.Remove(_currentThemeResourceDictionary);
					_currentThemeResourceDictionary = null;
				}
				else
				{
					var toRemove = Resources.MergedDictionaries.FirstOrDefault(r => r.Source == oldTheme.GetResourceUri());
					if (toRemove != null)
					{
						Resources.MergedDictionaries.Remove(toRemove);
					}
				}
			}

			if (newTheme == null) return;

			if (newTheme is DictionaryTheme dictionaryTheme)
			{
				_currentThemeResourceDictionary = dictionaryTheme.ThemeResourceDictionary;
				Resources.MergedDictionaries.Add(_currentThemeResourceDictionary);
			}
			else
			{
				Resources.MergedDictionaries.Add(new ResourceDictionary { Source = newTheme.GetResourceUri() });
			}
		}

		/// <inheritdoc/>
		protected override void OnClosing(CancelEventArgs e)
		{
			// Persist the geometry while the window still has a handle: RestoreBounds is only
			// meaningful before the window is closed.
			PersistBounds();
			base.OnClosing(e);
		}

		/// <inheritdoc/>
		protected override void OnClosed(EventArgs e)
		{
			IsClosed = true;
			_model.PropertyChanged -= OnModelPropertyChanged;
			base.OnClosed(e);
		}

		/// <summary>Applies the geometry remembered on the model, falling back to sensible defaults.</summary>
		private void ApplyModelBounds()
		{
			Width = _model.FloatingWidth > 0d ? _model.FloatingWidth : DefaultDetachedWidth;
			Height = _model.FloatingHeight > 0d ? _model.FloatingHeight : DefaultDetachedHeight;

			if (_model.FloatingLeft != 0d || _model.FloatingTop != 0d)
			{
				WindowStartupLocation = WindowStartupLocation.Manual;
				Left = _model.FloatingLeft;
				Top = _model.FloatingTop;
			}
			else
			{
				WindowStartupLocation = WindowStartupLocation.CenterScreen;
			}
		}

		/// <summary>
		/// Writes the current geometry back onto the model so that detaching the anchorable again
		/// restores the same window position, and so that the values take part in layout serialization.
		/// </summary>
		private void PersistBounds()
		{
			var bounds = WindowState == WindowState.Normal
				? new Rect(Left, Top, ActualWidth, ActualHeight)
				: RestoreBounds;

			if (bounds.IsEmpty || bounds.Width <= 0d || bounds.Height <= 0d) return;

			_model.FloatingLeft = bounds.Left;
			_model.FloatingTop = bounds.Top;
			_model.FloatingWidth = bounds.Width;
			_model.FloatingHeight = bounds.Height;
		}

		/// <summary>Keeps the window caption and icon in sync with the model.</summary>
		/// <param name="sender">The model that raised the change.</param>
		/// <param name="e">The event arguments.</param>
		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(LayoutAnchorable.Title))
			{
				UpdateTitle();
			}
			else if (e.PropertyName == nameof(LayoutAnchorable.IconSource))
			{
				UpdateIcon();
			}
		}

		/// <summary>Copies the title of the model onto the window caption.</summary>
		private void UpdateTitle() => Title = _model.Title ?? string.Empty;

		/// <summary>Copies the icon of the model onto the window.</summary>
		private void UpdateIcon() => Icon = _model.IconSource;
	}
}