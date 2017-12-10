using Xamarin.Forms;
using System.Diagnostics;
using System;
using System.ComponentModel;

namespace Servofocus
{
    public partial class ServofocusPage : ContentPage
    {
        readonly MainViewModel _viewModel;
        public ServofocusPage()
        {
            InitializeComponent();
            _viewModel = (MainViewModel)BindingContext;
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(_viewModel.FloatingButtonVisibility))
            {
                EraseFloatingButton.ScaleTo(_viewModel.FloatingButtonVisibility ? 1 : 0, easing: Easing.Linear);
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(_viewModel.ServoVisibility))
            {
                if (_viewModel.ServoVisibility)
                    ShowServo();
                else HideServo();
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(_viewModel.UrlFocused))
            {
                var black = Color.FromRgba(0, 0, 0, 100);
                UrlBackground.BackgroundColor = _viewModel.UrlFocused ? black : Color.Transparent;
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(_viewModel.CanGoBack))
            {
                BackButton.Scale = _viewModel.CanGoBack ? 1 : 0;
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(_viewModel.ToolbarOffset))
            {
                var deviceFactor = -1;
                if (Device.RuntimePlatform == Device.macOS)
                {
                    deviceFactor = 1;
                }
                MainStackLayout.TranslateTo(0, deviceFactor * _viewModel.ToolbarOffset);
                float factor = (float)_viewModel.ToolbarOffset / (float)_viewModel.ToolbarHeight;
                UrlView.FadeTo(1 - 3 * factor);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            AddScrollableOffset();

            _viewModel.Initialize((uint)ServoView.Bounds.Width, (uint)ServoView.Bounds.Height);
            _viewModel.ToolbarHeight = (int)ServoView.Bounds.Top;

            LayoutChanged += OnLayoutChanged;
        }

        void AddScrollableOffset()
        {
            if (Device.RuntimePlatform != Device.macOS)
            {
                var cr = Content.Bounds;
                cr.Height += ServoView.Bounds.Top;
                MainStackLayout.Layout(cr);
            }
        }

        private void OnLayoutChanged(object sender, EventArgs e)
        {
            _viewModel.ToolbarHeight = (int)ServoView.Bounds.Top;
            AddScrollableOffset();
        }

        void ShowServo(bool immediate = false)
        {
            uint delay = 500;
            if (immediate)
            {
                delay = 0;
            }
            UrlView.TranslateTo(0, 0, delay, Easing.SpringOut);
            ServoView.TranslateTo(0, 0, delay, Easing.SpringOut);
            StatusView.ScaleTo(1, delay, Easing.Linear);
            _viewModel.ToolbarHeight = (int)ServoView.Bounds.Top;
        }

        void HideServo(bool immediate = false)
        {
            uint delay = 500;
            var deviceFactor = 1;
            var offset = 0.25;

            if (immediate)
            {
                delay = 0;
            }

            if (Device.RuntimePlatform == Device.macOS)
            {
                deviceFactor = -1;
                offset = 0.5;
            }

            UrlView.TranslateTo(0, deviceFactor * offset * ServoView.Bounds.Height, delay, Easing.SpringIn);
            ServoView.TranslateTo(0, deviceFactor * ServoView.Bounds.Height, delay, Easing.SpringIn);
            StatusView.ScaleTo(0, delay, Easing.Linear);
        }

        async void OnErase(object sender, EventArgs args)
        {
            await _viewModel.Erase();
        }

        // The way the focus is bound is messy.
        // We should be relying on the IsFocused bindable property,
        // but it's buggy on Mac. And we get Unfocus too often on Mac
        // too.

        void OnUrlFocused(object sender, EventArgs args)
        {
            _viewModel.UrlFocused = true;
        }

        void OnUrlUnfocused(object sender, EventArgs args)
        {
            if (_viewModel.IsAndroid)
            {
                _viewModel.UrlFocused = false;
            }
        }

        void LoadUrl(object sender, EventArgs args)
        {
            UrlView.Unfocus();
            _viewModel.UrlFocused = false;

            _viewModel.LoadCurrentUrl();
            ShowServo();
        }

        public bool SystemGoBack()
        {
            if (!_viewModel.CanGoBack) return false;
            _viewModel.GoBack();
            return true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
        }
    }
}
