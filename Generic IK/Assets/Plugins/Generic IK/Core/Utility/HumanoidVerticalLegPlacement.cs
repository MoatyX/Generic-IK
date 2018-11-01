using System;
using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// Dynamically reposition the legs on slopy surfaces
    /// </summary>
    public class HumanoidVerticalLegPlacement : MonoBehaviour
    {
        [Header("Interface")] public Animator SourceAnimator;

        [Tooltip("This uses the LegType parameter inside each Leg object and auto build it")]
        public bool AutoBuildChain = true;

        public LayerMask LayerMask = 0;

        public HumanLeg Right, Left;
        private Transform _root;

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
            _root = rigReader.Root.joint;

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
            Right.TerrainAdjustment(LayerMask);
            Left.TerrainAdjustment(LayerMask);
        }

        private void ProcessHips()
        {
        }

        private void Solve()
        {
        }
    }
}