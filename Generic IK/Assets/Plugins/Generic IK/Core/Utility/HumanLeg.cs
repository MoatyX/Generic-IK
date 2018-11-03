using System;
using UnityEngine;


namespace Generics.Dynamics
{
    /// <summary>
    /// A leg object
    /// </summary>
    [Serializable]
    public class HumanLeg
    {
        public enum HumanLegs
        {
            RightLeg = 4,
            LeftLeg = 5
        }


        [Header("General")]
        [Tooltip("Used only if Auto-building the chain is wished through the HumanLeg.AutoBuild() call")]
        public HumanLegs LegType;

        public Core.Chain LegChain;


        [Header("Terrain Adjustment")] public float HealHeight = 0.11f;
        public float RayLength = 0.5f;
        public float MaxStep = 0.5f;
        public bool intersecting;

        private Quaternion startRot;

        /// <summary>
        /// Automatically build the chain
        /// </summary>
        /// <param name="anim"></param>
        public void AutoBuild(Animator anim)
        {
            if (anim == null)
            {
                Debug.LogError("The Animator component passed is NULL");
                return;
            }

            RigReader rigReader = new RigReader(anim);

            switch (LegType)
            {
                case HumanLegs.RightLeg:
                    var tempR = rigReader.BuildChain(LegType);
                    LegChain.Joints = tempR.Joints;
                    break;
                case HumanLegs.LeftLeg:
                    var tempL = rigReader.BuildChain(LegType);
                    LegChain.Joints = tempL.Joints;
                    break;
            }

            LegChain.InitiateJoints();
            LegChain.Weight = 1;
            startRot = LegChain.GetEndEffector().rotation;
        }

        /// <summary>
        /// Cast rays to find pumps in the terrain and sets the IK target to the appropriate hit point.
        /// (does not solve the IK, you need to Call a Solver separately)
        /// (The AnalyticalSolver is suggested)
        /// </summary>
        public void TerrainAdjustment(LayerMask mask, Transform root)
        {
            Transform EE = LegChain.GetEndEffector();
            RaycastHit hit;
            Ray ray = new Ray(EE.position, Vector3.down);
            bool intersect = Physics.Raycast(ray, out hit, RayLength, mask, QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
            if (intersect)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }
#endif
            if (intersect)
            {
                Vector3 rootUp = root.up;
                Vector3 chainRoot = LegChain.Joints[0].joint.position;

                float footHeight = root.position.y - EE.position.y;
                float footFromGround = hit.point.y - root.position.y;
                intersecting = MaxStep > footFromGround;

                float offsetTarget = Mathf.Clamp(footFromGround, -MaxStep, MaxStep) + HealHeight;
                float currentMaxOffset = Mathf.Clamp(MaxStep - footHeight, 0f, MaxStep);
                float IK = Mathf.Clamp(offsetTarget, -currentMaxOffset, offsetTarget);

                Vector3 IKPoint = EE.position + rootUp * IK;
                LegChain.SetIKTarget(IKPoint);

                //TODO: rotate foot
            }
            else
            {
                LegChain.SetIKTarget(EE.position);
            }
        }
    }
}