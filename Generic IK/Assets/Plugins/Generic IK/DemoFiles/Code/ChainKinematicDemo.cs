using UnityEngine;
using Generics.Dynamics;

/// <summary>
/// Simple Chain Kinematics simulation
/// </summary>
public class ChainKinematicDemo : MonoBehaviour
{
    public Core.KinematicChain chain;

    private void LateUpdate()
    {
        ChainKinematicSolver.Process(chain);
    }

}
