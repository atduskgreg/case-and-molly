using UnityEngine;

class RobotScript : MonoBehaviour
{
    public float radius = 4;
    public float velocity = 1;

    private float angle;
    private Vector3 center;
    private Vector3 lastPos;

    void Start()
    {
        center = gameObject.transform.position;
    }

    void Update()
    {
        animation.Play();
    }

    void FixedUpdate()
    {
        var pos = gameObject.transform.position;

        pos.x = center.x + Mathf.Sin(angle) * radius;
        pos.z = center.z + Mathf.Cos(angle) * radius;

        gameObject.transform.position = pos;

        gameObject.transform.forward = pos - lastPos;
        lastPos = pos;

        angle += Time.deltaTime * velocity;
    }
}
