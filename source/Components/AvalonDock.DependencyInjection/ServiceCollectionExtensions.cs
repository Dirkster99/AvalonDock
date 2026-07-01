using System;
using System.Collections.Generic;
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

		/// <summary>
		/// Registers the <see cref="IDockLayoutService"/> which auto-builds the MVVM layout tree
		/// from all registered <see cref="IToolbox"/> instances, using a builder to configure
		/// toolboxes and toggle dock options in a single call.
		/// </summary>
		/// <param name="services">The service collection.</param>
		/// <param name="configure">Delegate to configure the dock layout via <see cref="DockLayoutBuilder"/>.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddDockLayoutService(
			this IServiceCollection services,
			Action<DockLayoutBuilder> configure)
		{
			var builder = new DockLayoutBuilder(services);
			configure(builder);

			if (!builder.ToggleDockConfigured)
			{
				services.AddSingleton(new ToggleDockOptions());
			}

			services.AddSingleton<IDockLayoutService>(sp =>
			{
				var toolboxes = sp.GetService<IEnumerable<IToolbox>>()
					?? System.Array.Empty<IToolbox>();
				return new Mvvm.DockLayoutService(toolboxes);
			});
			services.AddSingleton<Mvvm.SideToggleManager>();
			return services;
		}

		/// <summary>
		/// Registers the <see cref="IDockLayoutService"/> which auto-builds the MVVM layout tree
		/// from all registered <see cref="IToolbox"/> instances.
		/// </summary>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddDockLayoutService(this IServiceCollection services)
		{
			services.AddSingleton<IDockLayoutService>(sp =>
			{
				var toolboxes = sp.GetService<IEnumerable<IToolbox>>()
					?? System.Array.Empty<IToolbox>();
				return new Mvvm.DockLayoutService(toolboxes);
			});
			services.AddSingleton<Mvvm.SideToggleManager>();
			return services;
		}

		/// <summary>
		/// Registers the <see cref="IDockLayoutService"/> with a custom implementation.
		/// </summary>
		/// <typeparam name="TService">The concrete type implementing <see cref="IDockLayoutService"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddDockLayoutService<TService>(this IServiceCollection services)
			where TService : class, IDockLayoutService
		{
			services.AddSingleton<IDockLayoutService, TService>();
			return services;
		}
	}
}