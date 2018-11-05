using System;
using UnityEngine;
using System.Collections.Generic;

namespace Generics.Dynamics
{
    public class Core
    {
        public enum Solvers
        {
            CyclicDescend,
            FastReach
        }

        /// <summary>
        /// Definition of a joint
        /// </summary>
        [Serializable]
        public class Joint
        {
            public enum MotionLimit
            {
                Full,
                FullRestricted,
                SingleDegree
            }

            public MotionLimit motionFreedom = MotionLimit.Full;

            public Transform joint;
            [Range(0f, 1f)] public float weight = 1f;

            public float length;

            /// <summary>
            /// The joint's position in the solver
            /// </summary>
            public Vector3 pos;

            /// <summary>
            /// The joint's rotation in the solver;
            /// </summary>
            public Quaternion rot;

            /// <summary>
            /// The local axis in a chain
            /// </summary>
            public Vector3 localAxis = Vector3.up;

            public Vector3 axis = Vector3.right;
            public Vector2 hingLimit = Vector2.one * 180f;
            public float maxAngle = 180f;
            public float maxTwist = 180f;

            /// <summary>
            /// Map the vitual joint to physical
            /// </summary>
            public void MapVirtual()
            {
                pos = joint.position;
                rot = joint.rotation;
            }

            public void ApplyVirtualMap(bool applyPos, bool applyRot)
            {
                if (applyPos) joint.position = pos;
                if (applyRot) joint.rotation = rot;
            }

            public Quaternion ApplyRestrictions()
            {
                switch (motionFreedom)
                {
                    case MotionLimit.Full:
                        return joint.localRotation;
                    case MotionLimit.FullRestricted:
                        return TwistAndSwing();
                    case MotionLimit.SingleDegree:
                        return SingleDegree();
                    default:
                        return joint.localRotation;
                }
            }

            /// <summary>
            /// The Full-Restricted motion limit
            /// </summary>
            /// <param name="_localRot"></param>
            /// <returns></returns>
            private Quaternion TwistAndSwing()
            {
                Func<Quaternion, float, Quaternion> LimitByAngle = (q, x) =>
                {
                    if (x == 0) return Quaternion.identity;

                    float angle = GenericMath.QuaternionAngle(Quaternion.identity, q);
                    float t = Mathf.Clamp01(x / angle);
                    Quaternion output = Quaternion.Slerp(Quaternion.identity, q, t); //lerp doesnt work :(
                    return output;
                };

                Func<float, float> Sqr = x => x * x;

                Vector3 _localAxis = GenericMath.TransformVector(axis, joint.localRotation);

                //swing only quaternion
                Quaternion swing = GenericMath.RotateFromTo(_localAxis, axis);
                Quaternion limitedSwing = LimitByAngle(swing, maxAngle);

                //twist only quaternion
                Quaternion twist = GenericMath.ApplyQuaternion(Quaternion.Inverse(swing), joint.localRotation);

                //twist decomposition
                float qM = Mathf.Sqrt(Sqr(twist.w) + Sqr(twist.x) + Sqr(twist.y) + Sqr(twist.z));
                float qw = twist.w / qM;
                float qx = twist.x / qM;
                float qy = twist.y / qM;
                float qz = twist.z / qM;

                Quaternion limitedTwist = LimitByAngle(new Quaternion(qx, qy, qz, qw), maxTwist);

                joint.localRotation = GenericMath.ApplyQuaternion(limitedTwist, limitedSwing);
                return joint.localRotation;
            }

            /// <summary>
            /// Limit the motion to 1 Degree of freedom
            /// </summary>
            /// <param name="_localRot"></param>
            /// <returns></returns>
            private Quaternion SingleDegree()
            {
                float angle;
                Vector3 axis;

                //Hinge only Quaternion
                Vector3 _localAxis = GenericMath.TransformVector(this.axis, joint.transform.localRotation);
                Quaternion _delta = GenericMath.RotateFromTo(this.axis, _localAxis);
                Quaternion _legalRot = GenericMath.ApplyQuaternion(_delta, joint.localRotation);

                GenericMath.QuaternionToAngleAxis(_legalRot, out angle, out axis);

                float min = hingLimit.x;
                float max = hingLimit.y;
                float dot = Vector3.Dot(this.axis, axis);

                //clamp values
                //angle = Mathf.Clamp(angle * dot, min, max);   //Unity's Clamp gives NaN values in particular cases so use our own
                angle = GenericMath.Clamp(angle * dot, min, max);

                joint.localRotation = GenericMath.QuaternionFromAngleAxis(angle, this.axis);
                return joint.localRotation;
            }
        }

        /// <summary>
        /// Definition of a chain
        /// </summary>
        [Serializable]
        public class Chain
        {
            public Transform Target;
            [Range(0f, 1f)] public float Weight;
            public int Iterations;
            public List<Joint> Joints = new List<Joint>();

            private Vector3 IKpos;

            public bool Initiated { get; private set; }
            public float ChainLength { get; private set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public Chain()
            {
                Iterations = 2; //fun
            }

            /// <summary>
            /// Make sure the joints are initiated correctly
            /// </summary>
            public void InitiateJoints()
            {
                MapVirtualJoints();

                for (int i = 0; i < Joints.Count - 1; i++)
                {
                    Joints[i].localAxis =
                        GenericMath.GetLocalAxisToTarget(Joints[i].joint, Joints[i + 1].joint.position);
                    Joints[i].length = Vector3.Distance(Joints[i].joint.position, Joints[i + 1].joint.position);
                    ChainLength += Joints[i].length;
                }

                Joints[Joints.Count - 1].localAxis =
                    GenericMath.GetLocalAxisToTarget(Joints[0].joint, Joints[Joints.Count - 1].joint.position);
                SetIKTarget(GetVirtualEE());

                Initiated = true;
            }

            /// <summary>
            /// Get the End Effector
            /// </summary>
            /// <returns></returns>
            public Transform GetEndEffector()
            {
                if (Joints.Count <= 0) return null;

                return Joints[Joints.Count - 1].joint;
            }

            /// <summary>
            /// Get the End Effector's virtual position in the solver
            /// </summary>
            /// <returns></returns>
            public Vector3 GetVirtualEE()
            {
                if (Joints.Count <= 0) return Vector3.zero;

                return Joints[Joints.Count - 1].pos;
            }

            /// <summary>
            /// Get the IK target of this chain
            /// </summary>
            /// <returns></returns>
            public Vector3 GetIKTarget()
            {
                return Target ? Target.position : IKpos;
            }

            /// <summary>
            /// Set a target position for the IK solver
            /// </summary>
            /// <param name="target"></param>
            /// <returns></returns>
            public Vector3 SetIKTarget(Vector3 target)
            {
                IKpos = this.Target ? this.Target.position : target;
                return IKpos;
            }

            public Quaternion SetEERotation(Quaternion target, bool relativeRot = false, bool applyLimits = false)
            {
                Joint ee = Joints[Joints.Count - 1];
                ee.joint.rotation = relativeRot ? ee.joint.rotation * target : target;

                ee.ApplyVirtualMap(false, true);
                if (applyLimits) ee.ApplyRestrictions();
                return ee.rot;
            }

            /// <summary>
            /// Map the vitual joints to the physical ones
            /// </summary>
            public void MapVirtualJoints()
            {
                for (int i = 0; i < Joints.Count; i++)
                {
                    Joints[i].MapVirtual();
                }
            }

            /// <summary>
            /// Apply the virtual pos and rot to the chain
            /// </summary>
            /// <param name="pos"></param>
            /// <param name="rot"></param>
            public void MapPhysicalJoints(bool pos = true, bool rot = true)
            {
                for (int i = 0; i < Joints.Count; i++)
                {
                    Joints[i].ApplyVirtualMap(pos, rot);
                }
            }
        }

        /// <summary>
        /// Dynamic Chain
        /// </summary>
        [Serializable]
        public class KinematicChain
        {
            [Header("Chain")] [Range(0f, 1f)] public float weight = 1f;
            public List<Joint> joints = new List<Joint>();

            [Header("Interafce")] public AnimationCurve solverFallOff;
            public Vector3 gravity = Vector3.down;
            public float momentOfInteria = 1000f;
            public float stiffness = -0.1f;
            public float torsionDamping = 0f;


            public bool initiated { get; private set; }
            public Quaternion[] initLocalRot { get; private set; }
            public Vector3[] prevPos { get; private set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public KinematicChain()
            {
                momentOfInteria = 1000;
                stiffness = -0.1f;
            }

            /// <summary>
            /// Make sure the joints are initiated correctly
            /// </summary>
            public void InitiateJoints()
            {
                initLocalRot = new Quaternion[joints.Count];
                prevPos = new Vector3[joints.Count];

                int i = 0;

                for (i = 0; i < joints.Count - 1; i++)
                {
                    joints[i].MapVirtual();
                    joints[i].localAxis =
                        GenericMath.GetLocalAxisToTarget(joints[i].joint, joints[i + 1].joint.position);
                    joints[i].length = Vector3.Distance(joints[i].joint.position, joints[i + 1].joint.position);

                    initLocalRot[i] = joints[i].joint.localRotation;
                    prevPos[i] = joints[i].joint.position;
                }

                joints[i].MapVirtual();
                initLocalRot[i] = joints[i].joint.localRotation;
                prevPos[i] = joints[i].joint.position;

                joints[i].localAxis = GenericMath.GetLocalAxisToTarget(joints[0].joint, joints[i].joint.position);
                initiated = true;
            }

            /// <summary>
            /// Map the vitual joints to the physical ones
            /// </summary>
            public void MapVirtualJoints()
            {
                for (int i = 0; i < joints.Count; i++)
                {
                    joints[i].joint.localRotation = initLocalRot[i];
                }
            }
        }
    }
}