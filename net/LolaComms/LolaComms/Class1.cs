using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolaComms
{
    
    public class VisionListener
    {
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

        public delegate void VisionListener_OnConnectCallback([MarshalAs(UnmanagedType.BStr)]string errstr);
        [DllImport("LolaCommsNative")]
        public static extern void VisionListener_OnConnect(IntPtr vl, VisionListener_OnConnectCallback callback);

        public delegate void VisionListener_OnDisconnectCallback([MarshalAs(UnmanagedType.BStr)]string errstr);
        [DllImport("LolaCommsNative")]
        public static extern void VisionListener_OnDisconnect(IntPtr vl, VisionListener_OnDisconnectCallback callback);

        public delegate void VisionListener_OnObstacleMessageCallback(ObstacleMessage obstacle);
        [DllImport("LolaCommsNative")]
        public static extern void VisionListener_OnObstacleMessage(IntPtr vl, VisionListener_OnObstacleMessageCallback callback);
    }
}
