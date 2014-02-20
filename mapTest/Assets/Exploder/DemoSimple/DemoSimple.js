#pragma strict

var Exploder : GameObject = null;
private var DestroyableObjects : GameObject[] = null;

function Start()
{
    // find all objects in the scene with tag "Exploder"
    DestroyableObjects = GameObject.FindGameObjectsWithTag("Exploder");
}

function ChangeExploderParams()
{
    // create hashtable
    var hashtable = new Hashtable();

    // these are all Exploder parameters, you can change any of them, all of them or none of them at once
    // use only what you want to change
    // explanation of these parameters you can find in documentation or in ExploderObject class

    hashtable["DontUseTag"] = true;
    hashtable["Radius"] = 15.0f;
    hashtable["ForceVector"] = Vector3.one;
    hashtable["Force"] = 10.0f;
    hashtable["FrameBudget"] = 10;
    hashtable["TargetFragments"] = 200;
    hashtable["DeactivateOptions"] = 1;
    hashtable["DeactivateTimeout"] = 4.0f;
    hashtable["MeshColliders"] = false;
    hashtable["ExplodeSelf"] = false;
    hashtable["HideSelf"] = false;
    hashtable["DestroyOriginalObject"] = true;
    hashtable["ExplodeFragments"] = false;

    // call this to change exploder parameters
    Exploder.SendMessage("SetFromJavaScript", hashtable);
}

function OnGUI()
{
    if (GUI.Button(Rect(10, 10, 100, 30), "Explode!"))
    {
        if (Exploder)
        {
            Exploder.SendMessage("Explode");
        }
    }

    if (GUI.Button(Rect(130, 10, 100, 30), "Reset"))
    {
        // activate exploder
        Activate(Exploder, true);

        // activate destroyable objects
        for (var destroyableObject : GameObject in DestroyableObjects)
        {
            if (destroyableObject)
            {
                Activate(destroyableObject.gameObject, true);
            }
        }
    }
}

// this function just activate game object
function Activate(obj : GameObject, status : boolean)
{
    if (obj)
    {
#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3

        var childCount = obj.transform.childCount;
        for (var i = 0; i < childCount; i++)
        {
            Activate(obj.transform.GetChild(i).gameObject, status);
        }
        obj.SetActive(status);
#else

        obj.SetActiveRecursively(status);
        obj.active = status;

#endif
    }
}
