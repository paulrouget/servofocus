﻿using System;
using AppKit;
using CoreGraphics;
using Servofocus.Mac;
using Servofocus.Views;
using ServoSharp;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;


[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Mac
{
    public partial class NSServoView : NSView
    {
        NSOpenGLContext openGLContext;
        NSOpenGLPixelFormat pixelFormat;
        ServoView _servoView;

        public NSServoView(ServoView control)
        {
            _servoView = control;
            Object[] attributes =
            {
                NSOpenGLPixelFormatAttribute.DoubleBuffer,
                NSOpenGLPixelFormatAttribute.ClosestPolicy,
                NSOpenGLPixelFormatAttribute.ColorSize, 32,
                NSOpenGLPixelFormatAttribute.AlphaSize, 8,
                NSOpenGLPixelFormatAttribute.DepthSize, 24,
                NSOpenGLPixelFormatAttribute.StencilSize, 8,
                NSOpenGLPixelFormatAttribute.OpenGLProfile, NSOpenGLProfile.Version3_2Core,
                0
            };
            pixelFormat = new NSOpenGLPixelFormat(attributes);
            openGLContext = new NSOpenGLContext(pixelFormat, null);
            openGLContext.MakeCurrentContext();
        }

        public void Flush()
        {
            openGLContext?.FlushBuffer();
        }

        public override bool AcceptsFirstResponder ()
        {
            return true;
        }

        public override void DrawRect (CGRect dirtyRect)
        {
            if (openGLContext.View != this)
            {
                openGLContext.View = this;
            }
        }
    }

    public class ServoViewRenderer : ViewRenderer<ServoView, NSServoView>
    {
        MainViewModel _viewModel;

        protected override void OnElementChanged(ElementChangedEventArgs<ServoView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var view = new NSServoView(Element)
                {
                    WantsBestResolutionOpenGLSurface = true,
                    WantsLayer = true
                };
                _viewModel = (MainViewModel) e.NewElement.BindingContext;
                _viewModel.SetHostCallbacks(Device.BeginInvokeOnMainThread, view.Flush);
                SetNativeControl(view);
             }
        }

        public override void ScrollWheel(NSEvent e)
        {
            var phase = ScrollState.Move;
            if (e.Phase == NSEventPhase.MayBegin ||
                e.Phase == NSEventPhase.Began) {
                phase = ScrollState.Start;
            } else if (e.Phase == NSEventPhase.Ended) {
                phase = ScrollState.End;
            }
            // FIXME: pixel density
            _viewModel.Scroll(0, 2 * (int)e.ScrollingDeltaY, 0, 0, phase);
            base.ScrollWheel(e);
        }

        public override void MouseUp(NSEvent e)
        {
            var nswindow = e.Window;
            var window_point = e.LocationInWindow;
            var view_point = Control.ConvertPointFromView(window_point, Control);
            var frame = Control.Frame;
            var hidpi_factor = nswindow.BackingScaleFactor;
            var x = hidpi_factor * view_point.X;
            var y = hidpi_factor * (frame.Size.Height - view_point.Y);
            _viewModel.Click((uint)x, (uint)y);
            base.MouseUp(e);
        }
    }
}
