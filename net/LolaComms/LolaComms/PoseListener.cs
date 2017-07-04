using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LolaComms
{
    public class PoseListener
    {
        private int _port;
        private IntPtr _poseListener = IntPtr.Zero; // pointer to native implementation

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PoseListener_OnErrorCallback([MarshalAs(UnmanagedType.BStr)]string errstr);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PoseListener_OnNewPoseCallback(HR_Pose_Red pose);

        public PoseListener_OnErrorCallback onError;
        public PoseListener_OnNewPoseCallback onNewPose;

        private PoseListener_OnErrorCallback p_onError;
        private PoseListener_OnNewPoseCallback p_onNewPose;

        public PoseListener(int port)
        {
            _port = port;
            _poseListener = PoseListener_Create(_port);

            /// Ensure callbacks we pass to native code are allocated outside our class
            /// see issue #11
            p_onError = new PoseListener_OnErrorCallback(OnError);
            p_onNewPose = new PoseListener_OnNewPoseCallback(OnNewPose);

            PoseListener_OnError(_poseListener, p_onError);
            PoseListener_OnNewPose(_poseListener, p_onNewPose);
        }

        ~PoseListener()
        {
            if (_poseListener != IntPtr.Zero)
            {
                if (PoseListener_IsListening(_poseListener))
                {
                    PoseListener_Stop(_poseListener);
                }
                PoseListener_Destroy(_poseListener);
                _poseListener = IntPtr.Zero;
            }
        }

        public void Listen()
        {
            PoseListener_Listen(_poseListener);
        }

        public void Stop()
        {
            PoseListener_Stop(_poseListener);
        }

        public bool Listening
        {
            get { return PoseListener_IsListening(_poseListener); }
        }

#region private callbacks
        private void OnError(string error)
        {
            onError?.Invoke(error);
        }

        private void OnNewPose(HR_Pose_Red pose)
        {
            onNewPose?.Invoke(pose);
        }
        #endregion

        #region native imports
        /// <summary>
        /// Creates a new native PoseListener
        /// </summary>
        /// <param name="port">Port to listen on for pose data</param>
        /// <returns>Pointer to new PoseListener instance</returns>
        [DllImport("LolaCommsNative")]
        private static extern IntPtr PoseListener_Create(int port);

        /// <summary>
        /// Destroys an existing native PoseListener instance
        /// </summary>
        /// <param name="pl">Pointer to existing PoseListener instance</param>
        [DllImport("LolaCommsNative")]
        private static extern void PoseListener_Destroy(IntPtr pl);

        /// <summary>
        /// Tells PoseListener to begin listening for data
        /// </summary>
        /// <param name="pl">Pointer to existing PoseListener instance</param>
        [DllImport("LolaCommsNative")]
        private static extern void PoseListener_Listen(IntPtr pl);

        /// <summary>
        /// Queries PoseListener for its current listening state
        /// </summary>
        /// <param name="pl">Pointer to existing PoseListener instance</param>
        /// <returns></returns>
        [DllImport("LolaCommsNative")]
        private static extern bool PoseListener_IsListening(IntPtr pl);

        /// <summary>
        /// Shuts PoseListener down, closes open connections, etc.
        /// </summary>
        /// <param name="pl">Pointer to existing PoseListener instance</param>
        [DllImport("LolaCommsNative")]
        private static extern void PoseListener_Stop(IntPtr pl);

        /// <summary>
        /// Registers callback to be called any time PoseListener encounters an error
        /// </summary>
        /// <param name="pl">Pointer to existing PoseListener instance</param>
        /// <param name="callback"></param>
        [DllImport("LolaCommsNative")]
        private static extern void PoseListener_OnError(IntPtr pl, PoseListener_OnErrorCallback callback);

        /// <summary>
        /// Registers a callback to call when new pose data is received
        /// </summary>
        /// <param name="pl">Pointer to existing PoseListener instance</param>
        /// <param name="callback"></param>
        [DllImport("LolaCommsNative")]
        private static extern void PoseListener_OnNewPose(IntPtr pl, PoseListener_OnNewPoseCallback callback);

        #endregion
    }
}
