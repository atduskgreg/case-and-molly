using System.Collections.Generic;
using UnityEngine;

public class PanelResetScene : UseObject
{
    private List<GameObject> objectList;

    void Start()
    {
        objectList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Exploder"));
    }

    public override void Use()
    {
        base.Use();

        ExploderUtils.ClearLog();

        foreach (var o in objectList)
        {
            ExploderUtils.SetActiveRecursively(o, true);
            ExploderUtils.SetVisible(o, true);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Use();
        }
    }
}
