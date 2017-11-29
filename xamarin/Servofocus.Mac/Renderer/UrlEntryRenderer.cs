using Servofocus.Mac.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer (typeof (Entry), typeof (UrlEntryRenderer))]
namespace Servofocus.Mac.Renderer
{
    public class UrlEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged (ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged (e);
            Control.Bordered = false;
            Control.FocusRingType = AppKit.NSFocusRingType.None;
        }
    }
}