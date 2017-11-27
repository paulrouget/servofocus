using System;
using Xamarin.Forms;

namespace Servofocus.Views
{
    public class MenuButton : Button
    {
        readonly bool IsAndroid = Device.RuntimePlatform == Device.Android;

        public MenuButton()
        {
            IsVisible = IsAndroid;
            IsEnabled = IsAndroid;
        }
        public Action Reload { get; set; }
        public Action GoForward { get; set; }
    }
}