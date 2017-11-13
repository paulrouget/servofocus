using Xamarin.Forms;
using System.Diagnostics;
using Xamarin.Forms.Platform.Android;

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
            ServoView.Servo.SetUrlCallback(url => Device.BeginInvokeOnMainThread(() => UrlField.Text = url));

            // more callback setup here.

            ServoView.Servo.MeasureUrlHeight = () => (uint)UrlView.Height;
            
            ServoView.Servo.ValidateCallbacks();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
        }
    }
}