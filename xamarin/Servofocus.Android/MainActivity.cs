using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Servofocus.Android.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer (typeof (Entry), typeof (UrlEntryRenderer))]

namespace Servofocus.Android
{
    [Activity(Label = "Servofocus.Droid", Icon = "@drawable/icon", Theme = "@style/AppTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        //private GestureDetector _gestureDetector;
        //private OverScroller _scroller;

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

            //_scroller = new OverScroller(this.Context);
            //_gestureDetector = new GestureDetector(this);
        }

        public override void OnBackPressed()
        {
            var page = (ServofocusPage)Xamarin.Forms.Application.Current.MainPage;
            if (!page.SystemGoBack()) {
                base.OnBackPressed();
            }
        }

        //public bool OnDown(MotionEvent e)
        //{
        //    System.Diagnostics.Debug.WriteLine("down: " + e);
        //    return true;
        //}

        //public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        //{
        //    System.Diagnostics.Debug.WriteLine("fling: " + e1 + "," + e2);
        //    return false;
        //}

        //public void OnLongPress(MotionEvent e)
        //{
        //    System.Diagnostics.Debug.WriteLine("LongPress: " + e);
        //}

        //public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        //{
        //    System.Diagnostics.Debug.WriteLine("Scroll: " + e1 + "," + e2);
        //    return true;
        //}

        //public void OnShowPress(MotionEvent e)
        //{
        //    System.Diagnostics.Debug.WriteLine("ShowPress: " + e);
        //}

        //public bool OnSingleTapUp(MotionEvent e)
        //{
        //    System.Diagnostics.Debug.WriteLine("TapUp: " + e);
        //    return false;
        //}

        //public override bool DispatchTouchEvent(MotionEvent ev)
        //{
        //    base.DispatchTouchEvent(ev);
        //    return _gestureDetector.OnTouchEvent(ev);
        //}
    }
}