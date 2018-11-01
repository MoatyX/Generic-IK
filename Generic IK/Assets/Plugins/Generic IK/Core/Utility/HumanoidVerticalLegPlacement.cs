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
        private bool solve;

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
            if (Input.GetMouseButtonDown(1))
            {
                solve = !solve;
                Debug.Log(solve);
            }

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
            Transform selfRoot = Right.LegChain.Joints[0].joint;
            Transform otherRoot = Left.LegChain.Joints[0].joint;

            Vector3 x = selfRoot.position - Right.LegChain.GetIKTarget();
            Vector3 y = otherRoot.position - Left.LegChain.GetIKTarget();

            float dotX = Mathf.Abs(Vector3.Dot(selfRoot.rotation * Vector3.down, x));
            float dotY = Mathf.Abs(Vector3.Dot(otherRoot.rotation * Vector3.down, y));

            float min = Mathf.Min(dotX, dotY);
            float max = Mathf.Max(dotX, dotY);

            float delta = max - min;
            Vector3 hip = _root.position;

            hip.y -= delta;
            //rootPos = Vector3.Lerp(rootPos, hip, Time.deltaTime * RootAdjSpeed);
            rootPos.y = Mathf.Lerp(rootPos.y, hip.y, Time.deltaTime * RootAdjSpeed);

            if(solve)
            _root.position = new Vector3(_root.position.x, rootPos.y, _root.position.z);
        }

        private void Solve()
        {
            if(!solve) return;

            AnalyticalSolver.Process(Right);
            AnalyticalSolver.Process(Left);
            //Right.LegChain.GetEndEffector().rotation = Quaternion.Inverse(Quaternion.FromToRotation(test.up, _root.up)) * Right.LegChain.GetEndEffector().rotation;
            //Left.LegChain.GetEndEffector().rotation = Quaternion.Inverse(Quaternion.FromToRotation(test.up, _root.up)) * Left.LegChain.GetEndEffector().rotation;
        }
    }
}