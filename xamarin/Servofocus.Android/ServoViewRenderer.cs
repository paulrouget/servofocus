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
        private float _lastY;

        delegate void SimpleCallbackDelegate();
        delegate void LogCallbackDelegate(string log);

        private GCHandle _wakeupHandle;
        private GCHandle _flushHandle;
        private GCHandle _logHandle;

        protected override void OnElementChanged(ElementChangedEventArgs<ServoView> e)
        {
            base.OnElementChanged(e);



            if (e.NewElement != null)
            {
                //e.NewElement.ScrollRequested += OnScrollRequested;

                GLSurfaceView surfaceView = Control;
                if (surfaceView == null)
                {
                    surfaceView = new GLSurfaceView(Context);
                    surfaceView.SetEGLContextClientVersion(3);
                    surfaceView.SetEGLConfigChooser(8, 8, 8, 8, 24, 0);


                    //_flushHandle = GCHandle.Alloc(flush, GCHandleType.Pinned);
                    //_logHandle = GCHandle.Alloc(log, GCHandleType.Pinned);
                    var wakeUpCb = new SimpleCallbackDelegate(() =>
                    {
                        Control.QueueEvent(() =>
                        {
                            WriteLine("FOO");
                            var x = Element.ServoSharp.PerformUpdates();
                            WriteLine("BAR");
                        });
                    });

                    var flushCb = new SimpleCallbackDelegate(() => Control.RequestRender());
                    var logCb = new LogCallbackDelegate(log => WriteLine(log));

                    var wakeUpPtr = Marshal.GetFunctionPointerForDelegate(wakeUpCb);
                    //_wakeupHandle = GCHandle.Alloc(wakeUpPtr, GCHandleType.Pinned);

                    var flushPtr = Marshal.GetFunctionPointerForDelegate(flushCb);
                    //_flushHandle = GCHandle.Alloc(flushPtr, GCHandleType.Pinned);

                    var logPtr = Marshal.GetFunctionPointerForDelegate(logCb);
                    //_logHandle = GCHandle.Alloc(logPtr, GCHandleType.Pinned);

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
            var x = touchEventArgs.Event.RawX;
            var y = touchEventArgs.Event.RawY;
            var delta = y - _lastY;
            _lastY = touchEventArgs.Event.RawY;

            // https://developer.android.com/reference/android/view/MotionEvent.html
            System.Diagnostics.Debug.WriteLine(touchEventArgs.Event);

            if(touchEventArgs.Event.Action == MotionEventActions.Up)
                Element.OnTap(x, y);
            else if (touchEventArgs.Event.Action == MotionEventActions.Move)
            {
                // need to find gesture status
                // https://github.com/mozilla-mobile/focus-android/blob/a61745794f28f4ac924fed5d9b62d1a03fea3613/app/src/gecko/java/org/mozilla/focus/web/NestedGeckoView.java
                Element.OnScroll(GestureStatus.Started, delta);
            }
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


			public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
            {
                //Interop.InitWithEgl(
                    //() => _wakeup(),
                    //() => _flush(),
                    //(str) => {}, // System.Diagnostics.Debug.WriteLine("[servo] " + Marshal.PtrToStringAnsi(str)),
                    //540, 740);


                var viewLayout = ViewLayout.__CreateInstance(new ViewLayout.__Internal
                {
                    hidpi_factor = 1f,
                    margins = new Margins.__Internal(),
                    position = new Position.__Internal(),
                    view_size = new Size.__Internal()
                });


               _servoView.ServoSharp.InitWithEgl(_callbacks, viewLayout);
			}
        }

    }
}
