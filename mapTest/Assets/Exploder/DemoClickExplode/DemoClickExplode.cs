// to crack and explode use this macro
// crack by left mouse button, explode after by right mouse button
//#define ENABLE_CRACK_AND_EXPLODE
//#define TEST_SCENE_LOAD

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DemoClickExplode : MonoBehaviour
{
    public ExploderObject Exploder;
    private GameObject[] DestroyableObjects;
    public Camera Camera;

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

    bool IsExplodable(GameObject obj)
    {
        if (Exploder.DontUseTag)
        {
            return obj.GetComponent<Explodable>() != null;
        }
        else
        {
            return obj.CompareTag(ExploderObject.Tag);
        }
    }

    void Update()
    {
        // we hit the mouse button
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Ray mouseRay;

            if (Camera)
            {
                mouseRay = Camera.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                mouseRay = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
            }

            RaycastHit hitInfo;

            // we hit the object
            if (Physics.Raycast(mouseRay, out hitInfo))
            {
                var obj = hitInfo.collider.gameObject;

                // explode this object!
                if (IsExplodable(obj))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ExplodeObject(obj);
                    }
                    else
                    {
                        ExplodeAfterCrack();
                    }
                }
            }
        }
    }

    void ExplodeObject(GameObject obj)
    {
        // activate exploder
        ExploderUtils.SetActive(Exploder.gameObject, true);

        // move exploder object to the same position
        Exploder.transform.position = ExploderUtils.GetCentroid(obj);

        // decrease the radius so the exploder is not interfering other objects
        Exploder.Radius = 1.0f;

        // DONE!
#if ENABLE_CRACK_AND_EXPLODE
        Exploder.Crack(OnCracked);
#else
        Exploder.Explode(OnExplosion);

#endif
    }

    void OnExplosion(float time, ExploderObject.ExplosionState state)
    {
        if (state == ExploderObject.ExplosionState.ExplosionFinished)
        {
            //Utils.Log("Exploded");
        }
    }

    void OnCracked()
    {
        //Utils.Log("Cracked");
    }

    void ExplodeAfterCrack()
    {
#if ENABLE_CRACK_AND_EXPLODE
        Exploder.ExplodeCracked(OnExplosion);
#endif
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Reset"))
        {
            if (!Exploder.DestroyOriginalObject)
            {
                foreach (var destroyableObject in DestroyableObjects)
                {
                    ExploderUtils.SetActiveRecursively(destroyableObject, true);
                }
                ExploderUtils.SetActive(Exploder.gameObject, true);
            }
        }

#if TEST_SCENE_LOAD
        if (GUI.Button(new Rect(10, 50, 100, 30), "NextScene"))
        {
            UnityEngine.Application.LoadLevel(1);
        }
#endif
    }
}
