using System;
using System.Runtime.InteropServices;

namespace LolaComms
{
    public enum Foot
    {
        Left = 0,
        Right
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Footstep
    {    // a subset of the data from am2b_iface::struct_data_stepseq_ssv_log
        UInt64 stamp_gen;

        float L0;
        float L1;
        float B;
        float phiO;
        float dz_clear;
        float dz_step;
        float dy;
        float H;
        float T; // step time    

        float start_x;
        float start_y;
        float start_z;
        float start_phi_z;
        float phi_leg_rel;
        Int32 stance;
    }
}
