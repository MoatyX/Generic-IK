using System;
using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// safe Math functions
    /// </summary>
    public static class GenericMath
    {
        /// <summary>
        /// Apply the rotation through Quaternion Multipication
        /// </summary>
        /// <param name="_qA"></param>
        /// <param name="_qB"></param>
        /// <returns>the final quaternion from _qB applied over _qA</returns>
        public static Quaternion ApplyQuaternion(Quaternion _qA, Quaternion _qB)
        {
            Quaternion qr = Quaternion.identity;
            Vector3 va = new Vector3(_qA.x, _qA.y, _qA.z);
            Vector3 vb = new Vector3(_qB.x, _qB.y, _qB.z);
            qr.w = _qA.w * _qB.w - Vector3.Dot(va, vb);

            Vector3 vr = Vector3.Cross(va, vb) + _qA.w * vb + _qB.w * va;
            qr.x = vr.x;
            qr.y = vr.y;
            qr.z = vr.z;
            return qr;
        }

        /// <summary>
        /// Create a Quaternion from an axis and an angle
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Quaternion QuaternionFromAngleAxis(float angle, Vector3 axis)
        {
            Quaternion q = Quaternion.identity;

            axis.Normalize();
            angle *= Mathf.Deg2Rad;

            q.x = axis.x * Mathf.Sin(angle / 2f);
            q.y = axis.y * Mathf.Sin(angle / 2f);
            q.z = axis.z * Mathf.Sin(angle / 2f);
            q.w = Mathf.Cos(angle / 2f);

            return q;
        }

        /// <summary>
        /// Get the angle and the axis that makes up a Quaternion
        /// </summary>
        /// <param name="quaternion"></param>
        /// <param name="angle"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static Quaternion QuaternionToAngleAxis(Quaternion quaternion, out float angle, out Vector3 axis)
        {
            angle = 0f;
            axis = Vector3.zero;

            angle = 2 * Mathf.Acos(quaternion.w) * Mathf.Rad2Deg;
            axis.x = quaternion.x / Mathf.Sqrt(1 - Mathf.Pow(quaternion.w, 2f));
            axis.y = quaternion.y / Mathf.Sqrt(1 - Mathf.Pow(quaternion.w, 2f));
            axis.z = quaternion.z / Mathf.Sqrt(1 - Mathf.Pow(quaternion.w, 2f));

            return quaternion;
        }

        /// <summary>
        /// The angle between 2 vectors
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns>the angle between the vectors</returns>
        public static float VectorsAngle(Vector3 v0, Vector3 v1)
        {
            v0.Normalize();
            v1.Normalize();

            float _dot = Vector3.Dot(v0, v1);
            _dot = Mathf.Acos(Mathf.Clamp(_dot, -1f, 1f));

            return _dot * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Returns the angle between 2 rotations
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static float QuaternionAngle(Quaternion q1, Quaternion q2)
        {
            float dot = Quaternion.Dot(q1, q2);
            return 2f * Mathf.Acos(Mathf.Clamp01(dot)) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// the _source vector will rotate to point at the _target vector
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Quaternion RotateFromTo(Vector3 source, Vector3 target)
        {
            source.Normalize();
            target.Normalize();

            Vector3 axis = Vector3.Cross(target, source).normalized;
            float angle = VectorsAngle(source, target);

            Quaternion q = QuaternionFromAngleAxis(angle, axis);

            return q;
        }

        /// <summary>
        /// Rotate a vector by a quaternion
        /// </summary>
        /// <param name="v"></param>
        /// <param name="q"></param>
        /// <returns>the vector's new coordinates after being transformed by a quaternion</returns>
        public static Vector3 TransformVector(Vector3 v, Quaternion q)
        {
            Quaternion _qv = new Quaternion(v.x, v.y, v.z, 0f);
            Quaternion _qr = ApplyQuaternion(q, _qv);
            _qr = ApplyQuaternion(_qr, Quaternion.Inverse(q));
            return new Vector3(_qr.x, _qr.y, _qr.z);
        }

        /// <summary>
        /// Get the axis of self to target
        /// coordination system independent
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Vector3 GetLocalAxisToTarget(Transform self, Vector3 target)
        {
            Quaternion identity = Quaternion.Inverse(self.rotation);
            return TransformVector((target - self.position).normalized, identity);
        }

        /// <summary>
        /// check if the obj is inside the boundaries of the joint's rotation cone
        /// </summary>
        /// <param name="joint"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ConeBounded(Core.Joint joint, Vector3 obj)
        {
            Vector3 dir = obj - joint.pos;
            float angle = VectorsAngle(dir, joint.pos + TransformVector(joint.axis, joint.rot));
            return joint.maxAngle >= angle;
        }

        /// <summary>
        /// Get the next close point on the surface of the joint's rotation cone
        /// </summary>
        /// <param name="joint"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Vector3 GetConeNextPoint(Core.Joint joint, Vector3 obj)
        {
            //this algorithm is not as accurate as an optimization partial-differential problem, but its simple and straightforward.
            //accuracy doesn't matter in this case

            if (ConeBounded(joint, obj)) return obj;

            Vector3 jointPos = joint.pos;
            Vector3 dir = obj - jointPos;
            Vector3 axis = TransformVector(joint.axis, joint.rot);

            float currAngle = VectorsAngle(dir, jointPos + axis);
            float d = Mathf.Cos(currAngle * Mathf.Deg2Rad) * dir.magnitude;
            float x = d * (Mathf.Tan(currAngle * Mathf.Deg2Rad) - Mathf.Tan(joint.maxAngle * Mathf.Deg2Rad));

            Vector3 coneAxis = joint.joint.position + (TransformVector(axis * d, joint.rot));
            Vector3 rx = coneAxis - obj;
            float dot = Vector3.Dot(joint.joint.position + (TransformVector(axis, joint.rot)), dir.normalized);

            Vector3 point = (rx.normalized * x + obj) * Mathf.Clamp01(Mathf.Sign(dot)) + jointPos * Mathf.Clamp01(-Mathf.Sign(dot));
            return point;
        }

        /// <summary>
        /// A safer and more general clamp function than the built-in one
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable
        {
            if (value.CompareTo(min) < 0) return min;
            return value.CompareTo(max) > 0 ? max : value;
        }

        /// <summary>
        /// a simple Power function, since using the POW method could be expensive
        /// </summary>
        /// <param name="value"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static float SimplerPower(float value, ushort power)
        {
            float output = value;

            if (power == 0) return 1;
            for (var i = 0; i < power - 1; i++)
            {
                output *= output;
            }

            return output;
        }

        /// <summary>
        /// a general form of the cosine rule
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>the angle of the X variable makes with the other 2</returns>
        public static float CosineRule(float x, float y, float z)
        {
            float x2 = SimplerPower(x, 2);
            float y2 = SimplerPower(y, 2);
            float z2 = SimplerPower(z, 2);
            float value = (x2 + y2 - z2) / (2 * x * y);

            float angle = Mathf.Acos(Clamp(value, -1f, 1f)) * Mathf.Rad2Deg;
            return angle;
        }
    }
}
