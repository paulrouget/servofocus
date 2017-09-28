using System;
using System.Runtime.InteropServices;

namespace FormsExperiment
{
    public class Interop
    {   
        [DllImport(Import.MacOSLib, EntryPoint = "servo_version")]
        public static extern IntPtr ServoVersion();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MyCallback();

        [DllImport(Import.MacOSLib, EntryPoint = "init")]
        public static extern void Init([MarshalAs(UnmanagedType.FunctionPtr)]MyCallback flush, 
                                       [MarshalAs(UnmanagedType.FunctionPtr)]MyCallback wakeUp,
                                       uint width, 
                                       uint height);  

        [DllImport(Import.MacOSLib, EntryPoint = "ping")]
        public static extern void Ping();  

    }

    internal static class Import
    {
        public const string MacOSLib = "libsimpleservo.dylib";
    }
}
