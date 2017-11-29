using Servofocus.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer (typeof (Entry), typeof (UrlEntryRenderer))]
namespace Servofocus.iOS.Renderer
{
    public class UrlEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged (ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged (e);
            Control.BorderStyle = UITextBorderStyle.None;
        }
    }
}