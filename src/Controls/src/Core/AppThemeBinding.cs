#nullable disable
using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace Microsoft.Maui.Controls
{
	class AppThemeBinding : BindingBase
	{
		WeakReference<BindableObject> _weakTarget;
		BindableProperty _targetProperty;
		bool _attached;
		SetterSpecificity specificity;

		internal override BindingBase Clone()
		{
			var clone = new AppThemeBinding
			{
				Light = Light,
				_isLightSet = _isLightSet,
				Dark = Dark,
				_isDarkSet = _isDarkSet,
				Default = Default
			};

			if (DebuggerHelper.DebuggerIsAttached && VisualDiagnostics.GetSourceInfo(this) is SourceInfo info)
				VisualDiagnostics.RegisterSourceInfo(clone, info.SourceUri, info.LineNumber, info.LinePosition);

			return clone;
		}

		internal override void Apply(bool fromTarget)
		{
			base.Apply(fromTarget);
			ApplyCore();
			SetAttached(true);
		}

		internal override void Apply(object context, BindableObject bindObj, BindableProperty targetProperty, bool fromBindingContextChanged, SetterSpecificity specificity)
		{
			_weakTarget = new WeakReference<BindableObject>(bindObj);
			_targetProperty = targetProperty;
			base.Apply(context, bindObj, targetProperty, fromBindingContextChanged, specificity);
			this.specificity = specificity;
			ApplyCore(false);
			SetAttached(true);
		}

		internal override void Unapply(bool fromBindingContextChanged = false)
		{
			SetAttached(false);
			base.Unapply(fromBindingContextChanged);
			_weakTarget = null;
			_targetProperty = null;
		}

		void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
			=> ApplyCore(true);

		void OnRequestedThemeChanged(object sender, EventArgs e)
			=> ApplyCore(true);

		void ApplyCore(bool dispatch = false)
		{
			if (_weakTarget == null || !_weakTarget.TryGetTarget(out var target))
			{
				SetAttached(false);
				return;
			}

			if (dispatch)
				target.Dispatcher.DispatchIfRequired(Set);
			else
				Set();

			void Set()
			{
				var value = GetValue();
				if (value is DynamicResource dynamicResource)
					target.SetDynamicResource(_targetProperty, dynamicResource.Key, specificity);
				else
				{
					if (!BindingExpression.TryConvert(ref value, _targetProperty, _targetProperty.ReturnType, true))
					{
						BindingDiagnostics.SendBindingFailure(this, null, target, _targetProperty, "AppThemeBinding", BindingExpression.CannotConvertTypeErrorMessage, value, _targetProperty.ReturnType);
						return;
					}
					target.SetValueCore(_targetProperty, value, Internals.SetValueFlags.ClearDynamicResource, BindableObject.SetValuePrivateFlags.Default | BindableObject.SetValuePrivateFlags.Converted, specificity);
				}
			};
		}

		object _light;
		object _dark;
		bool _isLightSet;
		bool _isDarkSet;

		public object Light
		{
			get => _light;
			set
			{
				_light = value;
				_isLightSet = true;
			}
		}

		public object Dark
		{
			get => _dark;
			set
			{
				_dark = value;
				_isDarkSet = true;
			}
		}

		public object Default { get; set; }

		object GetValue()
		{
			// try use the theme from the parent
			var appTheme = AppTheme.Unspecified;
			if (_weakTarget?.TryGetTarget(out var target) == true && target is VisualElement ve)
				appTheme = ve.RequestedTheme;

			// if no parent theme, try the app and then just ask the OS
			if (appTheme == AppTheme.Unspecified)
				appTheme = Application.Current?.RequestedTheme ?? AppInfo.RequestedTheme;

			return appTheme switch
			{
				AppTheme.Dark => _isDarkSet ? Dark : Default,
				_ => _isLightSet ? Light : Default,
			};
		}

		void SetAttached(bool value)
		{
			if (_attached == value)
				return;

			_attached = value;

			if (_weakTarget?.TryGetTarget(out var target) == true && target is VisualElement ve)
			{
				// use the VisualElement as this is faster

				if (value)
					ve.RequestedThemeChanged += OnRequestedThemeChanged;
				else
					ve.RequestedThemeChanged -= OnRequestedThemeChanged;
			}
			else
			{
				// fall back to the app

				var app = Application.Current;
				if (value)
					app.RequestedThemeChanged += OnRequestedThemeChanged;
				else
					app.RequestedThemeChanged -= OnRequestedThemeChanged;
			}
		}
	}
}
