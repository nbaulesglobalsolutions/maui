using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#if WINDOWS
using PlatformApplication = Microsoft.UI.Xaml.Application;
#elif __IOS__ || __MACCATALYST__
using PlatformApplication = UIKit.IUIApplicationDelegate;
#elif __ANDROID__
using PlatformApplication = Android.App.Application;
#elif TIZEN
using PlatformApplication = Tizen.Applications.CoreApplication;
#else
using PlatformApplication = System.Object;
#endif

namespace Microsoft.Maui.Hosting
{
	static class MauiAppBuilderExtensions
	{
		public static MauiAppBuilder ConfigurePlatformProviders(this MauiAppBuilder builder)
		{
			builder.Services.TryAddSingleton<PlatformApplicationProvider>(svc => new PlatformApplicationProvider());

			builder.Services.TryAddSingleton<PlatformApplication>(svc =>
			{
				var provider = svc.GetRequiredService<PlatformApplicationProvider>();
				return provider.PlatformApplication;
			});

			return builder;
		}
	}

	class PlatformApplicationProvider
	{
		PlatformApplication? _platformApplication;

		public PlatformApplication PlatformApplication =>
			_platformApplication ?? throw new InvalidOperationException("No platform application was set on the PlatformApplicationProvider.");

		public void SetApplication(PlatformApplication application) =>
			_platformApplication = application;
	}
}