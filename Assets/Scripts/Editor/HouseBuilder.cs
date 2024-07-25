using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HouseBuilder : EditorWindow
{
    const string TOOL_NAME = "HouseBuilder";
    const string ASSET_ROOT = "Assets";
    const string TOOL_ASSET_FOLDER = "HouseAssets";

    [MenuItem("Tools/" + TOOL_NAME)]
    public static void ShowWindow() => GetWindow<HouseBuilder>();

    public float cellSize = 1f;
    public float gridSpan = 1f;

    private SerializedObject serializedObject;
    private SerializedProperty cellSizeProperty;
    private SerializedProperty gridSpanProperty;

    private GameObject chosenObject;
    Vector3 prefabDimensions = new Vector3(5, 5, 5);

    private string[] assetGuids;

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);

        if (!System.IO.Directory.Exists(ASSET_ROOT + "/" + TOOL_ASSET_FOLDER))
        {
            AssetDatabase.CreateFolder(ASSET_ROOT, TOOL_ASSET_FOLDER);
        }

        assetGuids = AssetDatabase.FindAssets("t:prefab", new[] { ASSET_ROOT + "/" + TOOL_ASSET_FOLDER });

        IEnumerable<string> assetPaths = assetGuids.Select(AssetDatabase.GUIDToAssetPath);

        GameObject[] loadedAssets = assetPaths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();

        chosenObject = loadedAssets.FirstOrDefault();

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        serializedObject.Update();

        if (serializedObject.ApplyModifiedProperties())
        {
            SceneView.RepaintAll();
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUI.FocusControl(null);
            Repaint();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        DisplayIcons();
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(mouseRay, out hitInfo) && hitInfo.collider != null)
        {
            if (chosenObject == null)
            {
                Debug.LogError("No object selected! Check the assets folder.");
                return;
            }
            DrawPreview(sceneView, hitInfo);
            if (Event.current.keyCode == KeyCode.Space && Event.current.type == EventType.KeyDown)
            {
                PlaceObject(hitInfo);
            }
        }
    }

    private void PlaceObject(RaycastHit hitInfo)
    {
        Vector3 positionToSpawn = (hitInfo.point + hitInfo.normal).Round(cellSize, prefabDimensions);
        Quaternion rotationToSpawn = Quaternion.identity;

        GameObject spawnedObject = (GameObject)PrefabUtility.InstantiatePrefab(chosenObject);
        spawnedObject.transform.position = positionToSpawn;
        spawnedObject.transform.rotation = rotationToSpawn;

        Undo.RegisterCreatedObjectUndo(spawnedObject, "Created Object");
    }

    private void DrawPreview(SceneView sceneView, RaycastHit hitInfo)
    {
        Handles.color = Color.green;
        Handles.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * 1f, 0.1f);

        Vector3 previewPosition = (hitInfo.point + hitInfo.normal).Round(cellSize, prefabDimensions);
        Matrix4x4 transformMatrix = Matrix4x4.TRS(previewPosition, Quaternion.identity, Vector3.one);

        MeshFilter[] meshFilters = chosenObject.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            Matrix4x4 localToWorld = meshFilter.transform.localToWorldMatrix;
            Matrix4x4 worldMatrix = transformMatrix * localToWorld;

            Mesh mesh = meshFilter.sharedMesh;
            Material material = meshFilter.GetComponent<MeshRenderer>().sharedMaterial;

            material.SetPass(0);

            Graphics.DrawMesh(mesh, worldMatrix, material, 0, sceneView.camera);
        }
    }

    private void DisplayIcons()
    {
        Handles.BeginGUI();
        Rect iconRect = new Rect(10, 10, 64, 64);
        Texture objectIcon = AssetPreview.GetAssetPreview(chosenObject);

        GUI.Toggle(iconRect, false, new GUIContent(objectIcon));
        Handles.EndGUI();
    }
}
