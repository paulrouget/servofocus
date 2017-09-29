using System;
using Android.Opengl;
using Servofocus;
using Servofocus.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Android
{
    public class ServoViewRenderer : ViewRenderer<ServoView, GLSurfaceView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ServoView> e)
        {
            base.OnElementChanged(e);

            if(Control == null)
            {
                var glSurfaceView = new GLSurfaceView(Context);
                glSurfaceView.SetRenderer(new GlRenderer());
                SetNativeControl(glSurfaceView);
                Subscribe();
            }

            if(e.OldElement != null)
            {
                Unsubscribe();
            }

            if(e.NewElement != null)
            {
                Subscribe();
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
