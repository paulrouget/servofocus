using Xamarin.Forms;
using System.Diagnostics;
using System;

namespace Servofocus
{
    public partial class ServofocusPage : ContentPage
    {
        string _url;
        bool _loading;
        bool _canGoBack;
        const string HttpsScheme = "https://";

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
            ServoView.Servo.SetLogCallback(log =>
            {
                // Debug.WriteLine("SERVO: " + log);
            });

            ServoView.Servo.SetUrlCallback(url => Device.BeginInvokeOnMainThread(() =>
            {
                UrlField.Text = url;
                _url = url;
                UpdateStatus();
            }));

            this.MenuButton.Reload = () => ServoView.Servo.Reload();
            this.MenuButton.GoForward = () => ServoView.Servo.GoForward();

            this.ServoView.Servo.SetTitleCallback(title => Device.BeginInvokeOnMainThread(() =>
            {
            }));

            ServoView.Servo.SetHistoryCallback((back, fwd) => Device.BeginInvokeOnMainThread(() =>
            {
                _canGoBack = back;
            }));

            ServoView.Servo.SetLoadStartedCallback(() => Device.BeginInvokeOnMainThread(() =>
            {
                _loading = true;
                UpdateStatus();
            }));

            ServoView.Servo.SetLoadEndedCallback(() => Device.BeginInvokeOnMainThread(() =>
            {
                _loading = false;
                UpdateStatus();
            }));

            // FIXME: hidpi
            if (Device.RuntimePlatform == Device.macOS)
            {
                ServoView.Servo.SetSize(2 * (uint)ServoView.Bounds.Width, 2 * (uint)ServoView.Bounds.Height);
                ServoView.Servo.SetResourcePath("/tmp/servo/resources/");
                ServoView.Servo.ValidateCallbacks();
                ServoView.Servo.InitWithGL();
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                ServoView.Servo.SetSize(600, 1000);
                ServoView.Servo.SetResourcePath("/sdcard/servo/resources/");
                ServoView.Servo.ValidateCallbacks();
                // InitWithEGL called in renderer
            }

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

        void EraseButtonClicked(object sender, EventArgs args)
        {
            HideServo();
        }

        void UpdateStatus()
        {
            SslIcon.IsVisible = !_loading && _url.StartsWith(HttpsScheme);
            Throbber.IsVisible = _loading;
        }

        void UrlChanged(object sender, EventArgs args)
        {
            var url = UrlField.Text;
            if (string.IsNullOrEmpty(url) || url == _url)
            {
                return;
            }
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                if (url.Contains(".") && Uri.IsWellFormedUriString(HttpsScheme + url, UriKind.Absolute))
                {
                    url = HttpsScheme + url;
                }
                else
                {
                    url = $"{HttpsScheme}duckduckgo.com/html/?q=" + url;
                } 
               
            }
            _url = url;
            UrlField.Text = url;
            ServoView.Servo.LoadUrl(url);
            ShowServo();
        }

        void UrlFocused(object sender, EventArgs args)
        {
        }

        public bool SystemGoBack()
        {
            if (_canGoBack)
            {
                ServoView.Servo.GoBack();
                return true;
            
            }
            return false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
        }
    }
}
