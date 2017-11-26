using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ServoSharp
{
    public delegate void SimpleCallbackDelegate();
    public delegate void LogCallbackDelegate(string log);
    public delegate void TitleChangedCallbackDelegate(string title);
    public delegate void UrlChangedCallbackDelegate(string url);
    public delegate void HistoryChangedCallbackDelegate(bool canGoBack, bool canGoForward);

    public class Servo
    {
        readonly ServoSharp _servoSharp = new ServoSharp();
        const string Url = "about:blank";
        string _resourcePath;
        Size _viewSize;
        float _hidpiFactor = 2f;
        public Margins Margins { get; } = new Margins(); 
        public Position Position { get; } = new Position();
        public HostCallbacks HostCallbacks { get; private set; }

        Action<Action> _executeInServoThread;
        SimpleCallbackDelegate _wakeUp;
        SimpleCallbackDelegate _flush;
        LogCallbackDelegate _log;
        SimpleCallbackDelegate _loadStarted;
        SimpleCallbackDelegate _loadEnded;
        TitleChangedCallbackDelegate _titleChanged;
        UrlChangedCallbackDelegate _urlChanged;
        HistoryChangedCallbackDelegate _historyChanged;

        public unsafe string ServoVersion => Marshal.PtrToStringAnsi((IntPtr) _servoSharp.ServoVersion());
       
        public void InitWithEgl()
        {
            ExecuteServoCode(() => _servoSharp.InitWithEgl(Url, _resourcePath, HostCallbacks, CreateLayout()));
        }

        public void InitWithGL()
        {
            ExecuteServoCode(() => _servoSharp.InitWithGL(Url, _resourcePath, HostCallbacks, CreateLayout()));
        }

        public void Resize(uint height, uint width)
        {
            _viewSize = new Size { Height = height, Width = width };
            ExecuteServoCode(() => _servoSharp.Resize(CreateLayout()));
        }

        public void PerformUpdates()
        {
            ExecuteServoCode(() => _servoSharp.PerformUpdates());
        }

        public void Scroll(int dx, int dy, uint x, uint y, ScrollState state)
        {
            ExecuteServoCode(() => _servoSharp.Scroll(dx, dy, x, y, state));
        }

        public void Click(uint x, uint y)
        {
            ExecuteServoCode(() => _servoSharp.Click(x, y));
        }

        public void SetResourcePath(string path)
        {
            _resourcePath = path;
        }

        public void LoadUrl(string url)
        {
            ExecuteServoCode(() => _servoSharp.LoadUrl(url));
        }

        public void GoBack()
        {
            ExecuteServoCode(() => _servoSharp.GoBack());
        }

        ViewLayout CreateLayout()
        {
            return new ViewLayout
            {
                Margins = Margins,
                Positions = Position,
                ViewSize = _viewSize,
                HidpiFactor = _hidpiFactor,
            };
        }

        public void SetSize(uint width, uint height)
        {
            _viewSize = new Size {Height = height, Width = width};
        }

        public void SetHidpiFactor(float hidpiFactor)
        {
            _hidpiFactor = hidpiFactor;
        }

        public void SetLogCallback(Action<string> callback)
        {
            _log = new LogCallbackDelegate(callback);
        }

        public void SetUrlCallback(Action<string> callback)
        {
            _urlChanged = new UrlChangedCallbackDelegate(callback);
        }

        public void SetTitleCallback(Action<string> callback)
        {
            _titleChanged = new TitleChangedCallbackDelegate(callback);
        }

        public void SetHistoryCallback(Action<bool,bool> callback)
        {
            _historyChanged = new HistoryChangedCallbackDelegate(callback);
        }

        public void SetLoadStartedCallback(Action callback)
        {
            _loadStarted = new SimpleCallbackDelegate(callback);
        }

        public void SetLoadEndedCallback(Action callback)
        {
            _loadEnded = new SimpleCallbackDelegate(callback);
        }

        public void SetHostCallbacks(Action<Action> wakeUp, Action flush)
        {
            _wakeUp = () => wakeUp(PerformUpdates);
            _executeInServoThread = wakeUp;
            _flush = new SimpleCallbackDelegate(flush);
        }

        public void ValidateCallbacks()
        {
            if(_wakeUp == null) throw new ArgumentNullException(nameof(_wakeUp));
            if(_flush == null) throw new ArgumentNullException(nameof(_flush));
            if(_log == null) throw new ArgumentNullException(nameof(_log));
            if(_loadStarted == null) throw new ArgumentNullException(nameof(_loadStarted));
            if(_loadEnded == null) throw new ArgumentNullException(nameof(_loadEnded));
            if(_titleChanged == null) throw new ArgumentNullException(nameof(_titleChanged));
            if(_historyChanged == null) throw new ArgumentNullException(nameof(_historyChanged));
            if(_urlChanged == null) throw new ArgumentNullException(nameof(_urlChanged));
            if(_executeInServoThread == null) throw new ArgumentNullException(nameof(_executeInServoThread));
            if(string.IsNullOrEmpty(_resourcePath)) throw new ArgumentNullException(nameof(_resourcePath));

            HostCallbacks = new HostCallbacks
            {
                wakeup = Marshal.GetFunctionPointerForDelegate(_wakeUp),
                flush = Marshal.GetFunctionPointerForDelegate(_flush),
                log = Marshal.GetFunctionPointerForDelegate(_log),
                on_load_started = Marshal.GetFunctionPointerForDelegate(_loadStarted),
                on_load_ended = Marshal.GetFunctionPointerForDelegate(_loadEnded),
                on_title_changed = Marshal.GetFunctionPointerForDelegate(_titleChanged),
                on_url_changed = Marshal.GetFunctionPointerForDelegate(_urlChanged),
                on_history_changed = Marshal.GetFunctionPointerForDelegate(_historyChanged)
            };
        }

        void ExecuteServoCode(Func<ServoResult> code)
        {
            _executeInServoThread(() => {
                var result = code();
                if (result != ServoResult.Ok)
                  throw new ServoException(result);
            });
        }
    }
}
