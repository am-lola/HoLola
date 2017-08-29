using System;
using System.Runtime.InteropServices;

namespace LolaComms
{
    public enum Foot
    {
        Left = 0,
        Right
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Footstep
    {    // a subset of the data from am2b_iface::struct_data_stepseq_ssv_log
        public UInt64 stamp_gen;

        public float L0;
        public float L1;
        public float B;
        public float phiO;
        public float dz_clear;
        public float dz_step;
        public float dy;
        public float H;
        public float T; // step time    

        public float start_x;
        public float start_y;
        public float start_z;
        public float start_phi_z;
        public float phi_leg_rel;
        public Int32 stance;
    }
}
