using System;
using System.ComponentModel;
using System.Drawing;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using GLKit;
using OpenGLES;
using Servofocus;
using Servofocus.iOS;
using Servofocus.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.iOS
{
    internal class ServoViewRenderer : ViewRenderer<ServoView, GLKView>
    {
        CADisplayLink _displayLink;

        public void Display(object sender, EventArgs eventArgs)
        {
            //if (Element.HasRenderLoop)
            //    return;
            //SetupRenderLoop(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (_displayLink != null)
            {
                _displayLink.Invalidate();
                _displayLink.Dispose();
                _displayLink = null;

                if (Element != null)
                    ((IOpenGlViewController)Element).DisplayRequested -= Display;
            }

            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ServoView> e)
        {
            if (e.OldElement != null)
                ((IOpenGlViewController)e.OldElement).DisplayRequested -= Display;

            if (e.NewElement != null)
            {
                var context = new EAGLContext(EAGLRenderingAPI.OpenGLES3);
                var glkView = new GLKView(RectangleF.Empty) 
                { 
                    Context = context, 
                    DrawableDepthFormat = GLKViewDrawableDepthFormat.Format24,
                    Delegate = new Delegate(e.NewElement) 
                };
                SetNativeControl(glkView);

                SetupRenderLoop(false);
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == OpenGLView.HasRenderLoopProperty.PropertyName)
                SetupRenderLoop(false);
        }

        void SetupRenderLoop(bool oneShot)
        {
            if (_displayLink != null)
                return;
            //if (!oneShot && !Element.HasRenderLoop)
                //return;

            _displayLink = CADisplayLink.Create(() =>
            {
                var control = Control;
                var model = Element;
                if (control != null)
                    control.Display();
                //if (control == null || model == null || !model.HasRenderLoop)
                //{
                    //_displayLink.Invalidate();
                    //_displayLink.Dispose();
                    //_displayLink = null;
                //}
            });
            _displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);
        }

        class Delegate : GLKViewDelegate
        {
            readonly View _model;

            public Delegate(View model)
            {
                _model = model;
            }

            public override void DrawInRect(GLKView view, CGRect rect)
            {
                //var onDisplay = _model.OnDisplay;
                //if (onDisplay == null)
                //    return;
                //onDisplay(rect.ToRectangle());
            }
        }
    }
}
