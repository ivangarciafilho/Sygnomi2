using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NGS.PolygonalCulling;

using Object = UnityEngine.Object;

public class PolygonalCullingSceneManager
{
    private Dictionary<Renderer, int> _cache;
    private Dictionary<KDTreeNode, Renderer[]> _nodesRenderers;
    private Renderer[] _renderers;
    private int _raysCount;
    private int _layerMask;
    private Collider _collider;

    public void CalculateVisibility(KDTree kdTree, GraphicCallsHierarchy hierarchy, VisibilityManagerType type, int accuracy, out GameObject polyDataRoot, out Renderer[] renderers)
    {
        if (!Directory.Exists(MeshesConverter.meshesSaveDirectory))
            Directory.CreateDirectory(MeshesConverter.meshesSaveDirectory);

        EditorHelper.AddLayer(PolygonalCulling.layerName);
        EditorHelper.IgnoreAllLayers(PolygonalCulling.layerName);

        polyDataRoot = MeshesConverter.GetTreeObjects(kdTree, out _cache, out _nodesRenderers);

        renderers = _renderers = _cache.Keys.Select(k => k).ToArray();

        if (type == VisibilityManagerType.Standard_Occlusion)
            return;

        StaticBatchingUtility.Combine(polyDataRoot);

        if (type == VisibilityManagerType.Realtime)
            return;

        _raysCount = accuracy;
        _layerMask = PolygonalCulling.layerMask;

        _collider = new GameObject("Collider").AddComponent<BoxCollider>();
        _collider.gameObject.layer = LayerMask.NameToLayer(PolygonalCulling.layerName);

        ComputeRenderersVisibility(kdTree, hierarchy);

        Object.DestroyImmediate(_collider.gameObject);
    }

    private void ComputeRenderersVisibility(KDTree kdTree, GraphicCallsHierarchy hierarchy)
    {
        List<GraphicCallsNode> callsNodes = hierarchy.leafs;

        for (int i = 0; i < callsNodes.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Calculating PC data", "Ready : " + i + " of " + callsNodes.Count, (float)i / callsNodes.Count);

            List<Renderer> visibleRenderers = new List<Renderer>();

            Bounds[] casters = VisibilityChecker.GetCasters(callsNodes[i]);

            TraverseTree(kdTree.rootNode, callsNodes[i], casters, visibleRenderers);

            callsNodes[i].AddRenderers(visibleRenderers.Select(r => _cache[r]));
        }

        EditorUtility.ClearProgressBar();
    }

    private void TraverseTree(KDTreeNode node, GraphicCallsNode callsNode, Bounds[] casters, List<Renderer> visibleRenderers)
    {
        if (node.left == null)
        {
            VisibilityChecker.GetVisibleRenderers(casters, _nodesRenderers[node], visibleRenderers, _collider, _layerMask, _raysCount);
            return;
        }

        if (!VisibilityChecker.IsCastersVisible(casters, node.bounds, visibleRenderers, _collider, _layerMask, _raysCount))
            return;

        if (Vector3.Distance(node.left.center, callsNode.center) > Vector3.Distance(node.right.center, callsNode.center))
        {
            TraverseTree(node.left, callsNode, casters, visibleRenderers);
            TraverseTree(node.right, callsNode, casters, visibleRenderers);
        }
        else
        {
            TraverseTree(node.right, callsNode, casters, visibleRenderers);
            TraverseTree(node.left, callsNode, casters, visibleRenderers);
        }
    }
}

public static class MeshesConverter
{
    public const string meshesSaveDirectory = "Assets/PCulling_Data/";

    private static Dictionary<Renderer, int> _cache;
    private static Dictionary<KDTreeNode, Renderer[]> _nodesRenderers;

    public static GameObject GetTreeObjects(KDTree kdTree, out Dictionary<Renderer, int> cache, out Dictionary<KDTreeNode, Renderer[]> nodesRenderers)
    {
        _cache = new Dictionary<Renderer, int>();
        _nodesRenderers = new Dictionary<KDTreeNode, Renderer[]>();

        GameObject root = new GameObject("PolygonalCulling_Data");

        List<KDTreeNode> leafs = kdTree.leafs;
        for (int i = 0; i < leafs.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Creating PC Data", "Ready : " + i + " of " + leafs.Count, (float)i / leafs.Count);
            _nodesRenderers.Add(leafs[i], CreateObjectsByTriangles(kdTree, leafs[i].triangles, root).ToArray());
        }

        EditorUtility.ClearProgressBar();

        cache = _cache;
        nodesRenderers = _nodesRenderers;

        return root;
    }

    private static List<Renderer> CreateObjectsByTriangles(KDTree kdTree, Triangle[] triangles, GameObject root)
    {
        List<Triangle>[] sortedTriangles = new List<Triangle>[kdTree.srcFilters.Length];
        List<Renderer> renderers = new List<Renderer>();

        for (int i = 0; i < sortedTriangles.Length; i++)
            sortedTriangles[i] = new List<Triangle>();

        for (int i = 0; i < triangles.Length; i++)
            sortedTriangles[triangles[i].meshFilterIndex].Add(triangles[i]);

        for (int i = 0; i < sortedTriangles.Length; i++)
        {
            if (sortedTriangles[i].Count > 0)
            {
                Renderer renderer = CreateObjectByTriangles(kdTree.srcFilters[i], sortedTriangles[i].ToArray());

                renderers.Add(renderer);

                renderer.transform.SetParent(root.transform);

                _cache.Add(renderer, _cache.Keys.Count);
            }
        }

        return renderers;
    }

    private static Renderer CreateObjectByTriangles(MeshFilter srcFilter, Triangle[] triangles)
    {
        GameObject go = new GameObject(srcFilter.gameObject.name);

        Mesh mesh = CreateMeshByTriangles(srcFilter.sharedMesh, triangles);

        go.transform.position = srcFilter.transform.position;
        go.transform.rotation = srcFilter.transform.rotation;
        go.transform.localScale = srcFilter.transform.lossyScale;

        go.layer = LayerMask.NameToLayer(PolygonalCulling.layerName);
        go.isStatic = true;

        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        Renderer renderer = (Renderer)go.AddComponent(srcFilter.gameObject.GetComponent<Renderer>().GetType());
        renderer.sharedMaterials = srcFilter.gameObject.GetComponent<Renderer>().sharedMaterials;

        go.AddComponent<MeshCollider>().sharedMesh = mesh;

        return renderer;
    }

    private static Mesh CreateMeshByTriangles(Mesh srcMesh, Triangle[] triangles)
    {
        int[] distinctTriangles = new int[triangles.Length * 3];
        for (int i = 0; i < distinctTriangles.Length; i += 3)
        {
            distinctTriangles[i] = triangles[i / 3].triangle1;
            distinctTriangles[i + 1] = triangles[i / 3].triangle2;
            distinctTriangles[i + 2] = triangles[i / 3].triangle3;
        }

        distinctTriangles = distinctTriangles.Distinct().ToArray();

        Vector3[] vertices = new Vector3[distinctTriangles.Length];
        Vector3[] normals = new Vector3[distinctTriangles.Length];
        Vector4[] tangents = new Vector4[distinctTriangles.Length];
        Vector2[] uv = new Vector2[distinctTriangles.Length];
        Vector2[] uv2 = srcMesh.uv2.Length == vertices.Length ? new Vector2[distinctTriangles.Length] : null;
        Vector2[] uv3 = srcMesh.uv3.Length == vertices.Length ? new Vector2[distinctTriangles.Length] : null;
        Vector2[] uv4 = srcMesh.uv4.Length == vertices.Length ? new Vector2[distinctTriangles.Length] : null;
        Color[] colors = srcMesh.colors.Length == vertices.Length ? new Color[distinctTriangles.Length] : null;

        for (int i = 0; i < distinctTriangles.Length; i++)
        {
            vertices[i] = srcMesh.vertices[distinctTriangles[i]];
            normals[i] = srcMesh.normals[distinctTriangles[i]];
            tangents[i] = srcMesh.tangents[distinctTriangles[i]];
            uv[i] = srcMesh.uv[distinctTriangles[i]];

            if (uv2 != null)
                uv2[i] = srcMesh.uv2[distinctTriangles[i]];

            if (uv3 != null)
                uv3[i] = srcMesh.uv3[distinctTriangles[i]];

            if (uv4 != null)
                uv4[i] = srcMesh.uv4[distinctTriangles[i]];

            if (colors != null)
                colors[i] = srcMesh.colors[distinctTriangles[i]];
        }

        List<int>[] meshTriangles = new List<int>[srcMesh.subMeshCount];
        for (int i = 0; i < meshTriangles.Length; i++) meshTriangles[i] = new List<int>();

        for (int i = 0; i < triangles.Length; i++)
        {
            meshTriangles[triangles[i].subMeshIndex].Add(Array.IndexOf(distinctTriangles, triangles[i].triangle1));
            meshTriangles[triangles[i].subMeshIndex].Add(Array.IndexOf(distinctTriangles, triangles[i].triangle2));
            meshTriangles[triangles[i].subMeshIndex].Add(Array.IndexOf(distinctTriangles, triangles[i].triangle3));
        }

        for (int i = 0; i < meshTriangles.Length; i++)
            if (meshTriangles[i].Count == 0)
                meshTriangles[i].AddRange(new int[] { 0, 0, 0 });

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.uv = uv;

        mesh.subMeshCount = srcMesh.subMeshCount;

        for (int i = 0; i < mesh.subMeshCount; i++)
            mesh.SetTriangles(meshTriangles[i].ToArray(), i);

        return SaveMeshToFile(mesh);
    }

    private static Mesh SaveMeshToFile(Mesh mesh)
    {
        string fullPath = meshesSaveDirectory + "Mesh " + mesh.GetHashCode() + ".asset";

        AssetDatabase.CreateAsset(mesh, fullPath);

        return (Mesh)AssetDatabase.LoadAssetAtPath(fullPath, typeof(Mesh));
    }
}

public static class VisibilityChecker
{
    public static Bounds[] GetCasters(GraphicCallsNode nodeCalls)
    {
        Bounds[] casters = new Bounds[8];

        casters[0] = new Bounds(nodeCalls.min, nodeCalls.size);
        casters[1] = new Bounds(nodeCalls.center + new Vector3(-nodeCalls.size.x, nodeCalls.size.y, -nodeCalls.size.z) / 2, nodeCalls.size);
        casters[2] = new Bounds(nodeCalls.center + new Vector3(nodeCalls.size.x, nodeCalls.size.y, -nodeCalls.size.z) / 2, nodeCalls.size);
        casters[3] = new Bounds(nodeCalls.center + new Vector3(nodeCalls.size.x, -nodeCalls.size.y, -nodeCalls.size.z) / 2, nodeCalls.size);
        casters[4] = new Bounds(nodeCalls.center + new Vector3(-nodeCalls.size.x, -nodeCalls.size.y, nodeCalls.size.z) / 2, nodeCalls.size);
        casters[5] = new Bounds(nodeCalls.center + new Vector3(-nodeCalls.size.x, nodeCalls.size.y, nodeCalls.size.z) / 2, nodeCalls.size);
        casters[6] = new Bounds(nodeCalls.max, nodeCalls.size);
        casters[7] = new Bounds(nodeCalls.center + new Vector3(nodeCalls.size.x, -nodeCalls.size.y, nodeCalls.size.z) / 2, nodeCalls.size);

        return casters;
    }

    public static void GetVisibleRenderers(Bounds[] casters, Renderer[] renderers, List<Renderer> visibleRenderers, Collider collider, int layerMask, int raysCount)
    {
        casters.ForEach(c => GetVisibleRenderers(c, renderers, visibleRenderers, collider, layerMask, raysCount));
    }

    public static void GetVisibleRenderers(Bounds caster, Renderer[] renderers, List<Renderer> visibleRenderers, Collider collider, int layerMask, int raysCount)
    {
        renderers = renderers.OrderByDescending(r => Vector3.Distance(r.bounds.center, caster.center)).ToArray();

        AAQuad[] casterQuads = AAQuad.GetAAQuads(caster);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (visibleRenderers.Contains(renderers[i]))
                continue;

            if (caster.Intersects(renderers[i].bounds))
            {
                visibleRenderers.Add(renderers[i]);
                continue;
            }

            Bounds rendererBounds = new Bounds(renderers[i].bounds.center, new Vector3(
                Mathf.Max(renderers[i].bounds.size.x, 0.01f),
                Mathf.Max(renderers[i].bounds.size.y, 0.01f),
                Mathf.Max(renderers[i].bounds.size.z, 0.01f)));

            collider.transform.position = rendererBounds.center;
            collider.transform.localScale = rendererBounds.size;

            AAQuad[] rendererQuads = AAQuad.GetAAQuads(rendererBounds);

            renderers[i].GetComponent<Collider>().enabled = false;

            for (int c = 0; c < casterQuads.Length; c++)
                if (casterQuads[c].CastRays(rendererQuads[c], visibleRenderers, collider, layerMask, raysCount))
                {
                    visibleRenderers.DistinctAdd(renderers[i]);
                    break;
                }

            renderers[i].GetComponent<Collider>().enabled = true;
        }
    }

    public static bool IsCastersVisible(Bounds[] casters, Bounds target, List<Renderer> visibleRenderers, Collider collider, int layerMask, int raysCount)
    {
        for (int i = 0; i < casters.Length; i++)
            if (IsCasterVisible(casters[i], target, visibleRenderers, collider, layerMask, raysCount))
                return true;

        return false;
    }

    public static bool IsCasterVisible(Bounds caster, Bounds target, List<Renderer> visibleRenderers, Collider collider, int layerMask, int raysCount)
    {
        if (caster.Intersects(target))
            return true;

        AAQuad[] casterQuads = AAQuad.GetAAQuads(caster);
        AAQuad[] targetQuads = AAQuad.GetAAQuads(target);

        collider.transform.position = target.center;
        collider.transform.localScale = target.size;

        for (int i = 0; i < casterQuads.Length; i++)
            if (casterQuads[i].CastRays(targetQuads[i], visibleRenderers, collider, layerMask, raysCount))
                return true;

        return false;
    }

    private struct AAQuad
    {
        public Vector3 point1 { get; private set; }
        public Vector3 point2 { get; private set; }
        public Vector3 point3 { get; private set; }
        public Vector3 point4 { get; private set; }

        public AAQuad(Vector3 min, Vector3 max)
        {
            Vector3 _min = new Vector3(
                Mathf.Min(min.x, max.x),
                Mathf.Min(min.y, max.y),
                Mathf.Min(min.z, max.z));

            Vector3 _max = new Vector3(
                Mathf.Max(min.x, max.x),
                Mathf.Max(min.y, max.y),
                Mathf.Max(min.z, max.z));

            point1 = _min;
            point3 = _max;

            if (_min.x.IsEqual(_max.x))
            {
                point2 = new Vector3(_min.x, _max.y, _min.z);
                point4 = new Vector3(_min.x, _min.y, _max.z);
            }
            else if (_min.y.IsEqual(_max.y))
            {
                point2 = new Vector3(_max.x, _min.y, _min.z);
                point4 = new Vector3(_min.x, _min.y, _max.z);
            }
            else
            {
                point2 = new Vector3(_min.x, _max.y, _min.z);
                point4 = new Vector3(_max.x, _min.y, _min.z);
            }
        }

        public static AAQuad[] GetAAQuads(Bounds bounds)
        {
            AAQuad[] quads = new AAQuad[6];

            quads[0] = new AAQuad(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), bounds.max);
            quads[1] = new AAQuad(bounds.min, new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));

            quads[2] = new AAQuad(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), bounds.max);
            quads[3] = new AAQuad(bounds.min, new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));

            quads[4] = new AAQuad(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), bounds.max);
            quads[5] = new AAQuad(bounds.min, new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));

            return quads;
        }

        public bool CastRays(AAQuad quad, Collider target, int layerMask, int raysCount)
        {
            return CastRays(quad, (hit) => { }, target, layerMask, raysCount);
        }

        public bool CastRays(AAQuad quad, List<Renderer> hittedObjects, Collider target, int layerMask, int raysCount)
        {
            Action<RaycastHit> action = (hit) =>
            {
                hittedObjects.DistinctAdd(hit.transform.GetComponent<Renderer>());
            };

            return CastRays(quad, action, target, layerMask, raysCount);
        }

        private bool CastRays(AAQuad quad, Action<RaycastHit> action, Collider target, int layerMask, int raysCount)
        {
            float verticalSize = Vector3.Distance(point1, point2);
            float horizontalSize = Vector3.Distance(point2, point3);

            if (verticalSize.IsEqual(0) || horizontalSize.IsEqual(0))
                return false;

            float targetVerticalSize = Vector3.Distance(quad.point1, quad.point2);
            float targetHorizontalSize = Vector3.Distance(quad.point2, quad.point3);

            if (targetVerticalSize.IsEqual(0) || targetHorizontalSize.IsEqual(0))
                return false;

            Func<float, float, float, float, Vector3> calcOffset;

            #region CalcOffset

            if (point1.x.IsEqual(point3.x))
            {
                calcOffset = (idx1, vertSize, idx2, horSize) =>
                {
                    return new Vector3(0, idx1 * vertSize, idx2 * horSize);
                };
            }
            else if (point1.y.IsEqual(point3.y))
            {
                calcOffset = (idx1, vertSize, idx2, horSize) =>
                {
                    return new Vector3(idx1 * vertSize, 0, idx2 * horSize);
                };
            }
            else
            {
                calcOffset = (idx1, vertSize, idx2, horSize) =>
                {
                    return new Vector3(idx1 * horSize, idx2 * vertSize, 0);
                };
            }

            #endregion

            RaycastHit hit;
            for (int i = 0; i < raysCount; i++)
            {
                float idx1 = HaltonSequence(i, 2);
                float idx2 = HaltonSequence(i, 3);

                Vector3 rayOrigin = point1 + calcOffset(idx1, verticalSize, idx2, horizontalSize);
                Vector3 rayEnd = quad.point1 + calcOffset(idx1, targetVerticalSize, idx2, targetHorizontalSize);

                if (Physics.Raycast(rayOrigin, rayEnd - rayOrigin, out hit, float.MaxValue, layerMask))
                {
                    if (hit.collider == target)
                        return true;

                    action(hit);
                }
            }

            return false;
        }

        private float HaltonSequence(int index, int b)
        {
            float res = 0f;
            float f = 1f / b;
            int i = index;
            while (i > 0)
            {
                res = res + f * (i % b);
                i = Mathf.FloorToInt(i / b);
                f = f / b;
            }
            return res;
        }
    }
}


