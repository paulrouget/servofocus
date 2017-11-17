﻿using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Servofocus.Android.Renderer
{
    public class UrlEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged (ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged (e);
            if (e.OldElement == null) {
                var nativeEditText = (global::Android.Widget.EditText)Control;
                nativeEditText.SetSelectAllOnFocus (true);
            }
        }
    }
}