using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// a solver suited for 3 joints configuration
    /// </summary>
    public static class AnalyticalSolver
    {
        private const float Eps = 0.01f;

        /// <summary>
        /// Process the chain through the Cosine Rule
        /// </summary>
        /// <param name="fwdDir"></param>
        /// <param name="chain"></param>
        public static void Process(Core.Chain chain)
        {
            if (chain.Initiated == false)
            {
                chain.InitiateJoints();
            }

            Core.Joint A = chain.Joints[0];
            Core.Joint B = chain.Joints[1];
            Core.Joint C = chain.Joints[2];
            Vector3 T = chain.GetIKTarget();

            Vector3 AB = Vector3.Normalize(B.joint.position - A.joint.position);
            Vector3 AC = Vector3.Normalize(C.joint.position - A.joint.position);
            Vector3 CB = Vector3.Normalize(B.joint.position - C.joint.position);
            Vector3 TA = A.joint.position - T;

            float l_ab = A.length;
            float l_cb = B.length;
            float l_at = GenericMath.Clamp(TA.magnitude, Eps, l_ab + l_cb - Eps);

            float kneeCurrent = GenericMath.VectorsAngle(AB, CB);
            float kneeTarget = GenericMath.CosineRule(A.length, B.length, l_at);

            Vector3 axis = Vector3.Normalize(Vector3.Cross(AC, -AB));
            Quaternion q1 = Quaternion.AngleAxis(kneeTarget - kneeCurrent, Quaternion.Inverse(B.joint.rotation) * axis);

            B.joint.localRotation = B.joint.localRotation * Quaternion.Inverse(q1);

            Quaternion q2 = Quaternion.FromToRotation(A.joint.position - C.joint.position, TA);
            A.joint.rotation = q2 * A.joint.rotation;
        }
    }
}