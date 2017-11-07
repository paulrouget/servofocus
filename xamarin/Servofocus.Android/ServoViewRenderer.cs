using System;
using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using Servofocus;
using Servofocus.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.Runtime.InteropServices;
using Android.Graphics;
using Android.Views;
using static System.Diagnostics.Debug;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Android
{
    public class ServoViewRenderer : ViewRenderer<ServoView, GLSurfaceView>
    {
        bool _disposed;
        private int _lastY;

        GestureStatus _gStatus;

        delegate void SimpleCallbackDelegate();
        delegate void LogCallbackDelegate(string log);

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

                    var wakeUpCb = new SimpleCallbackDelegate(() =>
                    {
                        Control.QueueEvent(() =>
                        {
                            var x = Element.ServoSharp.PerformUpdates();
                        });
                    });

                    var flushCb = new SimpleCallbackDelegate(() => Control.RequestRender());
                    var logCb = new LogCallbackDelegate(log => {/*WriteLine(log);*/});

                    var wakeUpPtr = Marshal.GetFunctionPointerForDelegate(wakeUpCb);

                    var flushPtr = Marshal.GetFunctionPointerForDelegate(flushCb);

                    var logPtr = Marshal.GetFunctionPointerForDelegate(logCb);

                    var hostCallbackInstance = HostCallbacks.__CreateInstance(new HostCallbacks.__Internal
                    {
                        wakeup = wakeUpPtr,
                        flush = flushPtr,
                        log = logPtr
                    });

                    var renderer = new Renderer(
                        hostCallbackInstance, Element
                    );

                    surfaceView.SetRenderer(renderer);
                    SetNativeControl(surfaceView);

                    Touch += OnTouch;
                    Element.RegisterGLCallback(callback => Control.QueueEvent(callback));
                }
                Control.RenderMode = Rendermode.WhenDirty;
            }
        }
        
        private void OnTouch(object sender, TouchEventArgs touchEventArgs)
        {
            int x = (int)touchEventArgs.Event.RawX;
            int y = (int)touchEventArgs.Event.RawY;
            int delta = y - _lastY;
            _lastY = (int)touchEventArgs.Event.RawY;

            // https://developer.android.com/reference/android/view/MotionEvent.html
            System.Diagnostics.Debug.WriteLine(touchEventArgs.Event);

            Control.QueueEvent(() =>
            {
                if(touchEventArgs.Event.Action == MotionEventActions.Down)
                    Element.ServoSharp.Scroll(0, 0, 0, 0, ScrollState.Start);
                if(touchEventArgs.Event.Action == MotionEventActions.Move)
                    Element.ServoSharp.Scroll(0, delta, 0, 0, ScrollState.Move);
                if(touchEventArgs.Event.Action == MotionEventActions.Up)
                    Element.ServoSharp.Scroll(0, delta, 0, 0, ScrollState.End);
                
            });

        }

        private void OnScrollRequested(object sender, EventArgs args)
        {
            var e = (ScrollArgs)args;
            Control.QueueEvent(() =>
            {
                if (e.status == GestureStatus.Started) {
                    Element.ServoSharp.Scroll(e.dx, e.dy, e.x, e.y, ScrollState.Start);
                } else if (e.status == GestureStatus.Running) {
                    Element.ServoSharp.Scroll(e.dx, e.dy, e.x, e.y, ScrollState.Move);
                } else {
                    Element.ServoSharp.Scroll(e.dx, e.dy, e.x, e.y, ScrollState.End);
                }
            });
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
            readonly HostCallbacks _callbacks;
            readonly ServoView _servoView;
            private GCHandle _viewLayoutHandle;
            private GCHandle _marginsHandle;
            private GCHandle _positionsHandle;
            private GCHandle _viewSizeHandle;

            public Renderer(HostCallbacks callbacks, ServoView servoView)
			{
                _callbacks = callbacks;
			    _servoView = servoView;
			}

			public void OnDrawFrame(IGL10 gl)
			{
			}

			public void OnSurfaceChanged(IGL10 gl, int width, int height)
			{
                System.Diagnostics.Debug.WriteLine("Resize:" + width + "x" + height);
			}


			public unsafe void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
            {
                var margins = new Margins();
                var position = new Position();

                var viewSize = new Size
                {
                    Height = (uint) _servoView.Height,
                    Width = (uint)_servoView.Width
                };

                var viewLayout = new ViewLayout
                {
                    __margins = margins,
                    __position = position,
                    __view_size = viewSize,
                    HidpiFactor = 1f
                };

                //var url = "http://paulrouget.com/";
                var url = "File:///sdcard/servo/home.html";
                var resourcePath = "File:///sdcard/servo/resources";

                var urlPtr = (byte*)Marshal.StringToCoTaskMemAnsi(url);
                var resourcePathPtr = (byte*)Marshal.StringToCoTaskMemAnsi(resourcePath);

                _servoView.ServoSharp.InitWithEgl(urlPtr, resourcePathPtr, _callbacks, viewLayout );
            }
        }

    }
}
