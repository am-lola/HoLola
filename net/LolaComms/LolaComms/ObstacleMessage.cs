using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LolaComms
{
    public enum ObstacleType
    {
        Sphere = 0,
        Capsule,
        Triangle
    };

    /// <summary>
    /// C# implementation of am2b_iface::ObstacleMessage
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ObstacleMessage
    {
        public ObstacleType type;    // sphere, capsule, etc
        public UInt32 model_id;
        public UInt32 part_id;
        public UInt32 action; // add, remove, modify, ...
        public float radius;
        public Int32 surface;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] coeffs;

        public override string ToString()
        {
            string res = "";
            res += type.ToString();
            res += " " + model_id + "|" + part_id;
            res += " 0x" + action.ToString("x2");
            res += " (" + radius + ")";
            res += " [" +
                coeffs[0] + " " +
                coeffs[1] + " " +
                coeffs[2] + " " +
                coeffs[3] + " " +
                coeffs[4] + " " +
                coeffs[5] + " " +
                coeffs[6] + " " +
                coeffs[7] + " " +
                coeffs[8] + "]";
            return res;
        }
    }
}
