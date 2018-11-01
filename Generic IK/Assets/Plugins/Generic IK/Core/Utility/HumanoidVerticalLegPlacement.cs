using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// Dynamically reposition the legs on slopy surfaces
    /// </summary>
    public class HumanoidVerticalLegPlacement : MonoBehaviour
    {
        [Header("Interface")]
        public Animator SourceAnimator;

        [Tooltip("This uses the LegType parameter inside each Leg object and auto build it")]
        public bool AutoBuildChain = true;

        public float RootAdjSpeed = 7;
        public LayerMask LayerMask = 0;

        public HumanLeg Right, Left;
        private Transform _root;
        private Vector3 rootPos;
        public Transform test;

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
            rootPos = _root.position;

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
            Right.TerrainAdjustment(LayerMask, _root);
            Left.TerrainAdjustment(LayerMask, _root);
        }

        private void ProcessHips()
        {
            float x = Right - Left;
            float y = Left - Right;

            float delta = x;
            Vector3 hip = _root.position;
            hip.y -= delta;
            rootPos = Vector3.Lerp(rootPos, hip, Time.deltaTime * RootAdjSpeed);
            _root.position = rootPos;
        }

        private void Solve()
        {
            AnalyticalSolver.Process(Right);
            AnalyticalSolver.Process(Left);
            //Right.LegChain.GetEndEffector().rotation = Quaternion.Inverse(Quaternion.FromToRotation(test.up, _root.up)) * Right.LegChain.GetEndEffector().rotation;
            Left.LegChain.GetEndEffector().rotation = Quaternion.Inverse(Quaternion.FromToRotation(test.up, _root.up)) * Left.LegChain.GetEndEffector().rotation;
        
        }
    }
}