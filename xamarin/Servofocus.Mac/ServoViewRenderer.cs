using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AppKit;
using OpenTK.Graphics.OpenGL;
using Servofocus;
using Servofocus.Mac;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Mac
{
    public class ServoViewRenderer : ViewRenderer<ServoView, NSOpenGLView>
    {
        NSOpenGLContext _ctx;

        protected override void OnElementChanged(ElementChangedEventArgs<ServoView> e)
        {
            base.OnElementChanged(e);

            if(Control == null)
            {
                var openGlView = new NSOpenGLView
                {
                    WantsBestResolutionOpenGLSurface = true,
                    WantsLayer = true
                };

                var width = (uint)Application.Current.MainPage.Width;
                var height = (uint)Application.Current.MainPage.Height;

                var result = Marshal.PtrToStringAuto(Interop.ServoVersion());
                Debug.WriteLine(result);

                Object[] attributes =
                {
                    NSOpenGLPixelFormatAttribute.DoubleBuffer,
                    NSOpenGLPixelFormatAttribute.ClosestPolicy,
                    NSOpenGLPixelFormatAttribute.ColorSize,
                    32,
                    NSOpenGLPixelFormatAttribute.AlphaSize,
                    8,
                    NSOpenGLPixelFormatAttribute.DepthSize,
                    24,
                    NSOpenGLPixelFormatAttribute.StencilSize,
                    8,
                    NSOpenGLPixelFormatAttribute.OpenGLProfile,
                    NSOpenGLProfile.Version3_2Core,
                    0
                };

                var pixelFormat = new NSOpenGLPixelFormat(attributes);

                _ctx = new NSOpenGLContext(pixelFormat, null)
                {
                    View = openGlView
                };

                //FIXME
                //int value = 1;
                //ctx.SetValues(new IntPtr(value), NSOpenGLContextParameter.SwapInterval);

                _ctx.Update();
                _ctx.MakeCurrentContext();

                GL.Clear(ClearBufferMask.ColorBufferBit);

                GL.Finish();

                SetNativeControl(openGlView);
                Subscribe();

                //Interop.Init(_ctx.FlushBuffer, () => Device.BeginInvokeOnMainThread(Interop.Ping), width, height);
            }

            if (e.OldElement != null)
            {
                Unsubscribe();
            }

            if (e.NewElement != null)
            {
                Subscribe();
            }
        }

        public override void ScrollWheel(NSEvent theEvent)
        {
            base.ScrollWheel(theEvent);
        
            Debug.WriteLine(theEvent.DeltaX);
            Debug.WriteLine(theEvent.DeltaY);
            Debug.WriteLine(theEvent.DeltaZ);
        }

        public override void TouchesBeganWithEvent(NSEvent theEvent)
        {
            base.TouchesBeganWithEvent(theEvent);
        }

        void Subscribe()
        {
        }

        void Unsubscribe()
        {         
        }
    }
}
