using Xamarin.Forms;
using System.Diagnostics;

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
            ServoView.Servo.SetUrlCallback(url => Device.BeginInvokeOnMainThread(() => {
                UrlField.Text = url;
            }));

            ServoView.Servo.SetTitleCallback(title => Device.BeginInvokeOnMainThread(() => {
                // FIXME
            }));

            ServoView.Servo.SetHistoryCallback((back, fwd) => Device.BeginInvokeOnMainThread(() => {
                // FIXME
            }));

            ServoView.Servo.SetLoadStartedCallback(() => Device.BeginInvokeOnMainThread(() => {
                // FIXME
            }));

            ServoView.Servo.SetLoadEndedCallback(() => Device.BeginInvokeOnMainThread(() => {
                // FIXME
            }));

            ServoView.Servo.ValidateCallbacks();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
        }
    }
}