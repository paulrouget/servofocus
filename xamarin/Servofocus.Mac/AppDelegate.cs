using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Servofocus.Mac
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow _window;
        public AppDelegate()
        {
            var style = NSWindowStyle.Closable |
                        NSWindowStyle.Resizable |
                        NSWindowStyle.Titled |
                        NSWindowStyle.FullSizeContentView;

            var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
            _window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            _window.Title = "Servo.Xamarin on Mac!";
            _window.TitleVisibility = NSWindowTitleVisibility.Hidden;
            _window.TitlebarAppearsTransparent = true;
        }

        public override NSWindow MainWindow
        {
            get { return _window; }
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            Forms.Init();
            LoadApplication(new App());
            base.DidFinishLaunching(notification);
        }
    }
}
