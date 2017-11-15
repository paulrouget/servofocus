using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Java.Lang;

namespace Servofocus.Android
{
    [Activity(Label = "Servofocus.Droid", Icon = "@drawable/icon", Theme = "@style/AppTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            Window.SetBackgroundDrawableResource(Resource.Drawable.background_gradient);
            SetStatusBarColor(global::Android.Graphics.Color.Transparent);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());

            Runtime.GetRuntime().LoadLibrary("c++_shared");
        }
    }
}
