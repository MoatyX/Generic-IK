using System;
using UnityEngine;

namespace Generics.Dynamics
{
    /// <summary>
    /// Solver to solve dynamic chains
    /// </summary>
    public static class ChainKinematicSolver
    {
        private static readonly Func<float, ushort, float> Pow = GenericMath.SimplerPower;

        /// <summary>
        /// Apply Physics
        /// </summary>
        /// <param name="chain"></param>
        public static void Process(Core.KinematicChain chain)
        {
            if (!chain.initiated) chain.InitiateJoints();

            chain.MapVirtualJoints();
            FollowParent(chain);
            MapSolverOutput(chain);

        }

        /// <summary>
        /// Calculate joints position
        /// </summary>
        /// <param name="chain"></param>
        private static void FollowParent(Core.KinematicChain chain)
        {
            //calculate the new pos based on velocity and gravity
            for (int i = 1; i < chain.joints.Count; i++)
            {
                Vector3 drag = (chain.joints[i].pos - chain.prevPos[i]);

                //Derived on paper using the Bending differential equations
                float bendingFalloff = 6f * Pow(chain.joints.Count * i, 2) - 4f * chain.joints.Count * Pow(i, 3) + Pow(i, 4);
                Vector3 g = chain.gravity * bendingFalloff / (chain.momentOfInteria * chain.joints.Count * 12f);    //gravity must be fixed and not controlled by the solver fall off

                //calculate the solver fall off and its effects
                float t = i / (chain.joints.Count - 1f);
                float eval = chain.solverFallOff.Evaluate(t) * chain.weight;
                float velocityMag = chain.torsionDamping != 0f ? Mathf.Sin(eval * (Mathf.PI / 2f)) * Mathf.Exp(-chain.torsionDamping * eval) : 1f;  //maybe the user doesnt want damping at all

                chain.prevPos[i] = chain.joints[i].pos;              //reset
                chain.joints[i].pos += drag * velocityMag;     //apply drag
                chain.joints[i].pos += g;                      //apply gravity


                //Keep the root up to date
                chain.prevPos[0] = chain.joints[0].pos;
                chain.joints[0].pos = chain.joints[0].joint.position;


                float parentLength = chain.joints[i - 1].length;
                Vector3 selfNextPos = chain.joints[i].joint.position + (chain.joints[i - 1].pos - chain.joints[i - 1].joint.position);  //child's next pos relative to the parents momentum
                Vector3 selfDrag = selfNextPos - chain.joints[i].pos;       //how much drag to this bone was caused by the parent's influence

                chain.joints[i].pos += selfDrag * (1 - chain.weight);       //apply weight

                //calculate stiffness
                Vector3 clampedDrag = selfDrag * (2 - chain.weight);
                float currDrag = clampedDrag.magnitude;
                float maxDrag = parentLength * -chain.stiffness * eval;
                Vector3 stiffnessOffset = clampedDrag * (Mathf.Max((currDrag - maxDrag), 0f) / Mathf.Max(1f, currDrag));

                chain.joints[i].pos += stiffnessOffset;                     //apply stiffness

                //finalise the chain
                Vector3 chainDir = chain.joints[i - 1].pos - chain.joints[i].pos;
                float length = chainDir.magnitude;

                chain.joints[i].pos += chainDir * ((length - parentLength) / Mathf.Max(length, 1f));
                chain.joints[i].pos = Vector3.Lerp(chain.joints[i].joint.position, chain.joints[i].pos, chain.joints[i].weight);
            }
        }

        /// <summary>
        /// Map the solvers result on the object
        /// </summary>
        /// <param name="chain"></param>
        private static void MapSolverOutput(Core.KinematicChain chain)
        {
            for (int i = 1; i < chain.joints.Count; i++)
            {
                Vector3 chainPoleAxis = GenericMath.TransformVector(chain.joints[i - 1].localAxis, chain.joints[i - 1].rot);
                Vector3 chainDirection = chain.joints[i].pos - chain.joints[i - 1].pos;
                Quaternion offset = GenericMath.RotateFromTo(chainDirection, chainPoleAxis);

                chain.joints[i - 1].rot = GenericMath.ApplyQuaternion(offset, chain.joints[i - 1].rot);

                chain.joints[i - 1].ApplyVirtualMap(false, true);
                chain.joints[i].ApplyVirtualMap(true, false);

                chain.joints[i - 1].ApplyRestrictions();
                chain.joints[i].ApplyRestrictions();
            }
        }
    }
}
