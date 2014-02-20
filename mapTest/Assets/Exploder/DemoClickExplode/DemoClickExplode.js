//!!! IMPORTANT !!!
//
// make sure you copy Exploder source codes to "Assets/Standard Assets" folder
//

/*

#pragma strict

var Exploder : ExploderObject;
var Camera : Camera;
private var DestroyableObjects : GameObject[] = null;

function Start()
{
    // find all objects in the scene with tag "Exploder"
    DestroyableObjects = GameObject.FindGameObjectsWithTag("Exploder");
}

function IsExplodable(obj : GameObject)
{
    return obj.CompareTag(ExploderObject.Tag);
}

function Update()
{
    // we hit the mouse button
    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
    {
        var mouseRay : Ray;

        if (Camera)
        {
            mouseRay = Camera.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            mouseRay = UnityEngine.Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
        }

        var hitInfo : RaycastHit;

        // we hit the object
        if (Physics.Raycast(mouseRay, hitInfo))
        {
            var obj = hitInfo.collider.gameObject;

            // explode this object!
            if (IsExplodable(obj))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    CrackObject(obj);
                }
                else
                {
                    ExplodeAfterCrack();
                }
            }
        }
    }
}

function CrackObject(obj : GameObject)
{
    // activate exploder
    ExploderUtils.SetActive(Exploder.gameObject, true);

    // move exploder object to the same position
    Exploder.transform.position = ExploderUtils.GetCentroid(obj);

    // decrease the radius so the exploder is not interfering other objects
    Exploder.Radius = 1.0f;

    // crack
    Exploder.Crack();
}

function ExplodeAfterCrack()
{
    Exploder.ExplodeCracked();
}

function OnGUI()
{
    if (GUI.Button(new Rect(10, 10, 100, 30), "Reset"))
    {
        if (!Exploder.DestroyOriginalObject)
        {
            for (var destroyableObject : GameObject in DestroyableObjects)
            {
                destroyableObject.active = true;
            }

            Exploder.active = true;
        }
    }
}

*/
