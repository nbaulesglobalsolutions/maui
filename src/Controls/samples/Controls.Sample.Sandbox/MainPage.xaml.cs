using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		static FlyoutPage _flyoutPage;
		static ContentPage _mainPage = new MainPage();
		static Window _window;
		public MainPage()
		{
			InitializeComponent();
		}

		private async void Button_Clicked(object sender, EventArgs e)
		{
			await Task.Delay(2000);
			_window ??= Window;

			if (_flyoutPage is null)
				_flyoutPage = _window.Page as FlyoutPage;

			if (_window.Page is FlyoutPage)
				_window.Page = _mainPage;
			else
			{
				_window.Page = _flyoutPage;
				await Task.Yield();
				_window.Page = _mainPage;
				await Task.Yield();
				_window.Page = _flyoutPage;
				await Task.Yield();
				await _flyoutPage.Detail.Navigation.PushAsync(new MainPage());
				await Task.Yield();
				_window.Page = new TabbedPage() { Children = { new NavigationPage(new MainPage()) } };
				await (_window.Page as TabbedPage).Children[0].Navigation.PushAsync(new MainPage());
			}

			//if (Navigation.NavigationStack.Count > 1)
			//{
			//	await Navigation.PopAsync();
			//}
			//else
			//{
			//	await Navigation.PushAsync(new TabbedPage() { Children = { new MainPage() } });
			//	await Navigation.PushAsync(new TabbedPage() { Children = { new MainPage() } });
			//	await Navigation.PushAsync(new TabbedPage() { Children = { new MainPage() } });
			//	await Navigation.PopAsync();
			//	//await Navigation.PopAsync();
			//	//await Navigation.PopAsync();
			//}
		}
	}
}