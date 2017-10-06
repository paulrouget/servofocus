using Xamarin.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Servofocus
{
    public partial class ServofocusPage : ContentPage
    {
        PanGestureRecognizer _panGesture;
        TapGestureRecognizer _tapGesture;

        public ServofocusPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UrlField.Text = Marshal.PtrToStringAnsi(Interop.ServoVersion());

           
            Subscribe();
           
            Debug.WriteLine("OnAppearing");
        }


		protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
            Unsubscribe();
        }

        void Subscribe()
        {
            _panGesture = new PanGestureRecognizer();
            _panGesture.PanUpdated += PanGesture;

            _tapGesture = new TapGestureRecognizer();
            _tapGesture.Tapped += TapGesture;

            ServoView.GestureRecognizers.Add(_panGesture);
            ServoView.GestureRecognizers.Add(_tapGesture);
        }

        void Unsubscribe()
        {
            _panGesture.PanUpdated -= PanGesture;
            _tapGesture.Tapped -= TapGesture;

            ServoView.GestureRecognizers.Remove(_panGesture);
            ServoView.GestureRecognizers.Remove(_tapGesture);
        }

        void TapGesture(object sender, System.EventArgs e)
        {         
            // no point location though, probably have to go native...
            Debug.WriteLine("tapped");
        }

        void PanGesture(object sender, PanUpdatedEventArgs e)
        {
            Debug.WriteLine(e.TotalX);
            Debug.WriteLine(e.TotalY);
        }
    }
}
