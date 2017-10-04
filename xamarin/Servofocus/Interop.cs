using System;
using System.Runtime.InteropServices;

namespace Servofocus
{
    public class Interop
    {
        [DllImport(Import.Servo, EntryPoint = "servo_version")]
        public static extern IntPtr ServoVersion();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void InitCallback();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void LogCallback(IntPtr str);

        [DllImport(Import.Servo, EntryPoint = "init_with_egl")]
        public static extern void InitWithEgl([MarshalAs(UnmanagedType.FunctionPtr)]InitCallback init,
                                       [MarshalAs(UnmanagedType.FunctionPtr)]LogCallback log,
                                       uint width,
                                       uint height);  

        [DllImport(Import.Servo, EntryPoint = "on_event_loop_awaken_by_servo")]
        public static extern void OnEventLoopAwakenByServo();  
    }

    internal static class Import
    {
#if __ANDROID__
        public const string Servo = "libsimpleservo.so";
#elif __IOS__ || __UNIFIED__
        public const string Servo = "libsimpleservo.dylib";
#endif
    }
}