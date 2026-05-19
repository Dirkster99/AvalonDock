using System;
using AvalonDock.Core;
using Microsoft.Extensions.DependencyInjection;
namespace AvalonDock.DependencyInjection
{
	/// <summary>
	/// Extension methods for registering AvalonDock services with Microsoft.Extensions.DependencyInjection.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Registers the AvalonDock factory and state services.
		/// </summary>
		/// <typeparam name="TFactory">The concrete factory type implementing <see cref="IFactory"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="lifetime">Service lifetime for the factory (default: Singleton).</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddAvalonDock<TFactory>(
			this IServiceCollection services,
			ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TFactory : class, IFactory
		{
			services.Add(ServiceDescriptor.Describe(typeof(TFactory), typeof(TFactory), lifetime));
			services.Add(ServiceDescriptor.Describe(
				typeof(IFactory),
				sp => sp.GetRequiredService<TFactory>(),
				lifetime));

			return services;
		}

		/// <summary>
		/// Registers a dock serializer implementation.
		/// </summary>
		/// <typeparam name="TSerializer">The concrete serializer type implementing <see cref="ILayoutSerializer"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddAvalonDockSerializer<TSerializer>(
			this IServiceCollection services)
			where TSerializer : class, ILayoutSerializer
		{
			services.AddSingleton<ILayoutSerializer, TSerializer>();
			return services;
		}

		/// <summary>
		/// Registers a dock state manager.
		/// </summary>
		/// <typeparam name="TState">The concrete state type implementing <see cref="IDockState"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddAvalonDockState<TState>(
			this IServiceCollection services)
			where TState : class, IDockState
		{
			services.AddSingleton<IDockState, TState>();
			return services;
		}

		/// <summary>
		/// Registers <see cref="ToggleDockOptions"/> with an optional configuration delegate.
		/// Resolve the options in your window to call <see cref="ToggleDockOptions.ApplyTo"/>.
		/// </summary>
		/// <param name="services">The service collection.</param>
		/// <param name="configure">Optional delegate to configure the options.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddToggleDockOptions(
			this IServiceCollection services,
			Action<ToggleDockOptions>? configure = null)
		{
			var options = new ToggleDockOptions();
			configure?.Invoke(options);
			services.AddSingleton(options);
			return services;
		}

		/// <summary>
		/// Registers a toolbox ViewModel for use with the ToggleDockingManager.
		/// The ViewModel should implement <c>AvalonDock.Core.IToolbox</c>.
		/// All registered toolboxes are collected via <c>IEnumerable&lt;IToolboxRegistration&gt;</c>.
		/// </summary>
		/// <typeparam name="T">The concrete ViewModel type.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddToolbox<T>(this IServiceCollection services)
			where T : class, IToolbox
		{
			services.AddSingleton<T>();
			return services;
		}

		/// <summary>
		/// Registers a toolbox ViewModel with a factory for use with the ToggleDockingManager.
		/// </summary>
		/// <typeparam name="T">The concrete ViewModel type.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="factory">Factory to create the ViewModel instance.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddToolbox<T>(
			this IServiceCollection services,
			Func<IServiceProvider, T> factory)
			where T : class, IToolbox
		{
			services.AddSingleton<T>(factory);
			return services;
		}

		/// <summary>
		/// Registers a theme manager implementation.
		/// </summary>
		/// <typeparam name="TThemeManager">The concrete type implementing <see cref="IThemeManager"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddAvalonDockThemeManager<TThemeManager>(
			this IServiceCollection services)
			where TThemeManager : class, IThemeManager
		{
			services.AddSingleton<TThemeManager>();
			services.AddSingleton<IThemeManager>(sp => sp.GetRequiredService<TThemeManager>());
			return services;
		}

		/// <summary>
		/// Registers a docking manager implementation (e.g. for ViewModel access via <see cref="IDockingManager"/>).
		/// Typically registered after the WPF DockingManager is created.
		/// </summary>
		/// <param name="services">The service collection.</param>
		/// <param name="factory">Factory to create or resolve the IDockingManager instance.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddDockingManager(
			this IServiceCollection services,
			Func<IServiceProvider, IDockingManager> factory)
		{
			services.AddSingleton(factory);
			return services;
		}

		/// <summary>
		/// Registers an auto-hide manager implementation.
		/// </summary>
		/// <typeparam name="TAutoHide">The concrete type implementing <see cref="IAutoHideManager"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddAutoHideManager<TAutoHide>(
			this IServiceCollection services)
			where TAutoHide : class, IAutoHideManager
		{
			services.AddSingleton<IAutoHideManager, TAutoHide>();
			return services;
		}

		/// <summary>
		/// Registers a floating window service implementation.
		/// </summary>
		/// <typeparam name="TFloating">The concrete type implementing <see cref="IFloatingWindowService"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddFloatingWindowService<TFloating>(
			this IServiceCollection services)
			where TFloating : class, IFloatingWindowService
		{
			services.AddSingleton<IFloatingWindowService, TFloating>();
			return services;
		}

		/// <summary>
		/// Registers a drag-and-drop handler implementation.
		/// </summary>
		/// <typeparam name="TDragDrop">The concrete type implementing <see cref="IDragDropHandler"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddDragDropHandler<TDragDrop>(
			this IServiceCollection services)
			where TDragDrop : class, IDragDropHandler
		{
			services.AddSingleton<IDragDropHandler, TDragDrop>();
			return services;
		}
	}
}