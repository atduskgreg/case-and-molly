using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class DemoSimple : MonoBehaviour
{
    public ExploderObject Exploder;
    private GameObject[] DestroyableObjects;

    void Start()
    {
        if (Exploder.DontUseTag)
        {
            var objs = FindObjectsOfType(typeof(Explodable));
            var objList = new List<GameObject>(objs.Length);
            objList.AddRange(from Explodable ex in objs where ex select ex.gameObject);
            DestroyableObjects = objList.ToArray();
        }
        else
        {
            // find all objects in the scene with tag "Exploder"
            DestroyableObjects = GameObject.FindGameObjectsWithTag("Exploder");
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Explode!"))
        {
            if (Exploder)
            {
                Exploder.Explode();
            }
        }

        if (GUI.Button(new Rect(130, 10, 100, 30), "Reset"))
        {
            // activate exploder
            ExploderUtils.SetActive(Exploder.gameObject, true);

            if (!Exploder.DestroyOriginalObject)
            {
                foreach (var destroyableObject in DestroyableObjects)
                {
                    ExploderUtils.SetActiveRecursively(destroyableObject, true);
                }
                ExploderUtils.SetActive(Exploder.gameObject, true);
            }
        }
    }
}
