using UnityEngine;
using Generics.Dynamics;

public class SwingAtDemo : MonoBehaviour
{
    public Transform virtualEndEffector;    //use this transform as a virtual end effector
    public Vector3 offsetInAnim;            //arbitrary offset that we obtained through experminting with numbers
    public Vector3 lookAtAxis = Vector3.forward;
    public Core.Chain chain;

    private void LateUpdate()
    {
        //the idea is to create a rotational difference between the actual goal in anim with an arbitrary offset that makes it seem like the player is targeting something

        //create a rotatioal difference between the animation playing and goal in 3D
        Vector3 direction = offsetInAnim - virtualEndEffector.position;
        Quaternion difference = GenericMath.RotateFromTo(transform.position + direction, lookAtAxis);

        virtualEndEffector.rotation = difference;

        DirectionalSwingSolver.Process(chain, lookAtAxis, virtualEndEffector);
    }
}
