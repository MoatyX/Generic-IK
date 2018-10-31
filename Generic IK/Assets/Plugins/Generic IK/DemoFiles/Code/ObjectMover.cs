using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    public Vector3 mainDir, secondaryDir;
    public float range = 1;
    public float speed = 1;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        Vector3 a1 = mainDir * Mathf.Sin(Time.time * speed) *  range;
        Vector3 a2 = secondaryDir * Mathf.Cos(Time.time * speed) * range;
        transform.position = a1 + a2 + startPos;
    }
}
