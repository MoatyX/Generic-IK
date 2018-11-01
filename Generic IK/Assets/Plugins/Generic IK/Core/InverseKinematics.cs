using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// The out-of-the-box IK solution for all rigs
    /// </summary>
    public class InverseKinematics : MonoBehaviour
    {
        public Core.Solvers solver;

        public Core.Chain rArm, lArm;
        public Core.Chain rLeg, lLeg;

        public Core.Chain[] otherChains;
        public Core.KinematicChain[] otherKChains;

        //refs
        public RigReader rigReader;
        public Animator animator;

        private void OnEnable()
        {
            if (rigReader == null)
            {
                DetectRig();
            }
        }

        private void LateUpdate()
        {
            switch (solver)
            {
                case Core.Solvers.CyclicDescend:
                    CyclicDescendSolver.Process(rLeg);
                    CyclicDescendSolver.Process(lLeg);
                    CyclicDescendSolver.Process(rArm);
                    CyclicDescendSolver.Process(lArm);
                    for (int i = 0; i < otherChains.Length; i++) CyclicDescendSolver.Process(otherChains[i]);
                    break;
                case Core.Solvers.FastReach:
                    FastReachSolver.Process(rLeg);
                    FastReachSolver.Process(lLeg);
                    FastReachSolver.Process(rArm);
                    FastReachSolver.Process(lArm);
                    for (int i = 0; i < otherChains.Length; i++) FastReachSolver.Process(otherChains[i]);
                    break;
            }

            for (int i = 0; i < otherKChains.Length; i++)
            {
                ChainKinematicSolver.Process(otherKChains[i]);
            }
        }

        /// <summary>
        /// Detect and initiate the rig
        /// </summary>
        public void DetectRig()
        {
            if (!animator) animator = GetComponent<Animator>();
            rigReader = new RigReader(animator);
        }

        public void BuildRig()
        {
            rArm = rigReader.BuildChain(HumanPart.RightArm);
            lArm = rigReader.BuildChain(HumanPart.LeftArm);

            rLeg = rigReader.BuildChain(HumanPart.RightLeg);
            lLeg = rigReader.BuildChain(HumanPart.LeftLeg);
        }
    }
}
