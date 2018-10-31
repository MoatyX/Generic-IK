using System;
using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// Dynamically reposition the legs on slopy surfaces
    /// </summary>
    public class HumanoidVerticalLegPlacement : MonoBehaviour
    {
        /// <summary>
        /// A leg object
        /// </summary>
        internal class Leg
        {
            public readonly Core.Chain _legChain;
            private readonly float _healHeight;

            public Leg(Func<Core.Chain> targetLeg, float healHeight)
            {
                _legChain = targetLeg.Invoke();
                _legChain.Iterations = 5;

                _healHeight = healHeight;
            }

            /// <summary>
            /// Process the leg
            /// </summary>
            public void Update(Vector3 fwdDir, LayerMask mask)
            {
                _legChain.SetIKTarget(Vector3.zero);
            }

            /// <summary>
            /// find the height difference between 2 legs in their root's local space
            /// </summary>
            /// <param name="self"></param>
            /// <param name="other"></param>
            /// <returns></returns>
            public static float operator -(Leg self, Leg other)
            {
                Transform selfRoot = self._legChain.Joints[0].joint;
                Transform otherRoot = other._legChain.Joints[0].joint;

                Vector3 x = selfRoot.position - self._legChain.GetIKTarget();
                Vector3 y = otherRoot.position - other._legChain.GetIKTarget();

                float dotX = Vector3.Dot(selfRoot.rotation * Vector3.down, x);
                float dotY = Vector3.Dot(otherRoot.rotation * Vector3.down, y);

                return Math.Abs(dotX) - Math.Abs(dotY);
            }
        }

        [Header("Interface")] public Animator SourceAnimator;
        public float DefaultHealHeight = 0.1f;
        public LayerMask LayerMask = 0;
        public Transform target;

        private Leg _right, _left;
        private Transform _root;
        public Core.Chain Chain;

        private void Start()
        {
            if (SourceAnimator == null) SourceAnimator = GetComponent<Animator>();
            if (SourceAnimator == null)
            {
                Debug.LogWarning("No Source Animator was found. it is an important step to initialization");
                enabled = false;
                return;
            }

            RigReader rigReader = new RigReader(SourceAnimator);

            _root = rigReader.h_root.joint;
            _right = new Leg(rigReader.RightLegChain, DefaultHealHeight);
            _left = new Leg(rigReader.LeftLegChain, DefaultHealHeight);
            Chain = rigReader.RightLegChain();
            Chain.Target = target;
        }

        private void LateUpdate()
        {
            AnalyticalSolver.Process(Chain);
        }
    }
}