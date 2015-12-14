using UnityEngine;
using UnityEditor;


//*
[CustomEditor(typeof(PolygonCollider2D))]
public class PolygonCollider2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var Collider2D = (PolygonCollider2D)target;
        var points = Collider2D.points;
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = EditorGUILayout.Vector2Field(i.ToString(), points[i]);
        }
        Collider2D.points = points;
        EditorUtility.SetDirty(target);
    }
}
/* */