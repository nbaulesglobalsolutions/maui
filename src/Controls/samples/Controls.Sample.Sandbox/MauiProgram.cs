using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp() =>
			MauiApp
				.CreateBuilder()
				.UseMauiMaps()
				.UseMauiApp<App>()
				.Build();
	}

	class App : Application
	{
		Window _window;
		protected override Window CreateWindow(IActivationState activationState)
		{
			// To test shell scenarios, change this to true
			bool useShell = false;

			if (!useShell)
			{
				//return new Window(new NavigationPage(new MainPage()) { Title = "what" });
				return _window ??= new Window(new FlyoutPage()
				{
					Flyout = new ContentPage()
					{
						Title = "rabbit"
					},
					Detail = new NavigationPage(new MainPage()) { Title = "what" }
				});
			}
			else
			{
				return _window ??= new Window(new SandboxShell());
			}
		}
	}
}