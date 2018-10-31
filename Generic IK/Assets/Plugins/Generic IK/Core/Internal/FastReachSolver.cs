using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// Reachs a target as fast as possiable
    /// </summary>
    public static class FastReachSolver
    {

        /// <summary>
        /// Apply IK
        /// </summary>
        /// <param name="chain"></param>
        public static bool Process(Core.Chain chain)
        {
            if (chain.Joints.Count <= 0) return false;

            if (!chain.Initiated) chain.InitiateJoints();

            chain.MapVirtualJoints();

            for (int i = 0; i < chain.Iterations; i++)
            {
                SolveInward(chain);
                SolveOutward(chain);
            }

            MapSolverOutput(chain);

            return true;
        }

        /// <summary>
        /// Find the virtual new solved position of joints in the chain inward
        /// </summary>
        /// <param name="chain"></param>
        public static void SolveInward(Core.Chain chain)
        {
            int c = chain.Joints.Count;

            //Use Weight first
            chain.Joints[c - 1].pos = Vector3.Lerp(chain.GetVirtualEE(), chain.GetIKTarget(), chain.Weight);

            //find the joint on the chain's virtual line
            for (int i = c - 2; i >= 0; i--)
            {
                Vector3 _p = chain.Joints[i + 1].pos;   //point 
                Vector3 _d = chain.Joints[i].pos - _p;  //direction

                _d.Normalize();
                _d *= Vector3.Distance(chain.Joints[i + 1].joint.position, chain.Joints[i].joint.position);   //all points in a direction along a length

                chain.Joints[i].pos = _p + _d;
            }
        }

        /// <summary>
        /// Find the virtual new solved position of joints in the chain outward
        /// </summary>
        /// <param name="chain"></param>
        public static void SolveOutward(Core.Chain chain)
        {
            chain.Joints[0].pos = chain.Joints[0].joint.position;

            for (int i = 1; i < chain.Joints.Count; i++)
            {
                Vector3 _p = chain.Joints[i - 1].pos;   //point
                Vector3 _d = chain.Joints[i].pos - _p;  //direction

                _d.Normalize();
                _d *= Vector3.Distance(chain.Joints[i - 1].joint.position, chain.Joints[i].joint.position);

                chain.Joints[i].pos = _p + _d;
            }
        }

        /// <summary>
        /// Map the vitual solver's joints onto the physical ones
        /// </summary>
        /// <param name="chain"></param>
        public static void MapSolverOutput(Core.Chain chain)
        {
            for (int i = 0; i < chain.Joints.Count - 1; i++)
            {
                Vector3 _v1 = chain.Joints[i + 1].pos - chain.Joints[i].pos;
                Vector3 _v2 = GenericMath.TransformVector(chain.Joints[i].localAxis, chain.Joints[i].rot);

                Quaternion _offset = GenericMath.RotateFromTo(_v1, _v2);
                chain.Joints[i].rot = GenericMath.ApplyQuaternion(_offset, chain.Joints[i].rot);
                chain.Joints[i].ApplyVirtualMap(true, true);
            }
        }
    }
}
