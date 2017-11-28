using System;
using ServoSharp;
using Xamarin.Forms;

namespace Servofocus.Views
{
    public class ServoView : View
    {
        public Action<uint, uint> Click { get; set; }
        public Action<int, int, uint, uint, ScrollState> Scroll { get; set; }
    }
}