using System;
using Xamarin.Forms;

namespace Servofocus.Views
{
    public class EraseFloatingButton : Button
    {
        readonly bool _isAndroid = Device.RuntimePlatform == Device.Android;

        public EraseFloatingButton()
        {
            IsVisible = _isAndroid;
            IsEnabled = _isAndroid;
        }

        public Action Erase { get; set; }
    }
}