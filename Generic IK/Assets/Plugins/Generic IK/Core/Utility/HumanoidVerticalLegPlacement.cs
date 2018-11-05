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
        public float HealHeight = 0.1f;
        public float RootAdjSpeed = 7;
        public LayerMask LayerMask = 0;

        public Transform Root;
        public HumanLeg Right;
        public HumanLeg Left;

        private Vector3 rootPos;
        private float legLength;

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

            //auto build the chains
            Right.AutoBuild(SourceAnimator);
            Left.AutoBuild(SourceAnimator);

            legLength = Mathf.Abs(Root.position.y - Right.LegChain.GetEndEffector().position.y);
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
            float yRight = Right.LegChain.GetIKTarget().y;
            float yLeft = Left.LegChain.GetIKTarget().y;
            float min = Mathf.Min(yRight, yLeft);

            float delta = min - Root.position.y;
            Debug.Log(delta);
            float target = Mathf.Clamp(delta, legLength, legLength - delta);

            rootPos.x = Root.position.x;
            rootPos.y = Mathf.Lerp(rootPos.y, target, Time.deltaTime * RootAdjSpeed);
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

#if UNITY_EDITOR

        /// <summary>
        /// Debug and testing purposes
        /// </summary>
        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false) return;

            DrawDebugGizmos();
        }

        private void DrawDebugGizmos()
        {
            Time.timeScale = Input.GetMouseButton(0) ? 0.2f : 1f;

            Gizmos.color = Color.green;
            Vector3 pp1 = Right.LegChain.GetIKTarget();
            Vector3 pp2 = Left.LegChain.GetIKTarget();
            Gizmos.DrawWireSphere(pp1, 0.06f);
            Gizmos.DrawWireSphere(pp2, 0.06f);

            Gizmos.color = Color.red;
            Vector3 p1 = Right.LegChain.GetEndEffector().position;
            Vector3 p2 = Left.LegChain.GetEndEffector().position;
            Gizmos.DrawSphere(p1, 0.06f);
            Gizmos.DrawSphere(p2, 0.06f);
        }

#endif
    }
}