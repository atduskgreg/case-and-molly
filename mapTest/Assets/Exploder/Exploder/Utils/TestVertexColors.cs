using UnityEngine;

public class TestVertexColors : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = Color.blue;
        }
        mesh.colors32 = colors;
    }
}
