using System;
using AppKit;
using Servofocus;
using Servofocus.Mac.Renderer;
using Servofocus.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer (typeof (EraseButton), typeof (EraseButtonRenderer))]
namespace Servofocus.Mac.Renderer
{
    public class EraseButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            Control.Bordered = false;
        }
    }
}