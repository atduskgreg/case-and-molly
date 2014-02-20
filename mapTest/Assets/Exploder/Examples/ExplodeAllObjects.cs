using UnityEngine;

/// <summary>
/// example how to explode every explodable (tagged) objects in the scene at once
/// </summary>
public class ExplodeAllObjects : MonoBehaviour
{
    public ExploderObject ExploderObjectInstance = null;
    private GameObject[] DestroyableObjects;

    private int counter;
    private int counterFinished;

    void Start()
    {
        DestroyableObjects = GameObject.FindGameObjectsWithTag("Exploder");
    }

    void Update()
    {
        // press enter to start explosions
        if (Input.GetKeyDown(KeyCode.Return))
        {
            foreach (var o in DestroyableObjects)
            {
                ExplodeObject(o);
            }
        }
    }

    private void ExplodeObject(GameObject gameObject)
    {
        ExploderUtils.SetActive(ExploderObjectInstance.gameObject, true);
        ExploderObjectInstance.transform.position = ExploderUtils.GetCentroid(gameObject);
        ExploderObjectInstance.Radius = 1.0f;
        ExploderObjectInstance.Explode();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(200, 10, 300, 30), "Hit enter to explode everyting!");
    }
}
