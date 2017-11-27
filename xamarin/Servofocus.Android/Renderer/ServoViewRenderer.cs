using System;
using Android.Opengl;
using Android.Views;
using Android.Util;
using Javax.Microedition.Khronos.Opengles;
using Servofocus;
using Servofocus.Android.Renderer;
using Servofocus.Views;
using ServoSharp;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Android.Renderer
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
                        flush: () => Control.RequestRender()
                    );
                    
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
                        System.Diagnostics.Debug.WriteLine($"Click: {x}x{y}");
                        // FIXME: magic value. that's the height of the urlbar.
                        MessagingCenter.Send(new ClickMessage((uint)x, (uint)(y - Element.Bounds.Top * 4)), "click");
                        //Element.Servo.Click((uint)x, (uint)(y - Element.Bounds.Top * 4));
                        break;
                    case MotionEventActions.Move:
                        if (currentTime - _touchDownTime > MoveDelay)
                        { // 100ms
                            _isScrolling = true;
                            var delta = y - _lastY;
                            _lastY = (int)touchEventArgs.Event.RawY;
                            //System.Diagnostics.Debug.WriteLine(delta);             
                            MessagingCenter.Send(new ScrollMessage(0, delta, 0, 0, ScrollState.Start), "scroll");
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
                            //System.Diagnostics.Debug.WriteLine(delta);
                        MessagingCenter.Send(new ScrollMessage(0, delta, 0, 0, ScrollState.Move), "scroll");
                        break;
                    }
                    case MotionEventActions.Up:
                    {
                        _isScrolling = false;
                        var delta = y - _lastY;
                        MessagingCenter.Send(new ScrollMessage(0, delta, 0, 0, ScrollState.End), "scroll");
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