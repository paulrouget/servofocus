using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using ServoSharp;
using Xamarin.Forms;

namespace Servofocus
{
    public class MainViewModel : INotifyPropertyChanged
    {
        int _cumulativeDy;
        const string HttpsScheme = "https://";
        string _url;
        bool _canGoBack;
        bool _canGoForward;
        readonly Servo _servo;
        public uint Width;
        public uint Height;
        const string ResourcePathSuffix = "/servo/resources/";
        const string ResouceAndroidPrefix = "/sdcard/";
        const string ResouceMacPrefix = "/tmp/";

        public MainViewModel()
        {
            _servo = new Servo();
        }

        #region Servo calls

        public void Click(uint x, uint y)
        {
            _servo.Click(x, y);
        }

        public void SetHostCallbacks(Action<Action> wakeUp, Action flush)
        {
            _servo.SetHostCallbacks(wakeUp, flush);
        }

        public void Resize(uint height, uint width)
        {
            _servo.Resize(height, width);
        }

        public void Scroll(int dx, int dy, uint x, uint y, ScrollState state)
        {
            if (IsAndroid)
            {
                _cumulativeDy += dy;

                if (_cumulativeDy > 0)
                {
                    // scroll up
                    FloatingButtonVisibility = true;
                }
                else if (_cumulativeDy < 0)
                {
                    // scroll down
                    FloatingButtonVisibility = false;
                }

                if (state == ScrollState.End)
                    _cumulativeDy = 0;
            }

            //Debug.WriteLine($"cumulative DY: {_cumulativeDy}");
            _servo.Scroll(dx, dy, x, y, state);
        }

        public void Reload()
        {
            _servo.Reload();
        }

        public void GoForward()
        {
            _servo.GoForward();
        }

        public void GoBack()
        {
            _servo.GoBack();
        }

        public void LoadCurrentUrl()
        {
            if (string.IsNullOrEmpty(_url)) return;
            if (Url == _lastUrl) return;

            if (!Uri.IsWellFormedUriString(_url, UriKind.Absolute))
            {
                if (_url.Contains(".") && Uri.IsWellFormedUriString(HttpsScheme + _url, UriKind.Absolute))
                {
                    Url = HttpsScheme + _url;
                }
                else
                {
                    Url = $"{HttpsScheme}duckduckgo.com/html/?q=" + _url;
                }
            }

            _servo.LoadUrl(Url);

            ServoVisibility = true;
        }

        public void InitWithEgl()
        {
            _servo.InitWithEgl();
        }

        public void Stop()
        {
            _servo.Stop();
        }

        #endregion
       
        public bool IsAndroid => Device.RuntimePlatform == Device.Android;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        bool _loading;

        public bool IsLoading
        {
            get => _loading;
            set
            {
                _loading = value;
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(CanStop));
                OnPropertyChanged(nameof(CanGoForward));
                OnPropertyChanged(nameof(CanReload));
            }
        }

        bool _floatingButtonVisibility;

        public bool FloatingButtonVisibility
        {
            get => _floatingButtonVisibility;
            set
            {
                _floatingButtonVisibility = value;
                _cumulativeDy = 0;
                OnPropertyChanged(nameof(FloatingButtonVisibility));
            } 
        }

        public bool CanStop => IsLoading;
        public bool CanReload => !IsLoading;

        public bool CanGoBack
        {
            get => _canGoBack;
            set
            {
                _canGoBack = value;
                OnPropertyChanged(nameof(CanGoBack));
            }
        }

        public bool CanGoForward
        {
            get => _canGoForward;
            set
            {
                _canGoForward = value;
                OnPropertyChanged(nameof(CanGoForward));
            }
        }

        public void Initialize(uint width, uint height)
        {
            Width = width;
            Height = height;

            // FIXME: hidpi
            _servo.SetSize(2 * Width, 2 * Height);

            SetupServoCallbacks();
            
            if (Device.RuntimePlatform == Device.Android)
                InitializeAndroid();
            else if(Device.RuntimePlatform == Device.macOS)
                InitializeMacOS();
        }

        void SetupServoCallbacks()
        {
            _servo.SetLogCallback(log =>
            {
                // Debug.WriteLine("SERVO: " + log);
            });

            _servo.SetTitleCallback(title => Device.BeginInvokeOnMainThread(() =>
            {
            }));

            _servo.SetHistoryCallback((back, fwd) => Device.BeginInvokeOnMainThread(() =>
            {
                CanGoBack = back;
                CanGoForward = fwd;
            }));

            _servo.SetLoadStartedCallback(() => Device.BeginInvokeOnMainThread(() =>
            {
                IsLoading = true;
                OnPropertyChanged(string.Empty);
            }));

            _servo.SetLoadEndedCallback(() => Device.BeginInvokeOnMainThread(() =>
            {
                IsLoading = false;
                OnPropertyChanged(string.Empty);
            }));

            _servo.SetUrlCallback(url => Device.BeginInvokeOnMainThread(() =>
            {
                _lastUrl = url;
                Url = url;
                OnPropertyChanged(string.Empty);
            }));
        }

        void InitializeAndroid()
        {
            _servo.SetResourcePath($"{ResouceAndroidPrefix}{ResourcePathSuffix}");
            _servo.ValidateCallbacks();
            // InitWithEGL called in renderer
        }

        void InitializeMacOS()
        {
            _servo.SetResourcePath($"{ResouceMacPrefix}{ResourcePathSuffix}");
            _servo.ValidateCallbacks();
            _servo.InitWithGL();
        }

        public bool SslIconVisibility => !IsLoading && IsHttps;

        public bool IsHttps => Url != null && Url.StartsWith(HttpsScheme);

        public string Url
        {
            get => _url;
            set
            {
                if (_url == value) return;
                _url = value;
                OnPropertyChanged(nameof(Url));
                OnPropertyChanged(nameof(SslIconVisibility));
            }
        }

        public bool UrlFocused
        {
            get => _urlFocused;
            set
            {
                if (_urlFocused == value) return;
                _urlFocused = value;
                OnPropertyChanged(nameof(UrlFocused));
            }
        }

        bool _servoVisibility;
        private string _lastUrl;
        private bool _urlFocused;

        public bool ServoVisibility
        {
            get => _servoVisibility;
            set { _servoVisibility = value; OnPropertyChanged(nameof(ServoVisibility)); }
        }

        public void Erase()
        {
            ServoVisibility = false;
            System.Threading.Tasks.Task.Factory.StartNew(() => {
                Thread.Sleep(500);
                _servo.Erase();
            });
        }
    }
}
