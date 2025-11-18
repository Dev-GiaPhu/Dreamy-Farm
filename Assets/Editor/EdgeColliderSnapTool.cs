using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(EdgeCollider2D))]
public class EdgeColliderSnapTool : Editor
{
    private EdgeCollider2D edge;

    // --- Cài đặt tuỳ chỉnh ---
    private static float snapStep = 0.5f;
    private static bool requireCtrl = true;
    private static bool requireShift = false;
    private static bool requireAlt = false;
    private static int mouseButton = 0; // 0=Trái, 1=Phải, 2=Giữa

    private bool showSettings = true;

    private void OnEnable()
    {
        edge = (EdgeCollider2D)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚙️ Snap & Input Settings", EditorStyles.boldLabel);

        showSettings = EditorGUILayout.Foldout(showSettings, "Custom Settings");

        if (showSettings)
        {
            EditorGUI.BeginChangeCheck();
            snapStep = EditorGUILayout.FloatField("Snap Step", snapStep);

            EditorGUILayout.LabelField("Add Point Shortcut");
            requireCtrl = EditorGUILayout.Toggle("Require Ctrl (Cmd)", requireCtrl);
            requireShift = EditorGUILayout.Toggle("Require Shift", requireShift);
            requireAlt = EditorGUILayout.Toggle("Require Alt", requireAlt);
            mouseButton = EditorGUILayout.Popup("Mouse Button", mouseButton, new[] { "Left", "Right", "Middle" });

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetFloat("EdgeSnapStep", snapStep);
                EditorPrefs.SetBool("EdgeRequireCtrl", requireCtrl);
                EditorPrefs.SetBool("EdgeRequireShift", requireShift);
                EditorPrefs.SetBool("EdgeRequireAlt", requireAlt);
                EditorPrefs.SetInt("EdgeMouseButton", mouseButton);
            }
        }

        EditorGUILayout.HelpBox("Giữ phím đã chọn + click chuột để thêm điểm snap vào grid.", MessageType.Info);
    }

    private void OnSceneGUI()
    {
        if (edge == null) return;

        Vector2[] pts = edge.points;
        Handles.color = Color.green;

        // Vẽ đường line
        for (int i = 0; i < pts.Length - 1; i++)
        {
            Handles.DrawLine(edge.transform.TransformPoint(pts[i]),
                             edge.transform.TransformPoint(pts[i + 1]));
        }

        // Vẽ các điểm có thể kéo
        for (int i = 0; i < pts.Length; i++)
        {
            Vector3 world = edge.transform.TransformPoint(pts[i]);
            EditorGUI.BeginChangeCheck();
            var fmh_76_24_638979041387346150 = Quaternion.identity; Vector3 newWorld = Handles.FreeMoveHandle(
                world, 0.05f, Vector3.zero, Handles.DotHandleCap
            );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(edge, "Move Point");
                Vector2 local = edge.transform.InverseTransformPoint(newWorld);
                pts[i] = Snap(local);
                edge.points = pts;
                EditorUtility.SetDirty(edge);
            }
        }

        // Thêm điểm mới theo phím tắt
        HandleAddPoint(Event.current, edge);
    }

    private static Vector2 Snap(Vector2 pos)
    {
        return new Vector2(
            Mathf.Round(pos.x / snapStep) * snapStep,
            Mathf.Round(pos.y / snapStep) * snapStep
        );
    }

    private void HandleAddPoint(Event e, EdgeCollider2D edge)
    {
        if (e.type != EventType.MouseDown) return;
        if (e.button != mouseButton) return;
        if (requireCtrl && !e.control && !e.command) return;
        if (requireShift && !e.shift) return;
        if (requireAlt && !e.alt) return;

        // Lấy vị trí chuột
        Vector3 mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
        Vector2 local = edge.transform.InverseTransformPoint(mousePos);
        local = Snap(local);

        Undo.RecordObject(edge, "Add Edge Point");
        var list = new List<Vector2>(edge.points) { local };
        edge.points = list.ToArray();
        EditorUtility.SetDirty(edge);
        e.Use();
    }
}
