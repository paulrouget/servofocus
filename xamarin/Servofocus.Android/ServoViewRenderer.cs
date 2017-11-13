using Android.Content;
using Android.Opengl;
using Android.Util;
using Javax.Microedition.Khronos.Opengles;
using Servofocus;
using Servofocus.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Views;
using ServoSharp;
using static System.Diagnostics.Debug;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Android
{
    public class ServoViewRenderer : ViewRenderer<ServoView, GLSurfaceView>
    {
        bool _disposed;
        int _lastY;
        long _touchDownTime;
        bool _isScrolling;
        const int MoveDelay = 1000000;

        protected override void OnElementChanged(ElementChangedEventArgs<ServoView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                GLSurfaceView surfaceView = Control;
                if (surfaceView == null)
                {
                    surfaceView = new GLSurfaceView(Context);
                    surfaceView.SetEGLContextClientVersion(3);
                    surfaceView.SetEGLConfigChooser(8, 8, 8, 8, 24, 0);

                    Element.Servo.SetHostCallbacks(
                        wakeUp: action => Control.QueueEvent(action),
                        flush: () => Control.RequestRender(),
                        log: msg => {/*WriteLine(msg);*/},
                        loadStarted: () => WriteLine("Load started"),
                        loadEnded: () => WriteLine("Load ended"),
                        titleChanged: title => WriteLine($"new title {title}"),
                        historyChanged: (back, forward) =>
                        {
                            WriteLine($"Can go back: {back}");
                            WriteLine($"Can go forward: {forward}");
                        });
                    
                    var renderer = new Renderer(Element);

                    surfaceView.SetRenderer(renderer);
                    SetNativeControl(surfaceView);

                    Touch += OnTouch;
                }
                Control.RenderMode = Rendermode.WhenDirty;
            }

            
        }
        
        void OnTouch(object sender, TouchEventArgs touchEventArgs)
        {
            var x = (int)touchEventArgs.Event.RawX;
            var y = (int)touchEventArgs.Event.RawY;
            var currentTime = System.DateTime.Now.Ticks;

            // https://developer.android.com/reference/android/view/MotionEvent.html
            // System.Diagnostics.Debug.WriteLine(touchEventArgs.Event);

            if (!_isScrolling) {
                switch (touchEventArgs.Event.Action)
                {
                    case MotionEventActions.Down:
                        _touchDownTime = currentTime;
                        _lastY = y;
                        break;
                    case MotionEventActions.Up:
                        // click
                        //var dm = new DisplayMetrics();
                        var dp = Context.Resources.DisplayMetrics;
                        WriteLine($"Click: {x}x{y}");
                        // FIXME: magic value. that's the height of the urlbar.
                        Control.QueueEvent(() => Element.Servo.Click((uint)x, (uint)y - Element.Servo.MeasureUrlHeight() * 4));
                        break;
                    case MotionEventActions.Move:
                        if (currentTime - _touchDownTime > MoveDelay)
                        { // 100ms
                            _isScrolling = true;
                            var delta = y - _lastY;
                            _lastY = (int)touchEventArgs.Event.RawY;
                            Control.QueueEvent(() => Element.Servo.Scroll(0, delta, 0, 0, ScrollState.Start));
                        }
                        break;
                }
            }
            else
            {
                switch (touchEventArgs.Event.Action)
                {
                    case MotionEventActions.Move:
                    {
                        var delta = y - _lastY;
                        _lastY = (int)touchEventArgs.Event.RawY;
                        Control.QueueEvent(() => Element.Servo.Scroll(0, delta, 0, 0, ScrollState.Move));
                        break;
                    }
                    case MotionEventActions.Up:
                    {
                        _isScrolling = false;
                        var delta = y - _lastY;
                        Control.QueueEvent(() => Element.Servo.Scroll(0, delta, 0, 0, ScrollState.End));
                        break;
                    }
                }
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        class Renderer : Java.Lang.Object, GLSurfaceView.IRenderer
        {
            readonly ServoView _servoView;

            public Renderer(ServoView servoView)
			{
			    _servoView = servoView;
			}

			public void OnDrawFrame(IGL10 gl)
			{
			}

			public void OnSurfaceChanged(IGL10 gl, int width, int height)
			{
                _servoView.Servo.Resize((uint)height, (uint)width);
			}

			public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
            {
                _servoView.Servo.InitWithEgl();
            }
        }
    }
}