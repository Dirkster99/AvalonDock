using System;
using AvalonDock.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AvalonDock.DependencyInjection
{
	/// <summary>
	/// Builder for configuring dock layout services, toolboxes, and toggle dock options
	/// through a single <see cref="ServiceCollectionExtensions.AddDockLayoutService(IServiceCollection, Action{DockLayoutBuilder})"/> call.
	/// </summary>
	public class DockLayoutBuilder
	{
		private readonly IServiceCollection _services;

		/// <summary>
		/// The single options object of this builder.
		/// </summary>
		/// <remarks>
		/// There is one instance rather than one per configure method, because
		/// <see cref="ToggleDockOptions"/> is a <see cref="DockingOptions"/>: keeping them apart would
		/// let an application configure two objects and leave the order of the calls deciding which of
		/// them the layout ends up following.
		/// </remarks>
		private readonly ToggleDockOptions _options = new ToggleDockOptions();

		/// <summary>Whether <see cref="_options"/> has already been put into the service collection.</summary>
		private bool _optionsRegistered;

		/// <summary>
		/// Initializes a new instance of the <see cref="DockLayoutBuilder"/> class.
		/// </summary>
		/// <param name="services">The service collection to register services into.</param>
		public DockLayoutBuilder(IServiceCollection services)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
		}

		/// <summary>
		/// Registers the options, so that they can be resolved even when no configure method was called.
		/// </summary>
		internal void RegisterOptions() => EnsureOptionsRegistered();

		/// <summary>
		/// Puts the options into the service collection, once, under both the type the toggle docking
		/// manager asks for and the base type carrying the manager wide switches.
		/// </summary>
		/// <remarks>
		/// Done from the configure methods rather than only from
		/// <see cref="ServiceCollectionExtensions.AddDockLayoutService(IServiceCollection, Action{DockLayoutBuilder})"/>,
		/// because this builder is public: an application may drive it itself, and configuring it has
		/// always been what registered the options.
		/// </remarks>
		private void EnsureOptionsRegistered()
		{
			if (_optionsRegistered) return;
			_optionsRegistered = true;

			_services.AddSingleton(_options);
			_services.AddSingleton<DockingOptions>(_options);
		}

		/// <summary>
		/// Configures the manager wide docking behaviour - whether floating windows and standalone
		/// windows are available at all.
		/// </summary>
		/// <param name="configure">Optional delegate to configure the options.</param>
		/// <returns>This builder for chaining.</returns>
		/// <remarks>
		/// The settings are applied to the layout of <c>IDockLayoutService</c>, so a docking manager
		/// bound to that layout follows them without any further wiring.
		/// </remarks>
		/// <example>
		/// <code>
		/// services.AddDockLayoutService(dock =>
		/// {
		///     dock.ConfigureDocking(o =>
		///     {
		///         o.AllowFloatingWindows = false;
		///         o.AllowDetachedWindows = false;
		///     });
		/// });
		/// </code>
		/// </example>
		public DockLayoutBuilder ConfigureDocking(Action<DockingOptions>? configure = null)
		{
			configure?.Invoke(_options);
			EnsureOptionsRegistered();
			return this;
		}

		/// <summary>
		/// Configures <see cref="ToggleDockOptions"/> for the toggle docking manager.
		/// </summary>
		/// <param name="configure">Optional delegate to configure the options.</param>
		/// <returns>This builder for chaining.</returns>
		public DockLayoutBuilder ConfigureToggleDock(Action<ToggleDockOptions>? configure = null)
		{
			configure?.Invoke(_options);
			EnsureOptionsRegistered();
			return this;
		}

		/// <summary>
		/// Registers a toolbox ViewModel for use with the ToggleDockingManager.
		/// </summary>
		/// <typeparam name="T">The concrete ViewModel type implementing <see cref="IToolbox"/>.</typeparam>
		/// <returns>This builder for chaining.</returns>
		public DockLayoutBuilder AddToolbox<T>()
			where T : class, IToolbox
		{
			_services.AddSingleton<T>();
			_services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<T>());
			return this;
		}

		/// <summary>
		/// Registers a toolbox ViewModel with a factory for use with the ToggleDockingManager.
		/// </summary>
		/// <typeparam name="T">The concrete ViewModel type implementing <see cref="IToolbox"/>.</typeparam>
		/// <param name="factory">Factory to create the ViewModel instance.</param>
		/// <returns>This builder for chaining.</returns>
		public DockLayoutBuilder AddToolbox<T>(Func<IServiceProvider, T> factory)
			where T : class, IToolbox
		{
			_services.AddSingleton<T>(factory);
			_services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<T>());
			return this;
		}
	}
}