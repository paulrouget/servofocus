using System;
using Android.Opengl;
using Android.Views;
using Android.Util;
using Javax.Microedition.Khronos.Opengles;
using Servofocus;
using Servofocus.Android.Renderer;
using Servofocus.Views;
using ServoSharp;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.Threading;
using Android.Widget;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Android.Renderer
{
    public class ServoViewRenderer : ViewRenderer<ServoView, GLSurfaceView>, GestureDetector.IOnGestureListener
    {
        bool _disposed;
        MainViewModel ViewModel;

        protected override void OnElementChanged(ElementChangedEventArgs<ServoView> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                GLSurfaceView surfaceView = Control;
                if (surfaceView == null)
                {
                    surfaceView = new GLSurfaceView(Context);
                    surfaceView.SetEGLContextClientVersion(3);
                    surfaceView.SetEGLConfigChooser(8, 8, 8, 8, 24, 0);

                    ViewModel = (MainViewModel) Element.BindingContext;
                    ViewModel.SetHostCallbacks(action => Control.QueueEvent(action), () => Control.RequestRender());
                    
                    var renderer = new Renderer((height, width) => ViewModel.Resize(height, width), () => ViewModel.InitWithEgl() );

                    surfaceView.SetRenderer(renderer);
                    SetNativeControl(surfaceView);


                    mGestureDetector = new GestureDetector(this);
                    mScroller = new OverScroller(Context);
                }
                Control.RenderMode = Rendermode.WhenDirty;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        class Renderer : Java.Lang.Object, GLSurfaceView.IRenderer
        {
            readonly Action<uint, uint> _resize;
            readonly Action _initWithEgl;

            public Renderer(Action<uint,uint> resize, Action initWithEgl)
            {
                _resize = resize;
                _initWithEgl = initWithEgl;
            }

			public void OnDrawFrame(IGL10 gl)
			{
			}

			public void OnSurfaceChanged(IGL10 gl, int width, int height)
			{
                _resize((uint)height, (uint)width);
			}

			public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
            {
               _initWithEgl();
            }
        }


        private GestureDetector mGestureDetector;
        private OverScroller mScroller;
        private int mTouchStartY;
        private int mTouchState = TOUCH_FIRST_STATE;
        private static int TOUCH_STATE_SCROLL = 2;
        private static int TOUCH_FIRST_STATE = -1;
        private static int TOUCH_STATE_RESTING = 0;
        private const int TOUCH_STATE_CLICK = 1;

        private int mLastY = 0;
        private bool mFlinging;

        public bool OnDown(MotionEvent e)
        {
            mScroller.ForceFinished(true);
            return true;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            mScroller.Fling(
                0,
                (int)mLastY,
                (int)velocityX,
                (int)velocityY,
                int.MinValue,
                int.MaxValue,
                int.MinValue,
                int.MaxValue);
            mFlinging = true;
            FlingRun();
            return true;
        }

        public void FlingRun()
        {
            if (!mScroller.IsFinished)
            {
                mScroller.ComputeScrollOffset();
                var delta = mLastY - mScroller.CurrY;
                mLastY = mScroller.CurrY;
                System.Threading.Tasks.Task.Factory.StartNew(() => {
                    // FIXME: NO!!!
                    Thread.Sleep(15);
                    Device.BeginInvokeOnMainThread(() => {
                        ViewModel.Scroll(0, -delta, 0, 0, ServoSharp.ScrollState.Move);
                        FlingRun();
                    });
                });
            }
            else
            {
                mFlinging = false;
                System.Diagnostics.Debug.WriteLine("SCROLL END");
                ViewModel.Scroll(0, 0, 0, 0, ServoSharp.ScrollState.End);
            }
        }

        public void OnLongPress(MotionEvent e)
        {
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return true;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            ViewModel.Click((uint)e.GetX(), (uint)e.GetY());
            return false;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            mGestureDetector.OnTouchEvent(e);
            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                    StartTouch(e);
                    break;
                case MotionEventActions.Up:
                    if (mTouchState == TOUCH_STATE_CLICK)
                    {
                        // We rely on OnSingleTapUp
                    }
                    EndTouch();
                    break;
                case MotionEventActions.Move:
                    if (mTouchState == TOUCH_STATE_CLICK)
                    {
                        StartScrollIfNeeded(e);
                    }
                    if (mTouchState == TOUCH_STATE_SCROLL)
                    {
                        int delta = (int)(mLastY - e.GetY());
                        mLastY = (int)e.GetY();
                        //System.Diagnostics.Debug.WriteLine("SCROLL MOVE: " + -delta);
                        ViewModel.Scroll(0, -delta, 0, 0, ServoSharp.ScrollState.Move);
                    }
                    break;
                default:
                    EndTouch();
                    break;
            }
            return true;
        }

        private bool StartScrollIfNeeded(MotionEvent e)
        {
            mLastY = (int)e.GetY();
            mTouchState = TOUCH_STATE_SCROLL;
            return true;
        }

        private void StartTouch(MotionEvent e)
        {
            mTouchStartY = (int)e.GetY();
            mTouchState = TOUCH_STATE_CLICK;
            System.Diagnostics.Debug.WriteLine("SCROLL START");
            ViewModel.Scroll(0, 0, 0, 0, ServoSharp.ScrollState.Start);
        }

        private void EndTouch()
        {
            mTouchState = TOUCH_STATE_RESTING;
        }
    }
}