﻿using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// Dynamically reposition the legs on slopy surfaces
    /// </summary>
    public class HumanoidVerticalLegPlacement : MonoBehaviour
    {
        [Header("Interface")]
        public Animator SourceAnimator;
        public float TimeTarget = 0.2f;

        [Tooltip("This uses the LegType parameter inside each Leg object and auto build it")]
        public bool AutoBuildChain = true;

        public float RootAdjSpeed = 7;
        public LayerMask LayerMask = 0;

        public Transform Root;
        public HumanLeg Right;
        public HumanLeg Left;

        private Vector3 rootPos;

        private void Start()
        {
            if (SourceAnimator == null) SourceAnimator = GetComponent<Animator>();
            if (SourceAnimator == null)
            {
                Debug.LogError("No Source Animator was found. it is an important step to initialization");
                enabled = false;
                return;
            }

            if (SourceAnimator.isHuman == false)
            {
                Debug.LogError(this + " works only for humanoid characters");
                enabled = false;
                return;
            }

            RigReader rigReader = new RigReader(SourceAnimator);
            Root = rigReader.Root.joint;
            rootPos = Root.position;

            if (AutoBuildChain)
            {
                Right.AutoBuild(SourceAnimator);
                Left.AutoBuild(SourceAnimator);
            }
        }

        private void LateUpdate()
        {
            ProcessLegs();
            ProcessHips();
            Solve();
        }

        private void ProcessLegs()
        {
            Right.TerrainAdjustment(LayerMask, transform);
            Left.TerrainAdjustment(LayerMask, transform);
        }

        private void ProcessHips()
        {
            //TODO: better pelvis adj
            float yRight = Right.LegChain.GetIKTarget().y;
            float yLeft = Left.LegChain.GetIKTarget().y;
            float min = Mathf.Min(yRight, yLeft);
            float max = Mathf.Max(yRight, yLeft);
            float delta = max - min;
            float extraDelta = Right.LegChain.GetEndEffector().position.y - Right.LegChain.GetIKTarget().y;
            extraDelta = 0f;

            rootPos.x = Root.position.x;
            rootPos.y = Mathf.Lerp(rootPos.y, Root.position.y - (delta + extraDelta), Time.deltaTime * RootAdjSpeed);
            rootPos.z = Root.position.z;

            Root.position = rootPos;
        }

        private void Solve()
        {
            AnalyticalSolver.Process(Right);
            AnalyticalSolver.Process(Left);

            Right.RotateFoot();
            Left.RotateFoot();
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false) return;
            
            Gizmos.color = Color.green;
            Vector3 ppos1 = Right.LegChain.GetIKTarget();
            Vector3 ppos2 = Left.LegChain.GetIKTarget();
            Gizmos.DrawWireSphere(ppos1, 0.06f);
            Gizmos.DrawWireSphere(ppos2, 0.06f);
        }


    }
}