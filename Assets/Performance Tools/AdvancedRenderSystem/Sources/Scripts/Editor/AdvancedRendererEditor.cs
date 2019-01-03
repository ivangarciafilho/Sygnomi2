using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using NGS.AdvancedRenderSystem;

[CustomEditor(typeof(AdvancedRenderer))]
public class AdvancedRendererEditor : Editor
{
    private static bool _showSelectedObjects = false;
    private AdvancedRenderer _advancedRenderer
    {
        get
        {
            return target as AdvancedRenderer;
        }
    }

    [MenuItem("Tools/NGSTools/AdvancedRenderSystem")]
    private static void CreateObject()
    {
        GameObject go = new GameObject("AdvancedRenderSystem", typeof(AdvancedRenderer));

        EditorHelper.AddLayer(AdvancedRenderer.layerName);

        Selection.activeGameObject = go;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Manager Window"))
        {
            var window = EditorWindow.GetWindow<ManagerWindow>();
            window.advancedRenderer = _advancedRenderer;
        }

        EditorGUILayout.Space();

        _advancedRenderer.camera = (Camera)EditorGUILayout.ObjectField("Camera : ", _advancedRenderer.camera, typeof(Camera), true);
        _advancedRenderer.billboardMaterial = (Material)EditorGUILayout.ObjectField("Billboard Material : ", _advancedRenderer.billboardMaterial, typeof(Material), true);

        EditorGUILayout.Space();

        _advancedRenderer.updatesPerFrame = EditorGUILayout.IntField("Updates Per Frame : ", _advancedRenderer.updatesPerFrame);
        _advancedRenderer.replaceDistance = EditorGUILayout.FloatField("Replace Distance : ", _advancedRenderer.replaceDistance);
        _advancedRenderer.updateAngle = EditorGUILayout.Slider("Update Angle : ", _advancedRenderer.updateAngle, 0, 360);

        EditorGUILayout.Space();

        if (GUILayout.Button(_showSelectedObjects ? "Hide Selected Objects" : "Show Selected Objects"))
            _showSelectedObjects = !_showSelectedObjects;
    }

    private void OnSceneGUI()
    {
        if (_showSelectedObjects)
        {
            List<BaseBillboardData> datas = _advancedRenderer.billboardsData;

            for (int i = 0; i < datas.Count; i++)
                EditorHelper.DrawWireCube(datas[i].sourceBounds.center, datas[i].sourceBounds.size, Color.blue);
        }
    }
}
