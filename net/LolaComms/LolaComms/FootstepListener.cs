using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LolaComms
{
    class FootstepListener
    {
        private IntPtr _footstepListener = IntPtr.Zero; // pointer to native implementation
        private Queue<Footstep> _footsteps = new Queue<Footstep>();
        private object _footLock = new object();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void FootstepListener_OnErrorCallback([MarshalAs(UnmanagedType.BStr)]string errstr);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void FootstepListener_OnConnectCallback([MarshalAs(UnmanagedType.BStr)]string host);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void FootstepListener_OnDisconnectCallback([MarshalAs(UnmanagedType.BStr)]string host);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void FootstepListener_OnNewStepCallback(Footstep step);

        public FootstepListener_OnErrorCallback onError;
        public FootstepListener_OnConnectCallback onConnect;
        public FootstepListener_OnDisconnectCallback onDisonnect;
        public FootstepListener_OnNewStepCallback onNewStep;

        private FootstepListener_OnErrorCallback p_onError;
        private FootstepListener_OnConnectCallback p_onConnect;
        private FootstepListener_OnDisconnectCallback p_onDisconnect;
        private FootstepListener_OnNewStepCallback p_onNewStep;

        public FootstepListener(int port, string host)
        {
            _footstepListener = FootstepListener_Create(port, host);

            p_onError = new FootstepListener_OnErrorCallback(OnError);
            p_onConnect = new FootstepListener_OnConnectCallback(OnConnect);
            p_onDisconnect = new FootstepListener_OnDisconnectCallback(OnDisconnect);
            p_onNewStep = new FootstepListener_OnNewStepCallback(OnNewStep);

            FootstepListener_OnError(_footstepListener, p_onError);
            FootstepListener_OnConnect(_footstepListener, p_onConnect);
            FootstepListener_OnDisconnect(_footstepListener, p_onDisconnect);
            FootstepListener_OnNewStep(_footstepListener, p_onNewStep);
        }

        ~FootstepListener()
        {
            if (_footstepListener != IntPtr.Zero)
            {
                if (FootstepListener_IsListening(_footstepListener))
                {
                    FootstepListener_Stop(_footstepListener);
                }
                FootstepListener_Destroy(_footstepListener);
                _footstepListener = IntPtr.Zero;
            }
        }

        public void Listen()
        {
            FootstepListener_Listen(_footstepListener);
        }

        public void Stop()
        {
            FootstepListener_Stop(_footstepListener);
        }

        public bool Listening
        {
            get { return FootstepListener_IsListening(_footstepListener); }
        }

        /// <summary>
        /// To be called from Update() or FixedUpdate().
        /// Calls any outstanding callbacks from the calling thread.
        /// </summary>
        public void Process()
        {
            lock (_footLock)
            {
                foreach (var step in _footsteps)
                {
                    onNewStep?.Invoke(step);
                }
                _footsteps.Clear();
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
            onDisonnect?.Invoke(host);
        }

        private void OnNewStep(Footstep step)
        {
            lock (_footLock)
            {
                _footsteps.Enqueue(step);
            }
        }
#endregion

#region native imports
        /// <summary>
        /// Creates a new native FootstepListener instance
        /// </summary>
        /// <param name="port">Port to use for communication</param>
        /// <param name="hostname">Server to connect to for footstep data</param>
        /// <returns></returns>
        [DllImport("LolaCommsNative")]
        private static extern IntPtr FootstepListener_Create(int port, string hostname);

        /// <summary>
        /// Destroys existing native FootstepListener instance
        /// </summary>
        /// <param name="fl">Existing FootstepListener instance</param>
        [DllImport("LolaCommsNative")]
        private static extern void FootstepListener_Destroy(IntPtr fl);

        /// <summary>
        /// Tells FootstepListener to begin listening for incoming data
        /// </summary>
        /// <param name="fl">Existing FootstepListener instance</param>
        [DllImport("LolaCommsNative")]
        private static extern void FootstepListener_Listen(IntPtr fl);

        /// <summary>
        /// Queries FoootstepListener for its current listening state
        /// </summary>
        /// <param name="fl">Existing FootstepListener instance</param>
        /// <returns></returns>
        [DllImport("LolaCommsNative")]
        private static extern bool FootstepListener_IsListening(IntPtr fl);

        /// <summary>
        /// Shuts the FootstepListener down, closes any open connections, etc.
        /// </summary>
        /// <param name="fl"></param>
        [DllImport("LolaCommsNative")]
        private static extern void FootstepListener_Stop(IntPtr fl);

        /// <summary>
        /// Registers a callback to be called any time the FootstepListener encounters an error
        /// </summary>
        /// <param name="fl">Existing FootstepListener instance</param>
        /// <param name="callback"></param>
        [DllImport("LolaCommsNative")]
        private static extern void FootstepListener_OnError(IntPtr fl, FootstepListener_OnErrorCallback callback);

        /// <summary>
        /// Registers a callback to be called when the FootstepListener opens a new connection
        /// </summary>
        /// <param name="fl">Existing FootstepListener instance</param>
        /// <param name="callback"></param>
        [DllImport("LolaCommsNative")]
        private static extern void FootstepListener_OnConnect(IntPtr fl, FootstepListener_OnConnectCallback callback);

        /// <summary>
        /// Registers a callback to be called when the FootstepListener closes an open connection
        /// </summary>
        /// <param name="fl">Existing FootstepListener instance</param>
        /// <param name="callback"></param>
        [DllImport("LolaCommsNative")]
        private static extern void FootstepListener_OnDisconnect(IntPtr fl, FootstepListener_OnDisconnectCallback callback);

        /// <summary>
        /// Registers a callback to be called any time a new footstep is received
        /// </summary>
        /// <param name="fl">Existing FootstepListener instance</param>
        /// <param name="callback"></param>
        [DllImport("LolaCommsNative")]
        private static extern void FootstepListener_OnNewStep(IntPtr fl, FootstepListener_OnNewStepCallback callback);
#endregion

    }
}
