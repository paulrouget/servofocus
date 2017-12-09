using Android.Content.Res;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Views;
using Servofocus.Android.Renderer;
using Servofocus.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(EraseFloatingButton), typeof(EraseButtonRenderer))]

namespace Servofocus.Android.Renderer
{
    public class EraseButtonRenderer : ViewRenderer<EraseFloatingButton, FloatingActionButton>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<EraseFloatingButton> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var vm = (MainViewModel) e.NewElement.BindingContext;
                    
                    var eraseButton = new FloatingActionButton(Context);
                    var menuImage = ContextCompat.GetDrawable(Context, Resource.Drawable.ic_action_erase);
                    eraseButton.Click += vm.AndroidErase;
                    eraseButton.SetImageDrawable(menuImage);
                    SetNativeControl(eraseButton);
                }
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);
            Control.BringToFront();
        }
    }
}