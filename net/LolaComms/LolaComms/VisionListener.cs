using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolaComms
{
    /// <summary>
    /// Managed wrapper for C++ implementation of vision communication.
    /// Handles management of native resources and synchronization with Unity render thread.
    /// </summary>
    public class VisionListener
    {
        public delegate void VisionListener_OnErrorCallback([MarshalAs(UnmanagedType.BStr)]string errstr);
        public delegate void VisionListener_OnConnectCallback([MarshalAs(UnmanagedType.BStr)]string errstr);
        public delegate void VisionListener_OnDisconnectCallback([MarshalAs(UnmanagedType.BStr)]string errstr);
        public delegate void VisionListener_OnObstacleMessageCallback(ObstacleMessage obstacle);

        public VisionListener_OnErrorCallback onError;
        public VisionListener_OnConnectCallback onConnect;
        public VisionListener_OnDisconnectCallback onDisconnect;
        public VisionListener_OnObstacleMessageCallback onObstacleMessage;

        public VisionListener(int port)
        {
            _port = port;
            _visionListener = VisionListener_Create(_port);
            VisionListener_OnError(_visionListener, OnError);
            VisionListener_OnConnect(_visionListener, OnConnect);
            VisionListener_OnDisconnect(_visionListener, OnDisconnect);
            VisionListener_OnObstacleMessage(_visionListener, OnObstacleMessage);
        }

        ~VisionListener()
        {
            if (_visionListener != IntPtr.Zero)
            {
                if (VisionListener_IsListening(_visionListener))
                {
                    VisionListener_Stop(_visionListener);
                }
                VisionListener_Destroy(_visionListener);
                _visionListener = IntPtr.Zero;
            }
        }

        public void Listen()
        {
            VisionListener_Listen(_visionListener);
        }

        public void Stop()
        {
            VisionListener_Stop(_visionListener);
        }

        public bool Listening
        {
            get { return VisionListener_IsListening(_visionListener); }
        }

        private void OnError(string error)
        {
            onError?.Invoke(error);
        }

        private void OnConnect(string host)
        {
            onConnect?.Invoke(host);
        }

        private void OnDisconnect(string host)
        {
            onDisconnect?.Invoke(host);
        }

        private void OnObstacleMessage(ObstacleMessage msg)
        {
            onObstacleMessage?.Invoke(msg);
        }

        private int    _port;
        private IntPtr _visionListener = IntPtr.Zero; // pointer to native implementation


        [DllImport("LolaCommsNative")]
        private static extern IntPtr VisionListener_Create(int port);

        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_Destroy(IntPtr vl);

        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_Listen(IntPtr vl);

        [DllImport("LolaCommsNative")]
        private static extern bool VisionListener_IsListening(IntPtr vl);

        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_Stop(IntPtr vl);

        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_OnError(IntPtr vl, VisionListener_OnErrorCallback callback);

        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_OnConnect(IntPtr vl, VisionListener_OnConnectCallback callback);

        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_OnDisconnect(IntPtr vl, VisionListener_OnDisconnectCallback callback);

        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_OnObstacleMessage(IntPtr vl, VisionListener_OnObstacleMessageCallback callback);
    }
}
