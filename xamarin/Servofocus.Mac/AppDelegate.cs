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
                                     NSWindowStyle.FullSizeContentView |
                                     NSWindowStyle.Miniaturizable;

            var rect = new CoreGraphics.CGRect(200, 200, 800, 800);
            _window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            _window.Title = "Servo Focus";
            _window.TitleVisibility = NSWindowTitleVisibility.Hidden;
            _window.TitlebarAppearsTransparent = true;
            _window.MovableByWindowBackground = true;

            var button = _window.StandardWindowButton(NSWindowButton.CloseButton);
            NSView parent = button.Superview.Superview;
            var l = parent.Frame.Location;
            l.Y -= 18;
            l.X += 12;
            parent.SetFrameOrigin(l);
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
