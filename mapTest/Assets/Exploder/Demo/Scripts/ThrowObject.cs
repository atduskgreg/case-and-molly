using UnityEngine;

public class ThrowObject : MonoBehaviour
{
    private float destroyTimer;

    void Start()
    {
        destroyTimer = 10.0f;
    }

    void Update()
    {
        destroyTimer -= Time.deltaTime;

        if (destroyTimer < 0.0f)
        {
            Destroy(gameObject);
        }
    }
}
