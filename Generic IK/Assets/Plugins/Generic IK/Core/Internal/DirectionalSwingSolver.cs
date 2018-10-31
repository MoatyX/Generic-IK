using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// Rotate joints to make the End Effecotr look at a direction
    /// </summary>
    public static class DirectionalSwingSolver
    {
        /// <summary>
        /// Solve the cain
        /// </summary>
        /// <param name="chain">The chain required</param>
        /// <param name="lookAtAxis">Which axis of the end effector to consider</param>
        public static void Process(Core.Chain chain, Vector3 lookAtAxis)
        {
            Process(chain, lookAtAxis, chain.GetEndEffector());
        }

        /// <summary>
        /// Solve the cain
        /// </summary>
        /// <param name="chain">The chain required</param>
        /// <param name="lookAtAxis">Which axis of the end effector to consider</param>
        /// <param name="virtualEndEffector">Offset the end effector by this Transform (optional)</param>
        public static void Process(Core.Chain chain, Vector3 lookAtAxis, Transform virtualEndEffector)
        {
            Transform offset = virtualEndEffector ?? chain.GetEndEffector();
            for (int i = 0; i < chain.Iterations; i++)
            {
                Solve(chain, offset, lookAtAxis);
            }
        }

        /// <summary>
        /// Solve the chain to make the offset look at the target
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="endEffector"></param>
        private static void Solve(Core.Chain chain, Transform endEffector, Vector3 LookAtAxis)
        {
            for (int i = 0; i < chain.Joints.Count; i++)
            {
                //Vector3 axis = GenericMath.TransformVector(LookAtAxis, Quaternion.Inverse(offsetObj.rotation));
                Vector3 axis = GenericMath.TransformVector(LookAtAxis, endEffector.rotation);
                Quaternion delta = GenericMath.RotateFromTo(chain.GetIKTarget() - endEffector.position, axis);
                Quaternion final = Quaternion.Lerp(Quaternion.identity, delta, chain.Weight * chain.Joints[i].weight);

                chain.Joints[i].joint.rotation = GenericMath.ApplyQuaternion(final, chain.Joints[i].joint.rotation);
                chain.Joints[i].ApplyRestrictions();
            }
        }
    }
}
