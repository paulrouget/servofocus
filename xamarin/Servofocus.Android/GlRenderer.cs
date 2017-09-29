using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;

namespace Servofocus.Android
{
    public class GlRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
    {
        public void OnDrawFrame(IGL10 gl)
        {
        }

        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
        }

        public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
        }
    }
}