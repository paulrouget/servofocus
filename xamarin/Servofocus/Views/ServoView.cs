using ServoSharp;
using Xamarin.Forms;

namespace Servofocus.Views
{
    public class ServoView : View
    {
        public readonly Servo Servo;

        public ServoView()
        {
            Servo = new Servo();
        }
    }
}