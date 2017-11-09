using System;
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
        const string Url = "file:///sdcard/servo/newpage.html";
        const string ResourcePath = "/sdcard/servo/resources";
        Size _viewSize;
        float _hidpiFactor = 2f;
        public Margins Margins { get; } = new Margins(); 
        public Position Position { get; } = new Position();
        public HostCallbacks HostCallbacks { get; private set; }
        public ViewLayout ViewLayout { get; private set; }

        public unsafe string ServoVersion => Marshal.PtrToStringAnsi((IntPtr) _servoSharp.ServoVersion());

        public void InitWithEgl()
        {
            CheckServoResult(() => _servoSharp.InitWithEgl(Url, ResourcePath, HostCallbacks, ViewLayout));
        }
        
        public void Resize(uint height, uint width)
        {
            _viewSize = new Size { Height = height, Width = width };
            CheckServoResult(() => _servoSharp.Resize(CreateLayout()));
        }

        public void PerformUpdates()
        {
            CheckServoResult(() => _servoSharp.PerformUpdates());
        }

        public void Scroll(int dx, int dy, uint x, uint y, ScrollState state)
        {
            CheckServoResult(() => _servoSharp.Scroll(dx, dy, x, y, state));
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

        public void SetSize(uint height, uint width)
        {
            _viewSize = new Size {Height = height, Width = width};
        }

        public void SetHidpiFactor(float hidpiFactor)
        {
            _hidpiFactor = hidpiFactor;
        }

        public void SetHostCallbacks(Action<Action> wakeUp, Action flush, Action<string> log, Action loadStarted, Action loadEnded, 
            Action<string> titleChanged, Action<string> urlChanged, Action<bool, bool> historyChanged)
        {
            var wakeUpCb = new SimpleCallbackDelegate(() => wakeUp(PerformUpdates));
            var flushCb = new SimpleCallbackDelegate(flush);
            var logCb = new LogCallbackDelegate(log);
            var loadStartedCb = new SimpleCallbackDelegate(loadStarted);
            var loadEndedCb = new SimpleCallbackDelegate(loadEnded);
            var titleChangedCb = new TitleChangedCallbackDelegate(titleChanged);
            var urlChangedCb = new UrlChangedCallbackDelegate(urlChanged);
            var historyChangedCb = new HistoryChangedCallbackDelegate(historyChanged);

            HostCallbacks = new HostCallbacks
            {
                wakeup = Marshal.GetFunctionPointerForDelegate(wakeUpCb),
                flush = Marshal.GetFunctionPointerForDelegate(flushCb),
                log = Marshal.GetFunctionPointerForDelegate(logCb),
                on_load_started = Marshal.GetFunctionPointerForDelegate(loadStartedCb),
                on_load_ended = Marshal.GetFunctionPointerForDelegate(loadEndedCb),
                on_title_changed = Marshal.GetFunctionPointerForDelegate(titleChangedCb),
                on_url_changed = Marshal.GetFunctionPointerForDelegate(urlChangedCb),
                on_history_changed = Marshal.GetFunctionPointerForDelegate(historyChangedCb)
            };
        }

        void CheckServoResult(Func<ServoResult> action)
        {
            var result = action();
            if (result != ServoResult.Ok)
                throw new ServoException(result);
        }
    }
}
