using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LolaComms
{
    /// <summary>
    /// C# implementation of am2b_iface::SurfaceMessage
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SurfaceMessage
    {
        public UInt32 id;
        public UInt32 action; // add, remove, modify, ...
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] normal;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)] // 8 vertices
        public float[] vertices;

        public override string ToString()
        {
            string res = "";
            res += id.ToString();
            res += " " + (Common.MsgId)action;
            res += " [" +
                normal[0] + " " +
                normal[1] + " " +
                normal[2] + "]";
            res += " {";
            for (int i = 0; i < 24; i+=3)
            {
                res += " ["
                    + vertices[i] + ", "
                    + vertices[i+1] + ", "
                    + vertices[i+2] + "]";
            }
            res += "}";
            return res;
        }
    }
}
