// Version 1.2
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

public static class ExploderUtils
{
    /// <summary>
    /// just assert ...
    /// </summary>
    [Conditional("UNITY_EDITOR_DEBUG")]
    public static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            UnityEngine.Debug.LogError("Assert! " + message);
            UnityEngine.Debug.Break();
        }
    }

    /// <summary>
    /// just warning ...
    /// </summary>
    [Conditional("UNITY_EDITOR_DEBUG")]
    public static void Warning(bool condition, string message)
    {
        if (!condition)
        {
            UnityEngine.Debug.LogWarning("Warning! " + message);
        }
    }

    /// <summary>
    /// unity log
    /// </summary>
    [Conditional("UNITY_EDITOR_DEBUG")]
    public static void Log(string message)
    {
        UnityEngine.Debug.Log(message);
    }

    /// <summary>
    /// get centroid of the object (based on render bounds)
    /// </summary>
    public static Vector3 GetCentroid(GameObject obj)
    {
        var meshRenderer = obj.GetComponentsInChildren<MeshRenderer>();

        var centroid = Vector3.zero;

        if (meshRenderer == null || meshRenderer.Length == 0)
        {
            var skinnedMeshRenderer = obj.GetComponentInChildren<SkinnedMeshRenderer>();

            if (skinnedMeshRenderer)
            {
                return skinnedMeshRenderer.bounds.center;
            }

            return obj.transform.position;
        }

        foreach (var meshRend in meshRenderer)
        {
            centroid += meshRend.bounds.center;
        }

        return centroid / meshRenderer.Length;
    }

    /// <summary>
    /// set this object visible to render
    /// </summary>
    public static void SetVisible(GameObject obj, bool status)
    {
        if (obj)
        {
            var renderers = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in renderers)
            {
                meshRenderer.enabled = status;
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// clear console
    /// </summary>
    public static void ClearLog()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.SceneView));

        Type type = assembly.GetType("UnityEditorInternal.LogEntries");
        MethodInfo method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
#else
        public static void ClearLog() {}
#endif

    /// <summary>
    /// unity version specific isActive (to suppress warnings)
    /// </summary>
    public static bool IsActive(GameObject obj)
    {
#if !(UNITY_2_6	|| UNITY_2_6_1 || UNITY_3_0	|| UNITY_3_0_0 || UNITY_3_1	|| UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
            return obj && obj.activeSelf;
#else
        return obj && obj.active;
#endif
    }

    /// <summary>
    /// unity version specific SetActive (to suppress warnings)
    /// </summary>
    public static void SetActive(GameObject obj, bool status)
    {
#if !(UNITY_2_6	|| UNITY_2_6_1 || UNITY_3_0	|| UNITY_3_0_0 || UNITY_3_1	|| UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
        if (obj)
        {
            obj.SetActive(status);
        }
#else
        if (obj)
        {
            obj.active = status;
        }
#endif
    }

    /// <summary>
    /// unity version specific SetActiveRecursively (to suppress warnings)
    /// </summary>
    public static void SetActiveRecursively(GameObject obj, bool status)
    {
#if !(UNITY_2_6	|| UNITY_2_6_1 || UNITY_3_0	|| UNITY_3_0_0 || UNITY_3_1	|| UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
        if (obj)
        {
            var childCount = obj.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                SetActiveRecursively(obj.transform.GetChild(i).gameObject, status);
            }
            obj.SetActive(status);
        }
#else
        if (obj)
        {
            obj.SetActiveRecursively(status);
            obj.active = status;
        }
#endif
    }

    /// <summary>
    /// enable colliders in object hiearchy
    /// </summary>
    public static void EnableCollider(GameObject obj, bool status)
    {
        if (obj)
        {
            var colliders = obj.GetComponentsInChildren<Collider>();

            foreach (var collider in colliders)
            {
                collider.enabled = status;
            }
        }
    }
}
