using System;
using System.Runtime.InteropServices;

namespace LolaComms
{
    public static class Common
    {
        /// <summary>
        /// MsgIds taken from am2b_iface
        /// TODO: Generate this list
        /// </summary>
        [Flags]
        public enum MsgId : UInt32
        {
            SET_SSV                  = 0x80040203,
            SET_SSV_SEQ              = 0x80040204,
            SET_RESET_R_ODO          = 0x80040205,
            MODIFY_SSV               = 0x80040206,
            REMOVE_SSV_WHOLE_SEGMENT = 0x80040207,
            RESET_SSVMAP             = 0x80040208,
            REMOVE_SSV_ONLY_PART     = 0x80040209,

            SET_SURFACE              = 0x80040301,
            MODIFY_SURFACE           = 0x80040302,
            REMOVE_SURFACE           = 0x80040303,
            RESET_SURFACEMAP         = 0x80040304
        }

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
