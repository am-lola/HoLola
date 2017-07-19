using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LolaComms
{
    /// <summary>
    /// Pose data structs matching those defined
    /// in iface_vis.h
    /// </summary>

    /*!
        Robot pose data

        vectors given in world coordinate frame

        ub = upper body
        fl = left foot
        fr = right foot
        cl = left camera
        cr = right camera
        im = IMU,

        wr = world frame
        odo = drift frame
    */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HR_Pose
    {
        public enum SegmentIndex
        {
            //torso
            seg_torso = 0,
            //pelvis
            seg_pelvis_rot, seg_pelvis_add,
            //right leg
            seg_hip_rot_r, seg_hip_add_r, seg_hip_flx_r, seg_knee_flx_r, seg_ankle_add_r, seg_ankle_flx_r, seg_toe_flx_r,
            //left leg
            seg_hip_rot_l, seg_hip_add_l, seg_hip_flx_l, seg_knee_flx_l, seg_ankle_add_l, seg_ankle_flx_l, seg_toe_flx_l,
            //right arm
            seg_shoulder_flx_r, seg_shoulder_add_r, seg_elbow_flx_r,
            //left arm
            seg_shoulder_flx_l, seg_shoulder_add_l, seg_elbow_flx_l,
            //head (tilt: both cameras without convergence joint)
            seg_head_pan, seg_head_tilt
        };

        //!segment pose
        public struct SegmentPose
        {
            //!transform matrix
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            float[] R;
            //!position
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            float[] t;
        };

        //////////////////////////////////////////////////
        //// 1 -- header
        //!data struct version
        public UInt32 version;
        //! tick counter
        public UInt64 tick_counter;
        //!<stance leg (RIGHT/LEFT)
        public byte stance;
        //!<padding
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] zero;

        public UInt64 stamp;


        //////////////////////////////////////////////////
        //// 2 -- simplified /abstract robot model (feet, cameras, upper body)
        //!vector from world frame to left leg in world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] t_wr_fr;
        //!vector from world frame to right leg in world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] t_wr_fl;
        //!vector from world frame to left camera in world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] t_wr_cl;
        //!vector from world frame to right camera in world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] t_wr_cr;

        //!transformation matrix from left leg to world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] R_wr_fr;
        //!transformation matrix from right leg to world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] R_wr_fl;
        //!transformation matrix from left camera to world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] R_wr_cl;
        //!transformation matrix from right camera to world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] R_wr_cr;
        /*!
          transformation matrix from upper body coordinate frame
          to inertial frame measured by IMU (world frame)
          (identity matrix, if robot is standing upright)
        */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] R_wr_ub;
        //!vector from world frame to upper body frame in world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] t_wr_ub;

        //////////////////////////////////////////////////
        //// 3 -- full robot pose 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
        public SegmentPose[] seg_pose;

        //////////////////////////////////////////////////
        //// 4 -- "drift pose" (odometry)
        //!stance leg in drift frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] t_stance_odo;
        //!stance foot rotation in drift frame
        public float phi_z_odo;

        //////////////////////////////////////////////////
        //// 5 -- "drift pose" (odometry)
        //!<currently active velocity in x-direction [m/s]
        public float vx_act;
        //!<currently active velocity in y-direction [m/s]
        public float vy_act;
        //!<currently active angular velocity [rad/s]
        public float om_act;
    }

    /*!
        Robot pose data - new version for sending reduced packages to vision system

        vectors given in world coordinate frame

        ub = upper body
        fl = left foot
        fr = right foot
        cl = left camera
        cr = right camera
        im = IMU,

        wr = world frame
        odo = drift frame
    */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HR_Pose_Red
    {
        //////////////////////////////////////////////////
        //// 1 -- header
        //!data struct version
        public UInt32 version;
        //! tick counter
        public UInt64 tick_counter;
        //!<stance leg (RIGHT/LEFT)
        public byte stance;

        public UInt64 stamp;

        //////////////////////////////////////////////////
        //// 2 -- simplified /abstract robot model (feet, cameras, upper body)
        //!vector from world frame to left camera in world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] t_wr_cl;
        //!transformation matrix from left camera to world frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public float[] R_wr_cl;

        //////////////////////////////////////////////////
        //// 4 -- "drift pose" (odometry)
        //!stance leg in drift frame
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] t_stance_odo;
        //!stance foot rotation in drift frame
        public float phi_z_odo;
    };
}
