using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// a solver suited for 3 joints configuration
    /// </summary>
    public static class AnalyticalSolver
    {
        private static float _eps = 0.01f;

        /// <summary>
        /// A small margin that ensures that the chain is not fully contracted/stretched.
        /// default = 0.01f
        /// </summary>
        public static float Epsilon
        {
            get { return _eps; }
            set { _eps = value; }
        }

        /// <summary>
        /// Process a 2 bones chain with a specific "epsilon" value
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="eps">a specific value, not bounded to the global Epsilon</param>
        public static void Process(Core.Chain chain, float eps)
        {
            if (chain.Initiated == false)
            {
                chain.InitiateJoints();
            }

            if (chain.Joints.Count != 3)
            {
                Debug.LogError("The Analytical Solver only works with 3-joints(2 bones) chain configurations");
                return;
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
            float l_at = GenericMath.Clamp(TA.magnitude, eps, l_ab + l_cb - eps);

            float kneeCurrent = GenericMath.VectorsAngle(AB, CB);
            float kneeTarget = GenericMath.CosineRule(A.length, B.length, l_at);
            float kneeDelta = kneeTarget - kneeCurrent;

            Vector3 axis = GenericMath.TransformVector(Vector3.Normalize(Vector3.Cross(AC, AB)),
                Quaternion.Inverse(B.joint.rotation));
            Quaternion q1 = Quaternion.AngleAxis(kneeDelta, axis);

            Quaternion knee = Quaternion.Lerp(B.joint.rotation, GenericMath.ApplyQuaternion(B.joint.rotation, q1),
                chain.Weight);
            B.joint.rotation = knee;

            Quaternion q2 = Quaternion.FromToRotation(A.joint.position - C.joint.position, TA);
            Quaternion thigh = Quaternion.Lerp(A.joint.rotation, GenericMath.ApplyQuaternion(q2, A.joint.rotation),
                chain.Weight);
            A.joint.rotation = thigh;
        }

        /// <summary>
        /// Process a 2 bones chain through the Cosine Rule
        /// </summary>
        /// <param name="chain"></param>
        public static void Process(Core.Chain chain)
        {
            Process(chain, Epsilon);
        }
    }
}