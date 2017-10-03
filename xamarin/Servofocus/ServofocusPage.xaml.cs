using Xamarin.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.ES30;

namespace Servofocus
{
    public partial class ServofocusPage : ContentPage
    {
        float red, green, blue;

        public ServofocusPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            UrlField.Text = Marshal.PtrToStringAnsi(Interop.ServoVersion());

            //OpenGlView.HasRenderLoop = true;
            ServoView.OnDisplay += OnDisplay;

            Debug.WriteLine("OnAppearing");
        }

        private void OnDisplay(Rectangle rectangle)
        {
            GL.ClearColor(red, green, blue, 1.0f);
            GL.Clear((ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            red += 0.01f;
            if (red >= 1.0f)
                red -= 1.0f;
            green += 0.02f;
            if (green >= 1.0f)
                green -= 1.0f;
            blue += 0.03f;
            if (blue >= 1.0f)
                blue -= 1.0f;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
        }
    }
}
