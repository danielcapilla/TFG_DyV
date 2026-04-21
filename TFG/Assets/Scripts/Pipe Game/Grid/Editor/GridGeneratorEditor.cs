using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);

        if (GUILayout.Button("Generate Grid", GUILayout.Height(32)))
        {
            GridGenerator gen = (GridGenerator)target;
            Undo.RegisterFullObjectHierarchyUndo(gen.gameObject, "Generate Grid");
            gen.GenerateGrid();
            EditorUtility.SetDirty(gen.gameObject);
        }
    }
}
