using Android.Support.V4.Content;
using Android.Widget;
using Servofocus;
using Servofocus.Android.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MenuButton), typeof(MenuButtonRenderer))]
namespace Servofocus.Android.Renderer
{
    public class MenuButtonRenderer : ViewRenderer<MenuButton, ImageButton>
    {
        PopupWindow _menu;
        protected override void OnElementChanged(ElementChangedEventArgs<MenuButton> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var button = new ImageButton(Context) { Background = null };

                    var menuImage = ContextCompat.GetDrawable(Context, Resource.Drawable.ic_action_more_vert);
                    button.SetImageDrawable(menuImage);
                    SetNativeControl(button);
                }
                
                _menu = new PopupWindow(Context)
                {
                    ContentView = CreateLayout()
                };

                Control.Click += (sender, args) =>
                {
                    _menu.ShowAsDropDown(Control);
                };
            }
        }

        LinearLayout CreateLayout()
        {
            var menuLayout = new LinearLayout(Context)
            {
                Orientation = Orientation.Vertical,
            };

            var forwardImage = ContextCompat.GetDrawable(Context, Resource.Drawable.ic_action_arrow_forward);
            var refreshImage = ContextCompat.GetDrawable(Context, Resource.Drawable.ic_action_refresh);

            var navButtonsLayout = new LinearLayout(Context)
            {
                Orientation = Orientation.Horizontal,
            };

            var buttonForward = new ImageButton(Context) { Background = null };
            buttonForward.SetImageDrawable(forwardImage);
            buttonForward.Click += (sender, args) => Element.GoForward();

            var buttonReload = new ImageButton(Context) { Background = null };
            buttonReload.SetImageDrawable(refreshImage);
            buttonReload.Click += (sender, args) => Element.Reload();

            navButtonsLayout.AddView(buttonForward);
            navButtonsLayout.AddView(buttonReload);

            menuLayout.AddView(navButtonsLayout);

            return menuLayout;
        }
    }     
}