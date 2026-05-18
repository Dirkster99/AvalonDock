#nullable enable
using System.Collections.Generic;
using System.Linq;
using AvalonDock.Core;
using AvalonDock.DependencyInjection;
using AvalonDock.Mvvm;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AvalonDockTest
{
	internal class DiTestToolbox : ToolboxBase
	{
		public DiTestToolbox()
		{
			Id = "di-test";
			Title = "DI Test";
			Side = ToolboxSide.Left;
		}
	}

	internal class DiTestToolboxWithDeps : ToolboxBase
	{
		public string Dependency { get; }

		public DiTestToolboxWithDeps(string dependency)
		{
			Id = "di-deps";
			Title = "With Deps";
			Dependency = dependency;
		}
	}

	[TestFixture]
	public class ServiceCollectionExtensionsTests
	{
		[Test]
		public void AddToolbox_RegistersSingleton()
		{
			var services = new ServiceCollection();
			services.AddToolbox<DiTestToolbox>();

			var provider = services.BuildServiceProvider();
			var instance1 = provider.GetRequiredService<DiTestToolbox>();
			var instance2 = provider.GetRequiredService<DiTestToolbox>();

			Assert.That(instance1, Is.Not.Null);
			Assert.That(instance1, Is.SameAs(instance2));
		}

		[Test]
		public void AddToolbox_WithFactory_RegistersSingleton()
		{
			var services = new ServiceCollection();
			services.AddToolbox<DiTestToolboxWithDeps>(sp =>
				new DiTestToolboxWithDeps("injected-value"));

			var provider = services.BuildServiceProvider();
			var instance = provider.GetRequiredService<DiTestToolboxWithDeps>();

			Assert.That(instance, Is.Not.Null);
			Assert.That(instance.Dependency, Is.EqualTo("injected-value"));
		}

		[Test]
		public void AddToolbox_WithFactory_IsSingleton()
		{
			var services = new ServiceCollection();
			services.AddToolbox<DiTestToolboxWithDeps>(sp =>
				new DiTestToolboxWithDeps("test"));

			var provider = services.BuildServiceProvider();
			var instance1 = provider.GetRequiredService<DiTestToolboxWithDeps>();
			var instance2 = provider.GetRequiredService<DiTestToolboxWithDeps>();

			Assert.That(instance1, Is.SameAs(instance2));
		}

		[Test]
		public void AddToolbox_CanBeResolvedAsIToolbox()
		{
			var services = new ServiceCollection();
			services.AddToolbox<DiTestToolbox>();
			services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<DiTestToolbox>());

			var provider = services.BuildServiceProvider();
			var toolboxes = provider.GetServices<IToolbox>().ToList();

			Assert.That(toolboxes, Has.Count.EqualTo(1));
			Assert.That(toolboxes[0], Is.InstanceOf<DiTestToolbox>());
		}

		[Test]
		public void AddToolbox_MultipleToolboxes_AllResolvable()
		{
			var services = new ServiceCollection();
			services.AddToolbox<DiTestToolbox>();
			services.AddToolbox<DiTestToolboxWithDeps>(sp => new DiTestToolboxWithDeps("v"));

			services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<DiTestToolbox>());
			services.AddSingleton<IToolbox>(sp => sp.GetRequiredService<DiTestToolboxWithDeps>());

			var provider = services.BuildServiceProvider();
			var toolboxes = provider.GetServices<IToolbox>().ToList();

			Assert.That(toolboxes, Has.Count.EqualTo(2));
		}

		[Test]
		public void AddToggleDockOptions_RegistersOptions()
		{
			var services = new ServiceCollection();
			services.AddToggleDockOptions(opts =>
			{
				opts.ButtonSize = 32;
				opts.DefaultDockWidth = 300;
				opts.DefaultDockHeight = 200;
			});

			var provider = services.BuildServiceProvider();
			var options = provider.GetRequiredService<ToggleDockOptions>();

			Assert.That(options.ButtonSize, Is.EqualTo(32));
			Assert.That(options.DefaultDockWidth, Is.EqualTo(300));
			Assert.That(options.DefaultDockHeight, Is.EqualTo(200));
		}

		[Test]
		public void AddToggleDockOptions_WithoutConfigure_UsesDefaults()
		{
			var services = new ServiceCollection();
			services.AddToggleDockOptions();

			var provider = services.BuildServiceProvider();
			var options = provider.GetRequiredService<ToggleDockOptions>();

			Assert.That(options, Is.Not.Null);
		}

		[Test]
		public void AddToolbox_ReturnsSameServiceCollection_ForChaining()
		{
			var services = new ServiceCollection();
			var result = services.AddToolbox<DiTestToolbox>();
			Assert.That(result, Is.SameAs(services));
		}

		[Test]
		public void AddToggleDockOptions_ReturnsSameServiceCollection_ForChaining()
		{
			var services = new ServiceCollection();
			var result = services.AddToggleDockOptions();
			Assert.That(result, Is.SameAs(services));
		}
	}
}
