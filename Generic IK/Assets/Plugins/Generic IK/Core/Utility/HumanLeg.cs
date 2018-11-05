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

        public Core.Chain Chain;


        [Header("Terrain Adjustment")]
        public float FootOffset = 0f;
        public float MaxStepHeight = 0.8f;

        //used to smooth out the motion when we cant find ground anymore
        public float EaseOutPos = 10f;
        public float EaseOutNormals = 10f;

        private Vector3 normals;
        private Vector3 IKPointOffset;

        private Quaternion _EEAnimRot;
        private Quaternion _EETargetRot;
        private Transform EE;

        private bool _init;

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
                    Chain.Joints = tempR.Joints;
                    break;
                case HumanLegs.LeftLeg:
                    var tempL = rigReader.BuildChain(LegType);
                    Chain.Joints = tempL.Joints;
                    break;
            }

            Init();
        }

        private void Init()
        {
            if (Chain.Joints == null || Chain.Joints.Count != 3)
            {
                Debug.LogError("Fetal !: the chain joints are undefined");
                return;
            }

            Chain.InitiateJoints();
            Chain.Weight = 1;

            _EEAnimRot = Chain.GetEndEffector().rotation;
            EE = Chain.GetEndEffector();

            _init = true;
        }

        /// <summary>
        /// Cast rays to find pumps in the terrain and sets the IK target to the appropriate hit point.
        /// (does not solve the IK, you need to Call a Solver separately)
        /// (The AnalyticalSolver is suggested)
        /// </summary>
        public void TerrainAdjustment(LayerMask mask, Transform root)
        {
            if (_init == false)
            {
                Init();
                return;
            }

            RaycastHit hit;
            Vector3 rootUp = root.up;
            Ray ray = new Ray(EE.position, Vector3.down);
            bool intersect = Physics.Raycast(ray, out hit, MaxStepHeight, mask, QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
            if (intersect)
            {
                //Debug.DrawLine(ray.origin, hit.point, Color.green); //enable for debug purposes
            }
#endif
            if (intersect)
            {
                float footHeight = root.position.y - EE.position.y;
                float footFromGround = hit.point.y - root.position.y;

                float offsetTarget = Mathf.Clamp(footFromGround, -MaxStepHeight, MaxStepHeight) + FootOffset;
                float currentMaxOffset = Mathf.Clamp(MaxStepHeight - footHeight, 0f, MaxStepHeight);
                float IK = Mathf.Clamp(offsetTarget, -currentMaxOffset, offsetTarget);

                IKPointOffset = rootUp * IK;
                normals = Vector3.Lerp(normals, hit.normal, Time.deltaTime * EaseOutNormals);
            }
            else
            {
                IKPointOffset = Vector3.Lerp(IKPointOffset, Vector3.zero, Time.deltaTime * EaseOutPos);
                normals = Vector3.Lerp(normals, rootUp, Time.deltaTime * EaseOutNormals);
            }

            Chain.SetIKTarget(EE.position + IKPointOffset);

            //calculate the ankle rot, before applying the IK
            _EETargetRot = GenericMath.RotateFromTo(normals, rootUp);
            _EEAnimRot = EE.rotation;
        }


        /// <summary>
        /// Rotate the ankle
        /// </summary>
        public void RotateAnkle()
        {
            Quaternion rot = GenericMath.ApplyQuaternion(_EETargetRot, _EEAnimRot);
            Quaternion targetRot = Quaternion.Lerp(EE.rotation, rot, Chain.Weight);
            EE.rotation = targetRot;
        }
    }
}