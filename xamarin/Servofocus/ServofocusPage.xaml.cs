using Xamarin.Forms;
using System.Diagnostics;

namespace Servofocus
{
    public partial class ServofocusPage : ContentPage
    {
        public ServofocusPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            UrlField.Text = ServoView.Servo.ServoVersion;
         
            Debug.WriteLine("OnAppearing");
        }


		protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
        }
    }
}