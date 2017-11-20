using System;
using System.Drawing;
using AppKit;
using Foundation;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.MacOS;

namespace Servofocus.Mac
{
    public partial class MainWindow : NSWindow
    {
        public override void AwakeFromNib()
        {
            this.TitlebarAppearsTransparent = true;
            this.TitleVisibility = NSWindowTitleVisibility.Hidden;
            base.AwakeFromNib();
        }
    }
}