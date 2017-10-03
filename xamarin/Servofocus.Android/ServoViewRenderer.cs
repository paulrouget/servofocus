using System;
using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using Servofocus;
using Servofocus.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

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
                GLSurfaceView surfaceView = Control;
                if (surfaceView == null)
                {
                    surfaceView = new GLSurfaceView(Context);
                    surfaceView.SetEGLContextClientVersion(3);
                    surfaceView.SetEGLConfigChooser(8, 8, 8, 8, 24, 0);
                    var renderer = new Renderer(Element);
                    surfaceView.SetRenderer(renderer);
                    SetNativeControl(surfaceView);
                }

                Control.RenderMode = Rendermode.Continuously;
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
            readonly ServoView _model;
            Rectangle _rect;

            public Renderer(ServoView model)
            {
                _model = model;
            }

            public void OnDrawFrame(IGL10 gl)
            {
                Action<Rectangle> onDisplay = _model.OnDisplay;
                onDisplay?.Invoke(_rect);
            }

            public void OnSurfaceChanged(IGL10 gl, int width, int height)
            {
                _rect = new Rectangle(0.0, 0.0, width, height);
            }

            public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
            {
            }
        }

        void ScrollChanged(object sender, ScrollChangeEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("I scroll!");
        }

        void Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("I click!");
        }

        void Subscribe()
        {
            Control.ScrollChange += ScrollChanged;
            Control.Click += Click;
        }

        void Unsubscribe()
        {
            Control.ScrollChange -= ScrollChanged;
            Control.Click -= Click;
        }
    }
}
