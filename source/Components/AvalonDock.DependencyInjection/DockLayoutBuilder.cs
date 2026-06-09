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

		/// <summary>Gets a value indicating whether <see cref="ConfigureToggleDock"/> was called.</summary>
		internal bool ToggleDockConfigured { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DockLayoutBuilder"/> class.
		/// </summary>
		/// <param name="services">The service collection to register services into.</param>
		public DockLayoutBuilder(IServiceCollection services)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
		}

		/// <summary>
		/// Configures <see cref="ToggleDockOptions"/> for the toggle docking manager.
		/// </summary>
		/// <param name="configure">Optional delegate to configure the options.</param>
		/// <returns>This builder for chaining.</returns>
		public DockLayoutBuilder ConfigureToggleDock(Action<ToggleDockOptions>? configure = null)
		{
			var options = new ToggleDockOptions();
			configure?.Invoke(options);
			_services.AddSingleton(options);
			ToggleDockConfigured = true;
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