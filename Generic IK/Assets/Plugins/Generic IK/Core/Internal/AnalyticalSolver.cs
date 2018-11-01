using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// a solver suited for 3 joints configuration
    /// </summary>
    public static class AnalyticalSolver
    {
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

            float eps = 0.01f;
            float lab = A.length;
            float lcb = B.length;
            float lat = GenericMath.Clamp(TA.magnitude, eps, lab + lcb - eps);

            float ba_bc_0 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(-AB, -CB), -1f, 1f));
            float ba_bc_1 = Mathf.Acos(Mathf.Clamp((lat * lat - lab * lab - lcb * lcb) / (-2 * lab * lcb), -1, 1));

            Vector3 axis = Vector3.Normalize(Vector3.Cross(AC, -AB));
            Quaternion q1 = Quaternion.AngleAxis((ba_bc_1 - ba_bc_0) * Mathf.Rad2Deg, Quaternion.Inverse(B.joint.rotation) * axis);


            B.joint.localRotation = B.joint.localRotation * Quaternion.Inverse(q1);

            Quaternion q2 = Quaternion.FromToRotation(A.joint.position - C.joint.position, A.joint.position - T);
            A.joint.rotation = A.joint.rotation * Quaternion.Inverse(q2);
        }
    }
}