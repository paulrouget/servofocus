using Android.App;
using Android.Content.PM;
using Android.OS;
using Java.Lang;
using Servofocus.Android.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Servofocus.Android
{
    [Activity(Label = "Servofocus.Droid", Icon = "@drawable/icon", Theme = "@style/AppTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Window.SetBackgroundDrawableResource(Resource.Drawable.background_gradient);
            SetStatusBarColor(global::Android.Graphics.Color.Transparent);

            Forms.Init(this, bundle);

            LoadApplication(new App());

            Runtime.GetRuntime().LoadLibrary("c++_shared");
        }

        public override void OnBackPressed()
        {
            var page = (ServofocusPage)Xamarin.Forms.Application.Current.MainPage;
            if (!page.SystemGoBack())
            {
                base.OnBackPressed();
            }
        }
    }
}