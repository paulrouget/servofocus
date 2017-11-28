using System;
using System.ComponentModel;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Servofocus;
using Servofocus.Android.Renderer;
using Servofocus.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MenuButton), typeof(MenuButtonRenderer))]
namespace Servofocus.Android.Renderer
{
    public class MenuButtonRenderer : ViewRenderer<MenuButton, ImageButton>
    {
        PopupWindow _menu;
        MainViewModel _viewModel;
        ImageButton _buttonForward;
        ImageButton _buttonReload;
        ImageButton _buttonBackward;
        ImageButton _buttonStop;

        protected override void OnElementChanged(ElementChangedEventArgs<MenuButton> e)
        {
            base.OnElementChanged(e);
            
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var button = new ImageButton(Context) { Background = null };
                    _viewModel = (MainViewModel) Element.BindingContext;
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

                _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            }
        }

        void ViewModelOnPropertyChanged(object o, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case nameof(_viewModel.CanGoForward):
                    _buttonForward.Visibility = _viewModel.CanGoForward ? ViewStates.Visible : ViewStates.Gone;
                    break;
                case nameof(_viewModel.CanStop):
                    _buttonStop.Visibility = _viewModel.CanStop ? ViewStates.Visible : ViewStates.Gone;
                    break;
                case nameof(_viewModel.CanReload):
                    _buttonReload.Visibility = _viewModel.CanReload ? ViewStates.Visible : ViewStates.Gone;
                    break;
            }
        }

        LinearLayout CreateLayout()
        {
            var menuLayout = new LinearLayout(Context)
            {
                Orientation = Orientation.Vertical,
            };

            var forwardImage = ContextCompat.GetDrawable(Context, Resource.Drawable.ic_action_arrow_forward);
            var reloadImage = ContextCompat.GetDrawable(Context, Resource.Drawable.ic_action_refresh);
            var stopImage = ContextCompat.GetDrawable(Context, Resource.Drawable.ic_action_stop);
            
            var navButtonsLayout = new LinearLayout(Context)
            {
                Orientation = Orientation.Horizontal,
            };

            _buttonForward = new ImageButton(Context) { Background = null };
            _buttonForward.SetImageDrawable(forwardImage);
            _buttonForward.Click += (sender, args) => _viewModel.GoForward();
            
            _buttonReload = new ImageButton(Context) { Background = null };
            _buttonReload.SetImageDrawable(reloadImage);
            _buttonReload.Click += (sender, args) => _viewModel.Reload();

            _buttonStop = new ImageButton(Context) { Background = null };
            _buttonStop.SetImageDrawable(stopImage);
            _buttonStop.Click += (sender, args) => _viewModel.Stop();

            navButtonsLayout.AddView(_buttonForward);
            navButtonsLayout.AddView(_buttonReload);
            navButtonsLayout.AddView(_buttonStop);

            menuLayout.AddView(navButtonsLayout);

            return menuLayout;
        }
    }     
}