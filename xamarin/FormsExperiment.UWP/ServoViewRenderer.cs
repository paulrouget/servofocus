using System.Diagnostics;
using Windows.UI.Xaml.Input;
using FormsExperiment;
using FormsExperiment.UWP;
using SkiaSharp.Views.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace FormsExperiment.UWP
{
    public class ServoViewRenderer : ViewRenderer<ServoView, AngleSwapChainPanel>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ServoView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var angleSwapChainPanelView = new AngleSwapChainPanel
                {
                    EnableRenderLoop = true,
                    Context = new GlesContext()
                };

                SetNativeControl(angleSwapChainPanelView);

                Subscribe();
            }
        }

        private void Subscribe()
        {
            Control.PointerEntered += HoverEvent;
            Control.PointerWheelChanged += WheelChanged;
        }

        private void WheelChanged(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            Debug.WriteLine("WheelChanged!");
        }

        private void Unsubscribe()
        {
            Control.PointerEntered -= HoverEvent;
            Control.PointerWheelChanged -= WheelChanged;
        }

        private void HoverEvent(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            Debug.WriteLine("Hover!");
        }
    }
}
