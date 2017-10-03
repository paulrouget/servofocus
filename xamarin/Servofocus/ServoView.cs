using System;
using Xamarin.Forms;

namespace Servofocus
{
    public class ServoView : View, IOpenGlViewController
    {
        public event EventHandler DisplayRequested;
        public Action<Rectangle> OnDisplay { get; set; }

        public void Display()
        {
            var handler = DisplayRequested;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
