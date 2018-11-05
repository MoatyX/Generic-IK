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
        public float RootAdjSpeed = 7;
        public Transform Root;
        public LayerMask LayerMask = 0;

        [Header("Legs")]
        public HumanLeg Right;
        public HumanLeg Left;

        private float rootOff;

        private void Start()
        {
            if (SourceAnimator == null) SourceAnimator = GetComponent<Animator>();
            if (SourceAnimator == null)
            {
                Debug.LogError("No Source Animator was found. it is an important step to automatic initialization");
                return;
            }

            AutomaticInit();
        }

        private void LateUpdate()
        {
            ProcessLegs();
            ProcessHips();
            Solve();
        }

        /// <summary>
        /// Automatically Initialise the references
        /// </summary>
        private void AutomaticInit()
        {
            if (SourceAnimator.isHuman == false)
            {
                Debug.LogError(this + " works only for humanoid characters");
                enabled = false;
                return;
            }

            RigReader rigReader = new RigReader(SourceAnimator);

            if (Root == null) Root = rigReader.Root.joint;

            //auto build the chains
            Right.AutoBuild(SourceAnimator);
            Left.AutoBuild(SourceAnimator);
        }

        /// <summary>
        /// Process Legs
        /// </summary>
        private void ProcessLegs()
        {
            Right.TerrainAdjustment(LayerMask, transform);
            Left.TerrainAdjustment(LayerMask, transform);
        }

        /// <summary>
        /// Adjust Hips
        /// </summary>
        private void ProcessHips()
        {
            float yRight = Right.Chain.GetIKTarget().y;
            float yRight2 = Right.Chain.GetEndEffector().position.y;
            float yLeft = Left.Chain.GetIKTarget().y;
            float yLeft2 = Left.Chain.GetEndEffector().position.y;

            float min = Mathf.Min(yRight, yLeft);
            float min2 = Mathf.Min(yRight2, yLeft2);

            float target = min2 - min;

            rootOff = Mathf.Lerp(rootOff, target, Time.deltaTime * RootAdjSpeed);

            Vector3 downDir = -Root.up;     //can be adjusted to specific spaces (world/local space, down dir) (curSpace = Local)
            Root.position += downDir * rootOff;
        }

        private void Solve()
        {
            AnalyticalSolver.Process(Right.Chain);
            AnalyticalSolver.Process(Left.Chain);

            Right.RotateAnkle();
            Left.RotateAnkle();
        }

#if UNITY_EDITOR

        /// <summary>
        /// Debug and testing purposes
        /// </summary>
        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false) return;

           // DrawDebugGizmos(); Enable for debug purposes
        }

        private void DrawDebugGizmos()
        {
            Time.timeScale = Input.GetMouseButton(0) ? 0.2f : 1f;

            Gizmos.color = Color.green;
            Vector3 pp1 = Right.Chain.GetIKTarget();
            Vector3 pp2 = Left.Chain.GetIKTarget();
            Gizmos.DrawWireSphere(pp1, 0.06f);
            Gizmos.DrawWireSphere(pp2, 0.06f);

            Gizmos.color = Color.red;
            Vector3 p1 = Right.Chain.GetEndEffector().position;
            Vector3 p2 = Left.Chain.GetEndEffector().position;
            Gizmos.DrawSphere(p1, 0.06f);
            Gizmos.DrawSphere(p2, 0.06f);
        }

#endif
    }
}