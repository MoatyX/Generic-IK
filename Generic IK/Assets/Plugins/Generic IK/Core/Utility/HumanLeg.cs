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


        [Header("Terrain Adjustment")]
        public float HealHeight = 0.11f;
        public float RayLength = 0.5f;


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
                    LegChain = rigReader.BuildChain(LegType);
                    break;
                case HumanLegs.LeftLeg:
                    LegChain = rigReader.BuildChain(LegType);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Cast rays to find pumps in the terrain and sets the IK target to the appropriate hit point.
        /// (does not solve the IK, you need to process the chain in a solver-call separately)
        /// </summary>
        public void TerrainAdjustment(LayerMask mask)
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
                LegChain.Weight = 1f;

                Vector3 rootUp = LegChain.Joints[0].joint.up;
                Quaternion footRot = Quaternion.FromToRotation(rootUp, hit.normal);

                EE.rotation = EE.rotation * footRot;

                Vector3 IKPoint = hit.point + hit.normal * HealHeight;
                LegChain.SetIKTarget(IKPoint);
            }
            else
            {
                LegChain.Weight = 0f;
                LegChain.SetIKTarget(EE.position);
            }
        }

        /// <summary>
        /// find the height difference between 2 legs in their root's local space
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static float operator -(HumanLeg self, HumanLeg other)
        {
            Transform selfRoot = self.LegChain.Joints[0].joint;
            Transform otherRoot = other.LegChain.Joints[0].joint;

            Vector3 x = selfRoot.position - self.LegChain.GetIKTarget();
            Vector3 y = otherRoot.position - other.LegChain.GetIKTarget();

            float dotX = Vector3.Dot(selfRoot.rotation * Vector3.down, x);
            float dotY = Vector3.Dot(otherRoot.rotation * Vector3.down, y);

            return Mathf.Abs(dotX) - Mathf.Abs(dotY);
        }
    }
}