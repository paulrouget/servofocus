using System;
using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using Servofocus;
using Servofocus.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.Runtime.InteropServices;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Android
{
    public class ServoViewRenderer : ViewRenderer<ServoView, GLSurfaceView>
    {
        bool _disposed;

        protected override void OnElementChanged(ElementChangedEventArgs<ServoView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                
                e.NewElement.ScrollRequested += OnScrollRequested;

                GLSurfaceView surfaceView = Control;
                if (surfaceView == null)
                {
                    surfaceView = new GLSurfaceView(Context);
                    surfaceView.SetEGLContextClientVersion(3);
                    surfaceView.SetEGLConfigChooser(8, 8, 8, 8, 24, 0);

                    var renderer = new Renderer(
                        () => surfaceView.RequestRender(),
                        () => surfaceView.QueueEvent(() => Interop.OnEventLoopAwakenByServo() )
                    );

                    surfaceView.SetRenderer(renderer);
                    SetNativeControl(surfaceView);

                }

                Control.RenderMode = Rendermode.WhenDirty;
            }
        }

        private void OnScrollRequested(object sender, EventArgs args)
        {
            var e = (ScrollArgs)args;
            Control.QueueEvent(() =>
            {
                if (e.status == GestureStatus.Started) {
                    Interop.Scroll(e.dx, e.dy, e.x, e.y, 0);
                } else if (e.status == GestureStatus.Running) {
                    Interop.Scroll(e.dx, e.dy, e.x, e.y, 1);
                } else {
                    Interop.Scroll(0, 0, 0, 0, 2);
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
            Action _flush;
            Action _wakeup;

			public Renderer(Action interopCallback, Action wakeup)
			{
                _flush = interopCallback;
                _wakeup = wakeup;
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
                
                Interop.InitWithEgl(
                    () => _wakeup(),
                    () => _flush(),
                    (str) => {}, // System.Diagnostics.Debug.WriteLine("[servo] " + Marshal.PtrToStringAnsi(str)),
                    540, 740);
			}
        }

    }
}
