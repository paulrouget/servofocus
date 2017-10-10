using System;
using Xamarin.Forms;

namespace Servofocus
{
    public class ServoView : View, IOpenGlViewController
    {
        public event EventHandler DisplayRequested;
		public event EventHandler ScrollRequested;
		public Action<Rectangle> OnDisplay { get; set; }
        public double _lastScrollY { get; private set; }

        public void Display()
        {
            var handler = DisplayRequested;
            handler?.Invoke(this, EventArgs.Empty);
        }

        internal void Scroll(PanUpdatedEventArgs e)
        {
            var deltaY = e.TotalY - _lastScrollY;
            _lastScrollY = e.TotalY;
			var handler = ScrollRequested;
            var args = new ScrollArgs();
            args.dx = 0;
            args.dy = (Int32) deltaY;
            args.x = 0;
            args.y = 0;
            args.status = e.StatusType;
            handler?.Invoke(this, args);
        }
    }
}

public class ScrollArgs: EventArgs
{
    public Int32 dx;
    public Int32 dy;
    public UInt32 x;
    public UInt32 y;
    public GestureStatus status;
}
