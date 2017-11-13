using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using Servofocus;
using Servofocus.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Views;
using ServoSharp;
using static System.Diagnostics.Debug;
using System;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Android
{
    public class ServoViewRenderer : ViewRenderer<ServoView, GLSurfaceView>
    {
        bool _disposed;
        int _lastY;
        Int64 _touchDownTime;
        bool _isScrolling = false;

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
                        urlChanged: url => WriteLine($"new url {url}"),
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
            int x = (int)(touchEventArgs.Event.RawX);
            int y = (int)(touchEventArgs.Event.RawY);
            var currentTime = System.DateTime.Now.Ticks;

            // https://developer.android.com/reference/android/view/MotionEvent.html
            // System.Diagnostics.Debug.WriteLine(touchEventArgs.Event);

            if (!_isScrolling) {
                if (touchEventArgs.Event.Action == MotionEventActions.Down) {
                    _touchDownTime = currentTime;
                    _lastY = y;
                }
                if (touchEventArgs.Event.Action == MotionEventActions.Up) {
                    // click
                    System.Diagnostics.Debug.WriteLine($"Click: {x}x{y}");
                    // FIXME: magic value. that's the height of the urlbar.
                    Control.QueueEvent(() => Element.Servo.Click((uint) x, (uint) y - 300));
                }
                if (touchEventArgs.Event.Action == MotionEventActions.Move) {
                    if (currentTime - _touchDownTime > 1000000) { // 100ms
                        _isScrolling = true;
                        int delta = y - _lastY;
                        _lastY = (int)touchEventArgs.Event.RawY;
                        Control.QueueEvent(() => Element.Servo.Scroll(0, delta, 0, 0, ScrollState.Start));
                    }
                }
            } else {
                if (touchEventArgs.Event.Action == MotionEventActions.Move) {
                    int delta = y - _lastY;
                    _lastY = (int)touchEventArgs.Event.RawY;
                    Control.QueueEvent(() => Element.Servo.Scroll(0, delta, 0, 0, ScrollState.Move));
                }
                if (touchEventArgs.Event.Action == MotionEventActions.Up) {
                    _isScrolling = false;
                    int delta = y - _lastY;
                    Control.QueueEvent(() => Element.Servo.Scroll(0, delta, 0, 0, ScrollState.End));
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