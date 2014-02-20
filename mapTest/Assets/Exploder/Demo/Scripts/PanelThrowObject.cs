using UnityEngine;

public class PanelThrowObject : UseObject
{
    public GameObject ThrowBox = null;
    public GameObject[] ThrowObjects = null;

    void Start()
    {
    }

    public override void Use()
    {
        base.Use();

        var rndObject = ThrowObjects[Random.Range(0, ThrowObjects.Length)];

        var instance = Object.Instantiate(rndObject, ThrowBox.transform.position + Vector3.up*2, Quaternion.identity) as GameObject;
        instance.AddComponent<ThrowObject>();

        instance.tag = "Exploder";

        if (instance.rigidbody == null)
        {
            instance.AddComponent<Rigidbody>();
        }

        if (instance.GetComponentsInChildren<BoxCollider>().Length == 0)
        {
            instance.AddComponent<BoxCollider>();
        }

        // throw object in up direction with small deviation
        var v0 = Vector3.up;
        v0.x = Random.Range(-0.2f, 0.2f);
        v0.y = Random.Range(1.0f, 0.8f);
        v0.z = Random.Range(-0.2f, 0.2f);
        v0.Normalize();

        instance.rigidbody.velocity = v0 * 20;
        instance.rigidbody.angularVelocity = Random.insideUnitSphere * 3;

        instance.rigidbody.mass = 20;
    }
}
