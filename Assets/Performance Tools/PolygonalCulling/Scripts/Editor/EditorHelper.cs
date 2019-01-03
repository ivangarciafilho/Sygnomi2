using UnityEngine;
using UnityEditor;

public static class EditorHelper 
{
    public static void AddLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 0; i < layers.arraySize; i++)
            if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                return;

        for (int i = 8; i < layers.arraySize; i++)
        {
            string name = layers.GetArrayElementAtIndex(i).stringValue;

            if (name == "")
            {
                layers.GetArrayElementAtIndex(i).stringValue = layerName;
                break;
            }
        }

        tagManager.ApplyModifiedProperties();
    }

    public static void IgnoreAllLayers(string layerName)
    {
        IgnoreAllLayers(LayerMask.NameToLayer(layerName));
    }

    public static void IgnoreAllLayers(int layer)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 0; i < layers.arraySize; i++)
        {
            string name = layers.GetArrayElementAtIndex(i).stringValue;

            if (name == "") continue;

            Physics.IgnoreLayerCollision(layer, LayerMask.NameToLayer(name), true);
        }
    }

    public static void DrawWireCube(Vector3 position, Vector3 size, Color color = default(Color))
    {
        var half = size / 2;

        Handles.color = color;

        Handles.DrawLine(position + new Vector3(-half.x, -half.y, half.z), position + new Vector3(half.x, -half.y, half.z));
        Handles.DrawLine(position + new Vector3(-half.x, -half.y, half.z), position + new Vector3(-half.x, half.y, half.z));
        Handles.DrawLine(position + new Vector3(half.x, half.y, half.z), position + new Vector3(half.x, -half.y, half.z));
        Handles.DrawLine(position + new Vector3(half.x, half.y, half.z), position + new Vector3(-half.x, half.y, half.z));
       
        Handles.DrawLine(position + new Vector3(-half.x, -half.y, -half.z), position + new Vector3(half.x, -half.y, -half.z));
        Handles.DrawLine(position + new Vector3(-half.x, -half.y, -half.z), position + new Vector3(-half.x, half.y, -half.z));
        Handles.DrawLine(position + new Vector3(half.x, half.y, -half.z), position + new Vector3(half.x, -half.y, -half.z));
        Handles.DrawLine(position + new Vector3(half.x, half.y, -half.z), position + new Vector3(-half.x, half.y, -half.z));
       
        Handles.DrawLine(position + new Vector3(-half.x, -half.y, -half.z), position + new Vector3(-half.x, -half.y, half.z));
        Handles.DrawLine(position + new Vector3(half.x, -half.y, -half.z), position + new Vector3(half.x, -half.y, half.z));
        Handles.DrawLine(position + new Vector3(-half.x, half.y, -half.z), position + new Vector3(-half.x, half.y, half.z));
        Handles.DrawLine(position + new Vector3(half.x, half.y, -half.z), position + new Vector3(half.x, half.y, half.z));
    }
}
