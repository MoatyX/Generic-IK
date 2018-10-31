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

            Core.Joint A = chain.Joints[0]; //thigh
            Core.Joint B = chain.Joints[1]; //knee
            Core.Joint C = chain.Joints[2]; //foot
            Vector3 T = chain.GetIKTarget();

            Vector3 TB = T - B.joint.position;
            Vector3 CB = Vector3.Normalize(B.joint.position - C.joint.position);

            Vector3 BA = Vector3.Normalize(A.joint.position - B.joint.position);
            Vector3 CA = Vector3.Normalize(A.joint.position - C.joint.position);
            Vector3 TA = Vector3.Normalize(A.joint.position - T);

            //hip rotation (2-DOF rotation)

            //knee rotation (1-DOF rotation)
            //Vector3 kneeAxis = GenericMath.TransformVector(Vector3.Cross(BA, CB), Quaternion.Inverse(B.joint.rotation));
            //float kneeAngle = Vector3.Angle(GenericMath.TransformVector(B.localAxis, B.joint.rotation), TB.normalized);
            Vector3 kneeAxis = GenericMath.TransformVector(Vector3.Cross(BA, CB), Quaternion.Inverse(B.joint.rotation));
            float kneeCur = Vector3.Angle(BA, CB);
            float kneeAngle = Vector3.Angle(GenericMath.TransformVector(B.localAxis, B.joint.rotation), TB.normalized) - 180f;
            Quaternion knee = GenericMath.QuaternionFromAngleAxis(kneeAngle - kneeCur, kneeAxis);

            B.joint.rotation = GenericMath.ApplyQuaternion(B.joint.rotation, knee);

            Quaternion thigh = Quaternion.FromToRotation(TA, Vector3.Normalize(A.joint.position - C.joint.position));
            A.joint.rotation = A.joint.rotation * thigh;
        }

        //private static void old(Core.Chain chain)
        //{
        //    Core.Joint A = chain.Joints[0];
        //    Core.Joint B = chain.Joints[1];
        //    Core.Joint C = chain.Joints[2];
        //    Vector3 T = chain.GetIKTarget();

        //    Vector3 BA = A.joint.position - B.joint.position;
        //    Vector3 CA = C.joint.position - A.joint.position;
        //    Vector3 CB = B.joint.position - C.joint.position;
        //    Vector3 TB = B.joint.position - T;
        //    Vector3 TA = A.joint.position - T;

        //    float LT = GenericMath.Clamp(TA.magnitude, Epsilon, A.length + B.length - Epsilon);
        //    float L1T = GenericMath.Clamp(TB.magnitude, Epsilon, TB.magnitude);

        //    float alpha0 = Vector3.Angle(BA, CA);
        //    float beta0 = Vector3.Angle(BA, CB);

        //    float alpha1 = GenericMath.CosineRule(A.length, L1T, LT);
        //    float beta1 = GenericMath.CosineRule(A.length, LT, L1T);

        //    float alphaT = alpha1 - alpha0;
        //    float betaT = beta0 - beta1;

        //    Vector3 betaAxis = Vector3.Cross(BA, CB).normalized;

        //    Debug.Log(betaAxis);
        //    B.joint.rotation = Quaternion.AngleAxis(Vector3.Angle(B.joint.rotation * B.localAxis, TB), B.joint.rotation * betaAxis);
        //}
    }
}