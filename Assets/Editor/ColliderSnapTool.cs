using UnityEngine;
using UnityEditor;

public class ColliderSnapTool : EditorWindow
{
    private float snapValue = 0.5f;
    private KeyCode snapKey = KeyCode.LeftControl;
    private bool useMouseButton = false;
    private int mouseButton = 0; // 0 = Left, 1 = Right, 2 = Middle

    [MenuItem("Tools/Collider Snap Tool")]
    public static void ShowWindow()
    {
        GetWindow<ColliderSnapTool>("Collider Snap Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("⚙️ Collider Snap Settings", EditorStyles.boldLabel);
        snapValue = EditorGUILayout.FloatField("Snap Value", snapValue);
        snapKey = (KeyCode)EditorGUILayout.EnumPopup("Snap Key", snapKey);
        useMouseButton = EditorGUILayout.Toggle("Use Mouse Button", useMouseButton);

        if (useMouseButton)
        {
            mouseButton = EditorGUILayout.IntSlider("Mouse Button (0=LMB,1=RMB,2=MMB)", mouseButton, 0, 2);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "• Snap works on EdgeCollider2D & PolygonCollider2D.\n" +
            "• When snapping key/button is pressed, all points will align to grid.",
            MessageType.Info
        );

        if (GUILayout.Button("Snap Selected Collider(s)"))
        {
            SnapSelectedColliders();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        bool keyPressed = e.type == EventType.KeyDown && e.keyCode == snapKey;
        bool mousePressed = useMouseButton && e.type == EventType.MouseDown && e.button == mouseButton;

        if (keyPressed || mousePressed)
        {
            SnapSelectedColliders();
            e.Use(); // chặn sự kiện gốc
        }
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void SnapSelectedColliders()
    {
        foreach (var obj in Selection.gameObjects)
        {
            bool snapped = false;

            EdgeCollider2D edge = obj.GetComponent<EdgeCollider2D>();
            if (edge != null)
            {
                Undo.RecordObject(edge, "Snap EdgeCollider2D");
                Vector2[] points = edge.points;
                for (int i = 0; i < points.Length; i++)
                    points[i] = SnapToGrid(points[i], snapValue);
                edge.points = points;
                snapped = true;
            }

            PolygonCollider2D poly = obj.GetComponent<PolygonCollider2D>();
            if (poly != null)
            {
                Undo.RecordObject(poly, "Snap PolygonCollider2D");
                for (int p = 0; p < poly.pathCount; p++)
                {
                    Vector2[] path = poly.GetPath(p);
                    for (int i = 0; i < path.Length; i++)
                        path[i] = SnapToGrid(path[i], snapValue);
                    poly.SetPath(p, path);
                }
                snapped = true;
            }

            if (snapped)
                Debug.Log($"✅ Snapped collider on {obj.name} to {snapValue}-grid.");
        }

        SceneView.RepaintAll();
    }

    private Vector2 SnapToGrid(Vector2 pos, float snap)
    {
        pos.x = Mathf.Round(pos.x / snap) * snap;
        pos.y = Mathf.Round(pos.y / snap) * snap;
        return pos;
    }
}
