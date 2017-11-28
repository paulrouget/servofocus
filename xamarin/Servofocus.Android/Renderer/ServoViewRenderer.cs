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
        MainViewModel ViewModel;

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

                    ViewModel = (MainViewModel) Element.BindingContext;
                    ViewModel.SetHostCallbacks(action => Control.QueueEvent(action), () => Control.RequestRender());
                    
                    var renderer = new Renderer((height, width) => ViewModel.Resize(height, width), () => ViewModel.InitWithEgl() );

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
                        System.Diagnostics.Debug.WriteLine($"Click: {x}x{y}");
                        // FIXME: magic value. that's the height of the urlbar.
                        ViewModel.Click((uint)x, (uint)(y - Element.Bounds.Top * 4));
                        break;
                    case MotionEventActions.Move:
                        if (currentTime - _touchDownTime > MoveDelay)
                        { // 100ms
                            _isScrolling = true;
                            var delta = y - _lastY;
                            _lastY = (int)touchEventArgs.Event.RawY;
                            //System.Diagnostics.Debug.WriteLine(delta);      
                            ViewModel.Scroll(0, delta, 0, 0, ScrollState.Start);                            
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
                            ViewModel.Scroll(0, delta, 0, 0, ScrollState.Move);                        
                            break;
                        }
                    case MotionEventActions.Up:
                    {
                        _isScrolling = false;
                        var delta = y - _lastY;
                        ViewModel.Scroll(0, delta, 0, 0, ScrollState.End);                        
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
            readonly Action<uint, uint> _resize;
            readonly Action _initWithEgl;

            public Renderer(Action<uint,uint> resize, Action initWithEgl)
            {
                _resize = resize;
                _initWithEgl = initWithEgl;
            }

			public void OnDrawFrame(IGL10 gl)
			{
			}

			public void OnSurfaceChanged(IGL10 gl, int width, int height)
			{
                _resize((uint)height, (uint)width);
			}

			public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
            {
               _initWithEgl();
            }
        }
    }
}