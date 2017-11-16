using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer (typeof (Entry), typeof (MyEntryRenderer))]
public class MyEntryRenderer : EntryRenderer
{
    protected override void OnElementChanged (ElementChangedEventArgs<Entry> e)
    {
        base.OnElementChanged (e);
        if (e.OldElement == null) {
            var nativeEditText = (Android.Widget.EditText)Control;
            nativeEditText.SetSelectAllOnFocus (true);
        }
    }
}

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
            var page = (Servofocus.ServofocusPage)App.Current.MainPage;
            if (!page.SystemGoBack()) {
                base.OnBackPressed();
            }
        }
    }

}
