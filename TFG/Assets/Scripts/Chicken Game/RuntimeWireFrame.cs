using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class RuntimeWireframe : MonoBehaviour
{
    public Color lineColor = Color.black;

    private Mesh mesh;
    private Material lineMaterial;

    struct Edge
    {
        public int v1, v2;
        public Edge(int a, int b)
        {
            if (a < b) { v1 = a; v2 = b; } else { v1 = b; v2 = a; } // ordenar para evitar duplicados
        }
        public override int GetHashCode() => v1 * 73856093 ^ v2 * 19349663;
        public override bool Equals(object obj)
        {
            if (!(obj is Edge)) return false;
            Edge e = (Edge)obj;
            return v1 == e.v1 && v2 == e.v2;
        }
    }

    private List<Edge> edges = new List<Edge>();

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;

        // Material para las lineas
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
        Dictionary<Edge, int> edgeCount = new Dictionary<Edge, int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            Edge[] triEdges = {
                new Edge(i0, i1),
                new Edge(i1, i2),
                new Edge(i2, i0)
            };

            foreach (var e in triEdges)
            {
                if (edgeCount.ContainsKey(e)) edgeCount[e]++;
                else edgeCount[e] = 1;
            }
        }

        // Guardar solo los bordes externos 
        foreach (var kvp in edgeCount)
        {
            if (kvp.Value == 1) edges.Add(kvp.Key);
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
            GL.Vertex(vertices[e.v1]);
            GL.Vertex(vertices[e.v2]);
        }

        GL.End();
        GL.PopMatrix();
    }
}
