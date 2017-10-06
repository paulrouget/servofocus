using System;
using Xamarin.Forms;

namespace Servofocus
{
    public class ServoView : View, IOpenGlViewController
    {
        public event EventHandler DisplayRequested;
		public event EventHandler ScrollRequested;
		public Action<Rectangle> OnDisplay { get; set; }
        public Int16 _scrollState = 0;

        public void Display()
        {
            var handler = DisplayRequested;
            handler?.Invoke(this, EventArgs.Empty);
        }

        internal void Scroll(double totalX, double totalY)
        {
			var handler = ScrollRequested;
            var args = new ScrollArgs();
            args.dx = 0;
            args.dy = 10;
            args.x = 10;
            args.y = 300;
            args.state = _scrollState;
            handler?.Invoke(this, args);
            _scrollState = 1;
        }
    }
}

public class ScrollArgs: EventArgs
{
    public Int32 dx;
    public Int32 dy;
    public UInt32 x;
    public UInt32 y;
    public Int16 state;
}