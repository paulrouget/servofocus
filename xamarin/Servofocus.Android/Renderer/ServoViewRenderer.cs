using System;
using Android.Opengl;
using Android.Views;
using Android.Util;
using Javax.Microedition.Khronos.Opengles;
using Servofocus;
using Servofocus.Android.Renderer;
using ServoSharp;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using Android.Widget;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Android.Graphics;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Android.Renderer
{

    public class ServoViewRenderer : ViewRenderer<ServoView, GLSurfaceView>, GestureDetector.IOnGestureListener
    {
        bool _disposed;

        class Renderer : Java.Lang.Object, GLSurfaceView.IRenderer
        {
            readonly ServoView _servoView;

            public Renderer(ServoView servoView)
            {
                _servoView = servoView;
            }

            public void OnDrawFrame(IGL10 gl)
            {
            }

            public void OnSurfaceChanged(IGL10 gl, int width, int height)
            {
                _servoView.Servo.Resize((uint)height, (uint)width);
            }

            public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
            {
                _servoView.Servo.InitWithEgl();
            }
        }

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

                    Element.Servo.SetHostCallbacks(
                        wakeUp: action => Control.QueueEvent(action),
                        flush: () => Control.RequestRender()
                    );

                    var renderer = new Renderer(Element);

                    surfaceView.SetRenderer(renderer);
                    SetNativeControl(surfaceView);

                    mGestureDetector = new GestureDetector(this);
                    mScroller = new OverScroller(Context);

                    // Element.Servo.Click((uint)x, (uint)(y - Element.Bounds.Top * 4));
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
                        Element.Servo.Scroll(0, -delta, 0, 0, ServoSharp.ScrollState.Move);
                        FlingRun();
                    });
                });
            }
            else
            {
                mFlinging = false;
                System.Diagnostics.Debug.WriteLine("SCROLL END");
                Element.Servo.Scroll(0, 0, 0, 0, ServoSharp.ScrollState.End);
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
                        // FIXME: click maybe
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
                        Element.Servo.Scroll(0, -delta, 0, 0, ServoSharp.ScrollState.Move);
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
            Element.Servo.Scroll(0, 0, 0, 0, ServoSharp.ScrollState.Start);
        }

        private void EndTouch()
        {
            mTouchState = TOUCH_STATE_RESTING;
        }


    }

}