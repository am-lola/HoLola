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
        private int _port;
        private IntPtr _visionListener = IntPtr.Zero; // pointer to native implementation
        private Queue<ObstacleMessage> _obstacles = new Queue<ObstacleMessage>();
        private Object _obsLock = new Object();

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

        /// <summary>
        /// To be called from Update() or FixedUpdate().
        /// Calls any outstanding callbacks from the calling thread.
        /// </summary>
        public void Process()
        {
            lock (_obsLock)
            {
                foreach (var obstacle in _obstacles)
                {
                    onObstacleMessage?.Invoke(obstacle);
                }
                _obstacles.Clear();
            }
            
        }

#region private callbacks
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
            lock (_obsLock)
            {
                _obstacles.Enqueue(msg);
            }
        }
#endregion

#region native imports
        /// <summary>
        /// Creates a new native VisionListener
        /// </summary>
        /// <param name="port">Port to listen on for vision data</param>
        /// <returns>Pointer to new VisionListener instance</returns>
        [DllImport("LolaCommsNative")]
        private static extern IntPtr VisionListener_Create(int port);

        /// <summary>
        /// Destroys an existing native VisionListener instance
        /// </summary>
        /// <param name="vl">Pointer to existing instance</param>
        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_Destroy(IntPtr vl);

        /// <summary>
        /// Tells VisionListener to begin listening for data
        /// </summary>
        /// <param name="vl">Pointer to existing VisionListener interface</param>
        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_Listen(IntPtr vl);

        /// <summary>
        /// Queries VisionListener for its current listening state
        /// </summary>
        /// <param name="vl">Pointer to existing VisionListener interface</param>
        /// <returns></returns>
        [DllImport("LolaCommsNative")]
        private static extern bool VisionListener_IsListening(IntPtr vl);

        /// <summary>
        /// Shuts VisionListener down, closes open connections, kills listening thread, etc
        /// </summary>
        /// <param name="vl">Pointer to existing VisionListener interface</param>
        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_Stop(IntPtr vl);

        /// <summary>
        /// Registers a function to call when an error occurs inside the listener
        /// </summary>
        /// <param name="vl">Pointer to existing VisionListener interface</param>
        /// <param name="callback"></param>
        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_OnError(IntPtr vl, VisionListener_OnErrorCallback callback);

        /// <summary>
        /// Registers a function to call when a new connection is established
        /// </summary>
        /// <param name="vl">Pointer to existing VisionListener interface</param>
        /// <param name="callback"></param>
        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_OnConnect(IntPtr vl, VisionListener_OnConnectCallback callback);

        /// <summary>
        /// Registers a function to call when a connection is terminated
        /// </summary>
        /// <param name="vl">Pointer to existing VisionListener interface</param>
        /// <param name="callback"></param>
        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_OnDisconnect(IntPtr vl, VisionListener_OnDisconnectCallback callback);

        /// <summary>
        /// Registers a function to call when a new ObstacleMessage is received
        /// </summary>
        /// <param name="vl">Pointer to existing VisionListener interface</param>
        /// <param name="callback"></param>
        [DllImport("LolaCommsNative")]
        private static extern void VisionListener_OnObstacleMessage(IntPtr vl, VisionListener_OnObstacleMessageCallback callback);
#endregion
    }
}
