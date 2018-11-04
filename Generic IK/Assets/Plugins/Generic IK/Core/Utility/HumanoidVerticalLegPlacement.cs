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
        public float TimeTarget = 0.2f;
        public Vector3 test;

        [Tooltip("This uses the LegType parameter inside each Leg object and auto build it")]
        public bool AutoBuildChain = true;

        public float RootAdjSpeed = 7;
        public LayerMask LayerMask = 0;

        public HumanLeg Right;
        public HumanLeg Left;
        private Transform _root;
        private Vector3 rootPos;
        private bool solve = true;

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
            if (Input.GetMouseButtonDown(2))
            {
                solve = !solve;
                Debug.Log(solve);
            }

            Time.timeScale = Input.GetMouseButton(1) ? TimeTarget : 1f;

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

            rootPos.x = _root.position.x;
            rootPos.y = Mathf.Lerp(rootPos.y, _root.position.y - delta, Time.deltaTime * RootAdjSpeed);
            rootPos.z = _root.position.z;

            if (solve)
            {
                _root.position = rootPos;
            }
        }

        private void Solve()
        {
            if(!solve) return;

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
            //ppos1.x = transform.position.x;
            //ppos1.z = transform.position.z;
            Vector3 ppos2 = Left.LegChain.GetIKTarget();
            //Gizmos.DrawWireSphere(ppos1, 0.06f);
            //Gizmos.DrawWireSphere(ppos2, 0.06f);

            Gizmos.color = Color.red;
            var pos1 = Right.LegChain.GetEndEffector().position;
            //pos1.x = transform.position.x;
            //pos1.z = transform.position.z;
            //Gizmos.DrawSphere(pos1, 0.1f);

            Gizmos.color = Color.blue;
            //Gizmos.DrawLine(ppos1, pos1);

        }


    }
}