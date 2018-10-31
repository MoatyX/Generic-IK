using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// A simple iterative solver
    /// </summary>
    public static class CyclicDescendSolver
    {
        /// <summary>
        /// Solve the IK for a chain
        /// </summary>
        /// <param name="chain"></param>
        public static bool Process(Core.Chain chain)
        {
            if (chain.Joints.Count <= 0) return false;

            chain.MapVirtualJoints();

            for (int j = 0; j < chain.Iterations; j++)
            {
                for (int i = chain.Joints.Count - 1; i >= 0; i--)
                {
                    float _weight = chain.Weight * chain.Joints[i].weight;  //relative weight

                    //direction vectors
                    Vector3 _v0 = chain.GetIKTarget() - chain.Joints[i].joint.position;
                    Vector3 _v1 = chain.Joints[chain.Joints.Count - 1].joint.position - chain.Joints[i].joint.position;

                    //Weight
                    Quaternion _sourceRotation = chain.Joints[i].joint.rotation;
                    Quaternion _targetRotation = Quaternion.Lerp(Quaternion.identity, GenericMath.RotateFromTo(_v0, _v1), _weight);

                    //Rotation Math
                    chain.Joints[i].rot = Quaternion.Lerp(_sourceRotation, GenericMath.ApplyQuaternion(_targetRotation, _sourceRotation), _weight);
                    chain.Joints[i].ApplyVirtualMap(false, true);
                    chain.Joints[i].ApplyRestrictions();
                }
            }

            return true;
        }
    }
}
