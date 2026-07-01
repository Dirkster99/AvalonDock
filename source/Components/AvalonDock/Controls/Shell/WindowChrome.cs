/**************************************************************************\
	Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Windows.Shell
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Windows;
	using System.Windows.Data;
	using Standard;

	/// <summary>
	/// Represents the window Chrome.
	/// </summary>
	public class WindowChrome : Freezable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WindowChrome"/> class.
		/// </summary>
		public WindowChrome()
		{
			// Effective default values for some of these properties are set to be bindings
			// that set them to system defaults.
			// A more correct way to do this would be to Coerce the value iff the source of the DP was the default value.
			// Unfortunately with the current property system we can't detect whether the value being applied at the time
			// of the coersion is the default.
			foreach (var bp in _BoundProperties)
			{
				// This list must be declared after the DP's are assigned.
				Assert.IsNotNull(bp.DependencyProperty);
				BindingOperations.SetBinding(this, bp.DependencyProperty,
					new Binding
					{
						Source = SystemParameters2.Current,
						Path = new PropertyPath(bp.SystemParameterPropertyName),
						Mode = BindingMode.OneWay,
						UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
					});
			}
		}

		/// <summary>
		/// Occurs when property Changed That Requires Repaint.
		/// </summary>
		internal event EventHandler PropertyChangedThatRequiresRepaint;

		/// <summary>Represents the _SystemParameterBoundProperty structure.</summary>
		private struct _SystemParameterBoundProperty
		{
			/// <summary>
			/// Gets or sets the system Parameter Property Name.
			/// </summary>
			public string SystemParameterPropertyName { get; set; }

			/// <summary>
			/// Gets or sets the DependencyProperty value.
			/// </summary>
			public DependencyProperty DependencyProperty { get; set; }
		}

		// Named property available for fully extending the glass frame.

		/// <summary>
		/// Gets the glass Frame Complete Thickness.
		/// </summary>
		public static Thickness GlassFrameCompleteThickness => new Thickness(-1);

		/// <summary>
		/// <see cref="WindowChrome"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty WindowChromeProperty = DependencyProperty.RegisterAttached("WindowChrome", typeof(WindowChrome), typeof(WindowChrome),
			new PropertyMetadata(null, _OnChromeChanged));

		/// <summary>
		/// Executes the on Chrome Changed operation.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The event arguments.</param>
		private static void _OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// The different design tools handle drawing outside their custom window objects differently.
			// Rather than try to support this concept in the design surface let the designer draw its own
			// chrome anyways.
			// There's certainly room for improvement here.
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d)) return;

			var window = (Window)d;
			var newChrome = (WindowChrome)e.NewValue;
			Assert.IsNotNull(window);

			// Update the ChromeWorker with this new object.

			// If there isn't currently a worker associated with the Window then assign a new one.
			// There can be a many:1 relationship of to Window to WindowChrome objects, but a 1:1 for a Window and a WindowChromeWorker.
			var chromeWorker = WindowChromeWorker.GetWindowChromeWorker(window);
			if (chromeWorker == null)
			{
				chromeWorker = new WindowChromeWorker();
				WindowChromeWorker.SetWindowChromeWorker(window, chromeWorker);
			}

			chromeWorker.SetWindowChrome(newChrome);
		}

		/// <summary>
		/// Gets the get Window Chrome.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <returns>The requested value.</returns>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static WindowChrome GetWindowChrome(Window window)
		{
			Verify.IsNotNull(window, nameof(window));
			return (WindowChrome)window.GetValue(WindowChromeProperty);
		}

		/// <summary>
		/// Sets the set Window Chrome.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <param name="chrome">The chrome.</param>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static void SetWindowChrome(Window window, WindowChrome chrome)
		{
			Verify.IsNotNull(window, nameof(window));
			window.SetValue(WindowChromeProperty, chrome);
		}

		/// <summary>
		/// Gets the IsHitTestVisibleInChromeProperty value.
		/// </summary>
		public static readonly DependencyProperty IsHitTestVisibleInChromeProperty = DependencyProperty.RegisterAttached(
			"IsHitTestVisibleInChrome", typeof(bool), typeof(WindowChrome),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

		/// <summary>
		/// Gets the get Is Hit Test Visible In Chrome.
		/// </summary>
		/// <param name="inputElement">The input Element.</param>
		/// <returns>true if the operation succeeds; otherwise, false.</returns>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static bool GetIsHitTestVisibleInChrome(IInputElement inputElement)
		{
			Verify.IsNotNull(inputElement, nameof(inputElement));
			if (!(inputElement is DependencyObject dobj))
				throw new ArgumentException("The element must be a DependencyObject", nameof(inputElement));
			return (bool)dobj.GetValue(IsHitTestVisibleInChromeProperty);
		}

		/// <summary>
		/// Sets the set Is Hit Test Visible In Chrome.
		/// </summary>
		/// <param name="inputElement">The input Element.</param>
		/// <param name="hitTestVisible">The hit Test Visible.</param>
		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static void SetIsHitTestVisibleInChrome(IInputElement inputElement, bool hitTestVisible)
		{
			Verify.IsNotNull(inputElement, nameof(inputElement));
			if (!(inputElement is DependencyObject dobj))
				throw new ArgumentException("The element must be a DependencyObject", nameof(inputElement));
			dobj.SetValue(IsHitTestVisibleInChromeProperty, hitTestVisible);
		}

		/// <summary>
		/// <see cref="CaptionHeight"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register(nameof(CaptionHeight), typeof(double), typeof(WindowChrome),
			new PropertyMetadata(0d, (d, e) => ((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint()), value => (double)value >= 0d);

		/// <summary>
		/// Gets or sets the caption Height.
		/// </summary>
		public double CaptionHeight
		{
			get => (double)GetValue(CaptionHeightProperty);
			set => SetValue(CaptionHeightProperty, value);
		}

		/// <summary>
		/// <see cref="ResizeBorderThickness"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ResizeBorderThicknessProperty = DependencyProperty.Register(nameof(ResizeBorderThickness), typeof(Thickness), typeof(WindowChrome),
			new PropertyMetadata(default(Thickness)), (value) => Utility.IsThicknessNonNegative((Thickness)value));

		/// <summary>
		/// Gets or sets the resize Border Thickness.
		/// </summary>
		public Thickness ResizeBorderThickness
		{
			get => (Thickness)GetValue(ResizeBorderThicknessProperty);
			set => SetValue(ResizeBorderThicknessProperty, value);
		}

		/// <summary>
		/// <see cref="GlassFrameThickness"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty GlassFrameThicknessProperty = DependencyProperty.Register(nameof(GlassFrameThickness), typeof(Thickness), typeof(WindowChrome),
			new PropertyMetadata(default(Thickness), (d, e) => ((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint(), (d, o) => _CoerceGlassFrameThickness((Thickness)o)));

		/// <summary>
		/// Executes the coerce Glass Frame Thickness operation.
		/// </summary>
		/// <param name="thickness">The thickness.</param>
		/// <returns>The result of the operation.</returns>
		private static object _CoerceGlassFrameThickness(Thickness thickness)
		{
			// If it's explicitly set, but set to a thickness with at least one negative side then
			// coerce the value to the stock GlassFrameCompleteThickness.
			return !Utility.IsThicknessNonNegative(thickness) ? GlassFrameCompleteThickness : thickness;
		}

		/// <summary>
		/// Gets or sets the glass Frame Thickness.
		/// </summary>
		public Thickness GlassFrameThickness
		{
			get => (Thickness)GetValue(GlassFrameThicknessProperty);
			set => SetValue(GlassFrameThicknessProperty, value);
		}

		/// <summary>
		/// <see cref="CornerRadius"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(WindowChrome),
			new PropertyMetadata(default(CornerRadius), (d, e) => ((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint()), (value) => Utility.IsCornerRadiusValid((CornerRadius)value));

		/// <summary>
		/// Gets or sets the corner Radius.
		/// </summary>
		public CornerRadius CornerRadius
		{
			get => (CornerRadius)GetValue(CornerRadiusProperty);
			set => SetValue(CornerRadiusProperty, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether show System Menu.
		/// </summary>
		public bool ShowSystemMenu { get; set; }

		/// <inheritdoc/>
		protected override Freezable CreateInstanceCore()
		{
			return new WindowChrome();
		}

		/// <summary>
		/// The bound Properties field.
		/// </summary>
		private static readonly List<_SystemParameterBoundProperty> _BoundProperties = new List<_SystemParameterBoundProperty>
		{
			new _SystemParameterBoundProperty { DependencyProperty = CornerRadiusProperty, SystemParameterPropertyName = "WindowCornerRadius" },
			new _SystemParameterBoundProperty { DependencyProperty = CaptionHeightProperty, SystemParameterPropertyName = "WindowCaptionHeight" },
			new _SystemParameterBoundProperty { DependencyProperty = ResizeBorderThicknessProperty, SystemParameterPropertyName = "WindowResizeBorderThickness" },
			new _SystemParameterBoundProperty { DependencyProperty = GlassFrameThicknessProperty, SystemParameterPropertyName = "WindowNonClientFrameThickness" },
		};

		/// <summary>
		/// Executes the on Property Changed That Requires Repaint operation.
		/// </summary>
		private void _OnPropertyChangedThatRequiresRepaint() => PropertyChangedThatRequiresRepaint?.Invoke(this, EventArgs.Empty);
	}
}