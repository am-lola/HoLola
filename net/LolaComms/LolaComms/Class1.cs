using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolaComms
{
    public class SampleClass
    {
        [DllImport("LolaCommsNative", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string SampleFunc(float f);

        [DllImport("LolaCommsNative")]
        public static extern bool init();

        [DllImport("LolaCommsNative")]
        public static extern IntPtr VisionListener_Create(int port);

        [DllImport("LolaCommsNative")]
        public static extern void VisionListener_Destroy(IntPtr vl);

        [DllImport("LolaCommsNative")]
        public static extern void VisionListener_Listen(IntPtr vl);

        [DllImport("LolaCommsNative")]
        public static extern bool VisionListener_IsListening(IntPtr vl);

        [DllImport("LolaCommsNative")]
        public static extern void VisionListener_Stop(IntPtr vl);

        public delegate void VisionListener_OnErrorCallback([MarshalAs(UnmanagedType.BStr)]string errstr);
        [DllImport("LolaCommsNative")]
        public static extern void VisionListener_OnError(IntPtr vl, VisionListener_OnErrorCallback callback);
    }
}
