using Xamarin.Forms;
using System.Diagnostics;
using System;

namespace Servofocus
{
    public partial class ServofocusPage : ContentPage
    {
        private string _url;
        private bool _loading;
        private bool _canGoBack;

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
                //Debug.WriteLine("SERVO: " + log);
            });

            ServoView.Servo.SetUrlCallback(url => Device.BeginInvokeOnMainThread(() =>
            {
                if (url == "about:blank")
                {
                    UrlField.Text = "";
                }
                else
                {
                    UrlField.Text = url;
                }
                _url = url;
                UpdateStatus();
            }));

            ServoView.Servo.SetTitleCallback(title => Device.BeginInvokeOnMainThread(() =>
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

            ServoView.Servo.SetSize(2 * (uint)ServoView.Bounds.Width, 2 * (uint)ServoView.Bounds.Height);

            ServoView.Servo.ValidateCallbacks();
            ServoView.Servo.InitWithGL();
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
            EraseButton.TranslateTo(0, 0, delay, Easing.Linear);
            UrlField.TranslateTo(0, 0, delay, Easing.Linear);
            StatusView.ScaleTo(1, delay, Easing.Linear);

        }

        void HideServo(bool immediate = false)
        {
            uint delay = 500;
            if (immediate)
            {
                delay = 0;
            }
            UrlView.TranslateTo(0, 100, delay, Easing.SpringIn);
            ServoView.TranslateTo(0, 500, delay, Easing.SpringIn);
            EraseButton.TranslateTo(400, 0, delay, Easing.Linear);
            UrlField.TranslateTo(30, 0, delay, Easing.Linear);
            StatusView.ScaleTo(0, delay, Easing.Linear);


            UrlField.Focus();
        }

        void EraseButtonClicked(object sender, EventArgs args)
        {
            HideServo();
        }

        void UpdateStatus()
        {
            SslIcon.IsVisible = !_loading && _url.StartsWith("https://");
            Throbber.IsVisible = _loading;
        }

        void UrlChanged(object sender, EventArgs args)
        {
            var url = UrlField.Text;
            if (url == null)
            {
                return;
            }
            ShowServo();
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                if (url.Contains(".") && Uri.IsWellFormedUriString("https://" + url, UriKind.Absolute))
                {
                    url = "https://" + url;
                }
                else
                {
                    url = "https://duckduckgo.com/html/?q=" + url;
                }
            }
            ServoView.Servo.LoadUrl(url);
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
            else
            {
                return false;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
        }
    }
}
