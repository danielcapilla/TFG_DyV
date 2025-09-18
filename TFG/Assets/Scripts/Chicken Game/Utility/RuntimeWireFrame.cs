using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class RuntimeWireframe : MonoBehaviour
{
    [Header("Opciones Wireframe")]
    public Color lineColor = Color.black;

    private Mesh mesh;
    private Material lineMaterial;
    // Busqueda O(1) para aristas unicas
    private HashSet<(int, int)> edges = new HashSet<(int, int)>();

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null) return;

        // Material para lineas
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterial.SetInt("_ZWrite", 0);

        BuildEdges();
    }

    void BuildEdges()
    {
        edges.Clear();
        int[] triangles = mesh.triangles;
        // Recorrer todos los triangulos (3 indices)
        foreach (int i in System.Linq.Enumerable.Range(0, triangles.Length / 3))
        {
            // Sus 3 indices
            int i0 = triangles[i * 3];
            int i1 = triangles[i * 3 + 1];
            int i2 = triangles[i * 3 + 2];
            // Crear aristas (ordenadas para evitar duplicados)
            var triEdges = new (int, int)[]
            {
                i0 < i1 ? (i0,i1) : (i1,i0),
                i1 < i2 ? (i1,i2) : (i2,i1),
                i2 < i0 ? (i2,i0) : (i0,i2)
            };
            // Guardar solo aristas externas
            foreach (var e in triEdges)
            {
                if (!edges.Add(e)) edges.Remove(e); // Si ya estaba, eliminar
            }
        }
    }

    void OnRenderObject()
    {
        if (mesh == null) return;

        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(lineColor);

        Vector3[] vertices = mesh.vertices;
        foreach (var e in edges)
        {
            GL.Vertex(vertices[e.Item1]);
            GL.Vertex(vertices[e.Item2]);
        }

        GL.End();
        GL.PopMatrix();
    }
}
