using System;
using System.Runtime.InteropServices;

namespace LolaComms
{
    public static class Common
    {
        /// <summary>
        /// Register a callback function here to receive debugging output from the native layer.
        /// </summary>
        /// <param name="text"></param>
        public delegate void LolaLogInfoCallback([MarshalAs(UnmanagedType.BStr)]string text);
        [DllImport("LolaCommsNative")]
        public static extern string RegisterInfoCallback(LolaLogInfoCallback callback);

        /// <summary>
        /// Must be called before using any native networking capabilities
        /// </summary>
        /// <returns>TRUE on success, FALSE if the sockets library could not be initialized</returns>
        [DllImport("LolaCommsNative")]
        public static extern bool Init();

        /// <summary>
        /// Should be called when native networking capabilities are no longer necessary.
        /// </summary>
        /// <returns>TRUE if everything was cleaned up successfully, FALSE if an error occurred.</returns>
        [DllImport("LolaCommsNative")]
        public static extern bool DeInit();
    }
}
