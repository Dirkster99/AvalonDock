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
		/// <typeparam name="TSerializer">The concrete serializer type implementing <see cref="IDockSerializer"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddAvalonDockSerializer<TSerializer>(
			this IServiceCollection services)
			where TSerializer : class, IDockSerializer
		{
			services.AddSingleton<IDockSerializer, TSerializer>();
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
		/// The ViewModel must implement <see cref="IToolboxViewModel"/>.
		/// All registered toolboxes are collected via <c>IEnumerable&lt;IToolboxViewModel&gt;</c>.
		/// </summary>
		/// <typeparam name="T">The concrete ViewModel type implementing <see cref="IToolboxViewModel"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddToolbox<T>(this IServiceCollection services)
			where T : class, IToolboxViewModel
		{
			services.AddSingleton<T>();
			services.AddSingleton<IToolboxViewModel>(sp => sp.GetRequiredService<T>());
			return services;
		}

		/// <summary>
		/// Registers a toolbox ViewModel with a factory for use with the ToggleDockingManager.
		/// </summary>
		/// <typeparam name="T">The concrete ViewModel type implementing <see cref="IToolboxViewModel"/>.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="factory">Factory to create the ViewModel instance.</param>
		/// <returns>The service collection for chaining.</returns>
		public static IServiceCollection AddToolbox<T>(
			this IServiceCollection services,
			Func<IServiceProvider, T> factory)
			where T : class, IToolboxViewModel
		{
			services.AddSingleton<T>(factory);
			services.AddSingleton<IToolboxViewModel>(sp => sp.GetRequiredService<T>());
			return services;
		}
	}
}