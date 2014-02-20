using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    DestroyableObject,
    UseObject,
    Default,
    None,
}

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance { get { return instance; } }
    private static TargetManager instance;

    public GameObject TargetObject;
    public TargetType TargetType;
    public Vector3 TargetPosition;

    public GUITexture CrosshairGun = null;
    public GUITexture CrosshairHand = null;
    public ExploderMouseLook MouseLookCamera = null;
    public GUIText PanelText = null;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ExploderUtils.SetActive(CrosshairGun.gameObject, true);
        ExploderUtils.SetActive(CrosshairHand.gameObject, true);
        ExploderUtils.SetActive(PanelText.gameObject, true);
    }

    void Update()
    {
        // run raycast against objects in the scene
        var mouseRay = MouseLookCamera.mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        UnityEngine.Debug.DrawRay(mouseRay.origin, mouseRay.direction * 10, Color.red, 0);

        CrosshairGun.color = Color.white;
        TargetObject = null;
        TargetType = TargetType.None;
        TargetPosition = Vector3.zero;

        var hits = new List<RaycastHit>(Physics.RaycastAll(mouseRay, Mathf.Infinity/*, 1 << LayerMask.NameToLayer("Exploder")*/));
        GameObject hitObject = null;

        if (hits.Count > 0)
        {
            hits.Sort(delegate(RaycastHit a, RaycastHit b)
            {
                return (MouseLookCamera.transform.position - a.point).sqrMagnitude.CompareTo(
                       (MouseLookCamera.transform.position - b.point).sqrMagnitude);
            });

            hitObject = hits[0].collider.gameObject;
            TargetPosition = hits[0].point;
        }

        if (hitObject != null)
        {
            TargetObject = hitObject;

            if (IsDestroyableObject(TargetObject))
            {
                TargetType = TargetType.DestroyableObject;
            }
            else if (IsUseObject(TargetObject))
            {
                var useObject = TargetObject.GetComponent<UseObject>();

                if (useObject)
                {
                    if ((MouseLookCamera.transform.position - useObject.transform.position).sqrMagnitude <
                        useObject.UseRadius*useObject.UseRadius)
                    {
                        TargetType = TargetType.UseObject;
                    }
                }
            }
            else
            {
                TargetType = TargetType.Default;
            }
        }

        switch (TargetType)
        {
            case TargetType.DestroyableObject:
                CrosshairHand.enabled = false;
                CrosshairGun.enabled = true;
                CrosshairGun.color = Color.red;
                break;

            case TargetType.UseObject:
                CrosshairGun.enabled = false;
                CrosshairHand.enabled = true;
                PanelText.enabled = true;
                PanelText.text = TargetObject.GetComponent<UseObject>().HelperText;
                break;

            case TargetType.Default:
            case TargetType.None:
                CrosshairHand.enabled = false;
                CrosshairGun.enabled = true;
                CrosshairGun.color = Color.white;
                PanelText.enabled = false;
                break;
        }

        // activate use object
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (TargetType == TargetType.UseObject)
            {
                var useObject = TargetObject.GetComponent<UseObject>();

                if (useObject)
                {
                    useObject.Use();
                }
            }
        }
    }

    bool IsDestroyableObject(GameObject obj)
    {
        if (obj.CompareTag("Exploder"))
        {
            return true;
        }

        if (obj.transform.parent)
        {
            return IsDestroyableObject(obj.transform.parent.gameObject);
        }

        return false;
    }

    bool IsUseObject(GameObject obj)
    {
        if (obj.CompareTag("UseObject"))
        {
            return true;
        }

        if (obj.transform.parent)
        {
            return IsDestroyableObject(obj.transform.parent.gameObject);
        }

        return false;
    }
}
