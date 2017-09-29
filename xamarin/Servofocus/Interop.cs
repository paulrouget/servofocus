using System;
using System.Runtime.InteropServices;

namespace Servofocus
{
    public class Interop
    {
        [DllImport(Import.Servo, EntryPoint = "servo_version")]
        public static extern IntPtr ServoVersion();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MyCallback();

        [DllImport(Import.Servo, EntryPoint = "init")]
        public static extern void Init([MarshalAs(UnmanagedType.FunctionPtr)]MyCallback flush, 
                                       [MarshalAs(UnmanagedType.FunctionPtr)]MyCallback wakeUp,
                                       uint width, 
                                       uint height);  

        [DllImport(Import.Servo, EntryPoint = "ping")]
        public static extern void Ping();  
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