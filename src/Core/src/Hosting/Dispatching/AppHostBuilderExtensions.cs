using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureDispatching(this MauiAppBuilder builder)
		{
			// register the DispatcherProvider as a singleton for the entire app
			builder.Services.TryAddSingleton<IDispatcherProvider>(svc =>
				// the DispatcherProvider might have already been initialized, so ensure that we are grabbing the
				// Current and putting it in the DI container.
				DispatcherProvider.Current);

			// register the Dispatcher as a singleton for the entire app
			builder.Services.TryAddKeyedSingleton<IDispatcher>(typeof(IApplication), (svc, key) => GetDispatcher(svc));
			// register the initializer so we can init the dispatcher in the app thread for the app
			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMauiInitializeService, DispatcherInitializer>());

			// register the Dispatcher as a scoped service as there may be different dispatchers per window
			builder.Services.TryAddKeyedScoped<IDispatcher>(typeof(IWindow), (svc, key) => GetDispatcher(svc));
			builder.Services.TryAddScoped<IDispatcher>(svc => svc.GetRequiredKeyedService<IDispatcher>(typeof(IWindow)));
			// register the initializer so we can init the dispatcher in the window thread for that window
			builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IMauiInitializeScopedService, ScopeDispatcherInitializer>());

			return builder;
		}

		static IDispatcher GetDispatcher(IServiceProvider services)
		{
			var provider = services.GetRequiredService<IDispatcherProvider>();
			if (DispatcherProvider.SetCurrent(provider))
				services.CreateLogger<Dispatcher>()?.LogWarning("Replaced an existing DispatcherProvider with one from the service provider.");

			return Dispatcher.GetForCurrentThread()!;
		}

		class DispatcherInitializer : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
				_ = services.GetRequiredKeyedService<IDispatcher>(typeof(IApplication));
			}
		}

		class ScopeDispatcherInitializer : IMauiInitializeScopedService
		{
			public void Initialize(IServiceProvider services)
			{
				_ = services.GetRequiredKeyedService<IDispatcher>(typeof(IApplication));
			}
		}
	}
}