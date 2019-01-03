using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using NGS.PolygonalCulling;

[CustomEditor(typeof(PolygonalCulling))]
public class PolygonalCullingEditor : Editor
{
    private static bool _justCreated = false;

    [SerializeField]
    private static Bounds[] _drawBounds = null;

    [MenuItem("Tools/NGSTools/Polygonal Culling")]
    private static void CreateObject()
    {
        Selection.activeGameObject = new GameObject("PolygonalCulling", typeof(PolygonalCulling));

        _justCreated = true;
    }

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
            return;

        if (_justCreated)
        {
            _justCreated = false;
            OpenWindow();
        }

        var culling = target as PolygonalCulling;

        if (culling.polyDataRoot == null)
        {
            if (GUILayout.Button("Open window"))
                OpenWindow();

            return;
        }

        if (culling.managersType != VisibilityManagerType.Standard_Occlusion)
        {
            culling.enableFrustum = EditorGUILayout.Toggle("Enable Frustum", culling.enableFrustum);

            if (culling.managersType != VisibilityManagerType.Baked)
            {
                culling.screenError = EditorGUILayout.IntField("Screen error : ", culling.screenError);
                culling.raysPerFrame = EditorGUILayout.IntField("Rays per frame : ", culling.raysPerFrame);

                if (culling.managersType == VisibilityManagerType.Realtime)
                    culling.objectsLifetime = EditorGUILayout.FloatField("Objects lifetime : ", culling.objectsLifetime);
            }
        }

        if (GUILayout.Button(culling.polyDataRoot.hideFlags == HideFlags.None ? "Hide objects" : "Unhide objects"))
        {
            bool activity = culling.polyDataRoot.hideFlags == HideFlags.HideInHierarchy;

            culling.srcRenderers.ForEach(r => r.enabled = !activity);

            culling.polyDataRoot.hideFlags = activity ? HideFlags.None : HideFlags.HideInHierarchy;

            culling.renderers.ForEach(r => r.enabled = activity);
        }

        if (GUILayout.Button("Clear"))
        {
            Clear();

            culling.Clear();

            AssetDatabase.Refresh();
        }
    }

    private void OpenWindow()
    {
        var window = EditorWindow.GetWindow<PolygonalCullingEditorWindow>();

        Vector3 size = PolygonalCullingEditorWindow.stdWindowSize;

        window.position = new Rect(10, 20, size.x, size.y);
        window.minSize = size / 2;
        window.maxSize = size * 2;
        window.title = "Polygonal Culling";

        window.OnWindowChangedCallback += OnWindowChanged;
        window.OnVisualizeKDTreeCallback += VisualizeKDTree;
        window.OnHierarchyParametrsChanged += HierarchyParametrsChanged;
        window.OnVisualizeHierarchyCallback += VisualizeHierarchy;
        window.OnBakeCallback += Bake;
        window.OnCloseCallback += OnWindowClosed;
    }

    private void Clear()
    {
        MeshCollider[] colliders = ((PolygonalCulling)target).polyDataRoot.GetComponentsInChildren<MeshCollider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Deleting PCulling Data", "Ready : " + i + " of " + colliders.Length, (float)i / colliders.Length);

            if (colliders[i].sharedMesh == null)
                continue;

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(colliders[i].sharedMesh));
        }

        EditorUtility.ClearProgressBar();
    }


    private void OnSceneGUI()
    {
        if (_drawBounds == null || _drawBounds.Length == 0)
            return;

        _drawBounds.ForEach(b => EditorHelper.DrawWireCube(b.center, b.size, Color.yellow));
    }

    private void OnWindowChanged(WindowState state)
    {
        _drawBounds = null;
    }

    private void VisualizeKDTree(int maxStack, int minTrianglesCount)
    {
        KDTree kdTree = new KDTree();

        kdTree.CreateTree(GetMeshFilters(), maxStack, minTrianglesCount);

        _drawBounds = kdTree.leafs.Select(l => l.bounds).ToArray();
    }

    private void HierarchyParametrsChanged(Vector3 center, Vector3 size)
    {
        _drawBounds = new Bounds[] { new Bounds(center, size) };
    }

    private int VisualizeHierarchy(bool autoCalculate, Vector3 center, Vector3 size, float minNodeSize)
    {
        GraphicCallsHierarchy hierarchy = new GraphicCallsHierarchy();

        if (autoCalculate)
            hierarchy.CreateHierarchy(GetMeshFilters(), minNodeSize);
        else
            hierarchy.CreateHierarchy(center, size, minNodeSize);

        _drawBounds = hierarchy.leafs.Select(l => l.bounds).ToArray();

        return _drawBounds.Length;
    }

    private void Bake(Camera[] cameras, VisibilityManagerType type, int maxStack, int minTrianglesCount, bool autoCalculate, Vector3 center, Vector3 size, float minNodeSize, int accuracy)
    {
        MeshFilter[] filters = GetMeshFilters();

        GameObject polyDataRoot = null;
        Renderer[] renderers = null;

        KDTree kdTree = new KDTree();
        kdTree.CreateTree(filters, maxStack, minTrianglesCount);

        GraphicCallsHierarchy hierarchy = null;
        string hierarchyName = "null";

        if(type != VisibilityManagerType.Realtime && type != VisibilityManagerType.Standard_Occlusion)
        {
            hierarchy = new GraphicCallsHierarchy();

            if (autoCalculate)
                hierarchy.CreateHierarchy(filters, minNodeSize);
            else
                hierarchy.CreateHierarchy(center, size, minNodeSize);

            hierarchyName = hierarchy.GetHashCode().ToString();
        }

        PolygonalCullingSceneManager sceneManager = new PolygonalCullingSceneManager();
        sceneManager.CalculateVisibility(kdTree, hierarchy, type, accuracy, out polyDataRoot, out renderers);

        if(hierarchy !=null)
            HierarchyInfoLoader.SaveHierarchy(hierarchyName, hierarchy);

        Renderer[] srcRenderers = filters.Select(f => f.GetComponent<Renderer>()).ToArray();

        ((PolygonalCulling)target).SetValues(cameras, polyDataRoot, renderers, srcRenderers, type, hierarchyName);

        polyDataRoot.hideFlags = HideFlags.HideInHierarchy;
        ((PolygonalCulling)target).renderers.ForEach(r => r.enabled = false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void OnWindowClosed()
    {
        _drawBounds = null;

        System.GC.Collect();
    }


    private MeshFilter[] GetMeshFilters()
    {
        return (from f in FindObjectsOfType<MeshFilter>()
                where GameObjectUtility.AreStaticEditorFlagsSet(f.gameObject, StaticEditorFlags.BatchingStatic)
                where f.sharedMesh != null
                where f.GetComponent<Renderer>() != null
                where f.GetComponent<Renderer>().enabled
                where IsMeshValid(f.sharedMesh)
                select f).ToArray();
    }

    private bool IsMeshValid(Mesh mesh)
    {
        if (mesh.vertices.Length != mesh.normals.Length)
            return false;

        if (mesh.normals.Length != mesh.tangents.Length)
            return false;

        if (mesh.tangents.Length != mesh.uv.Length)
            return false;

        return true;
    }
}
