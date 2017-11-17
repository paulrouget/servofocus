using System;
using Xamarin.Forms;

namespace Servofocus
{
    public class MenuButton : Button
    {
        public Action Reload { get; set; }
        public Action GoForward { get; set; }
    }
}