using Xamarin.Forms;
using System.Diagnostics;

namespace FormsExperiment
{
    public partial class FormsExperimentPage : ContentPage
    {
        public FormsExperimentPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("OnAppearing");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
        }
    }
}
