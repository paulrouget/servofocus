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
            _viewModel = (MainViewModel) BindingContext;
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
                if(_viewModel.ServoVisibility)
                    ShowServo();
                else HideServo();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.Initialize((uint)ServoView.Bounds.Width, (uint)ServoView.Bounds.Height);
            
            Debug.WriteLine("OnAppearing");
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
            //EraseButton.TranslateTo(0, 0, delay, Easing.Linear);
            UrlField.TranslateTo(0, 0, delay, Easing.Linear);
            StatusView.ScaleTo(1, delay, Easing.Linear);
        }

        void HideServo(bool immediate = false)
        {
            uint delay = 500;
            var deviceFactor = 1;

            if (immediate)
            {
                delay = 0;
            }

            if (Device.RuntimePlatform == Device.macOS)
            {
                deviceFactor = -1;
            }

            UrlView.TranslateTo(0, deviceFactor * 0.5 *  ServoView.Bounds.Height, delay, Easing.SpringIn);
            ServoView.TranslateTo(0, deviceFactor * ServoView.Bounds.Height, delay, Easing.SpringIn);
            //UrlField.TranslateTo(30, 0, delay, Easing.Linear);
            StatusView.ScaleTo(0, delay, Easing.Linear);

            // UrlField.Focus();
        }

        void OnErase(object sender, EventArgs args)
        {
            HideServo();
        }
        
        void LoadUrl(object sender, EventArgs args)
        {
            _viewModel.LoadCurrentUrl();
            
            ShowServo();
        }

        void UrlFocused(object sender, EventArgs args)
        {
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
