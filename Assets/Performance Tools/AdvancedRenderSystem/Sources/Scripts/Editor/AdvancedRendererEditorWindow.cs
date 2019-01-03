using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace NGS.AdvancedRenderSystem
{
    public class ManagerWindow : EditorWindow
    {
        public AdvancedRenderer advancedRenderer;

        private void OnGUI()
        {
            if (advancedRenderer == null)
                Close();

            if (GUILayout.Button("Add Selection"))
                advancedRenderer.AddRenderers(GetSelectedRenderers());

            if (GUILayout.Button("Add Selection As Group"))
                advancedRenderer.AddAsGroup(GetSelectedObjects());

            EditorGUILayout.Space();

            if (GUILayout.Button("Add All Static Objects"))
                advancedRenderer.AddAllStaticObjects();

            EditorGUILayout.Space();

            if (GUILayout.Button("Remove Selection"))
                advancedRenderer.RemoveRenderers(GetSelectedRenderers());

            if (GUILayout.Button("Remove All Objects"))
                advancedRenderer.RemoveAllRenderers();
        }

        private Renderer[] GetSelectedRenderers()
        {
            List<Renderer> renderers = new List<Renderer>();

            if (Selection.activeGameObject != null)
            {
                foreach (var go in Selection.gameObjects)
                    renderers.AddRange(go.GetComponentsInChildren<Renderer>());
            }

            return renderers.ToArray();
        }

        private GameObject[] GetSelectedObjects()
        {
            return Selection.gameObjects;
        }
    }
}