using Xamarin.Forms;
using System.Diagnostics;
using System;

namespace Servofocus
{
    public partial class ServofocusPage : ContentPage
    {
        public ServofocusPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Initialize();
            Debug.WriteLine("OnAppearing");
        }

        void Initialize()
        {
            UrlView.TranslateTo(0, 200, 0);
            ServoView.ScaleTo(0, 0);
 
            ServoView.Servo.SetUrlCallback(url => Device.BeginInvokeOnMainThread(() =>
            {
                UrlField.Text = url;
            }));

            ServoView.Servo.SetTitleCallback(title => Device.BeginInvokeOnMainThread(() =>
            {
                // FIXME
            }));

            ServoView.Servo.SetHistoryCallback((back, fwd) => Device.BeginInvokeOnMainThread(() =>
            {
                // FIXME
            }));

            ServoView.Servo.SetLoadStartedCallback(() => Device.BeginInvokeOnMainThread(() =>
            {
                // FIXME

            }));

            ServoView.Servo.SetLoadEndedCallback(() => Device.BeginInvokeOnMainThread(() =>
            {
                // FIXME
                ShowServo();
            }));

            ServoView.Servo.MeasureUrlHeight = () => (uint)UrlView.Height;

            ServoView.Servo.ValidateCallbacks();
        }

        void ShowServo()
        {
            UrlView.TranslateTo(0, 0, 500, Easing.SpringOut);
            ServoView.ScaleTo(1, 500, Easing.SpringOut);
        }

        void HideServo()
        {
            UrlView.TranslateTo(0, 200, 500, Easing.SpringIn);
            ServoView.ScaleTo(0, 500, Easing.SpringIn);
        }

        void EraseButtonClicked(object sender, EventArgs args)
        {
            HideServo();
        }

        void UrlChanged(object sender, EventArgs args)
        {
            ServoView.Servo.LoadUrl(UrlField.Text);
        }

        void UrlFocused(object sender, EventArgs args)
        {
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
        }
    }
}
