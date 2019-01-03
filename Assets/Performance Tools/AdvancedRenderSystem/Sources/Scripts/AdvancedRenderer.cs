using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NGS.AdvancedRenderSystem
{
    public class AdvancedRenderer : MonoBehaviour
    {
        public const string layerName = "Billboard";
        public static int layer
        {
            get
            {
                return LayerMask.NameToLayer(layerName);
            }
        }

        [SerializeField]
        private Camera _camera;
        public new Camera camera
        {
            get
            {
                return _camera;
            }

            set
            {
                if (value == null)
                    return;

                _camera = value;
            }
        }

        [SerializeField]
        private Material _billboardMaterial = null;
        public Material billboardMaterial
        {
            get
            {
                return _billboardMaterial;
            }

            set
            {
                if (value == null)
                    return;

                _billboardMaterial = value;
            }
        }

        [SerializeField]
        private int _updatesPerFrame = 60;
        public int updatesPerFrame
        {
            get
            {
                return _updatesPerFrame;
            }

            set
            {
                _updatesPerFrame = Mathf.Clamp(value, 2, int.MaxValue);
            }
        }

        [SerializeField]
        private float _replaceDistance = 20f;
        public float replaceDistance
        {
            get
            {
                return _replaceDistance;
            }

            set
            {
                _replaceDistance = Mathf.Clamp(value, 0.01f, float.MaxValue);
            }
        }

        [SerializeField]
        private float _updateAngle = 2f;
        public float updateAngle
        {
            get
            {
                return _updateAngle;
            }

            set
            {
                _updateAngle = Mathf.Clamp(value, 0, 360);
            }
        }

        private Plane[] _frustumPlanes = null;
        private List<BaseBillboardData> _billboardsToUpdate = new List<BaseBillboardData>();

        private Camera _renderCamera = null;
        private TexturesManager _texturesManager = null;

        [SerializeField]
        private List<BaseBillboardData> _billboardsData = new List<BaseBillboardData>();
        public List<BaseBillboardData> billboardsData
        {
            get
            {
                return _billboardsData;
            }
        }

        #region UnityEditor

        #if UNITY_EDITOR

        public void AddRenderers(IEnumerable<Renderer> renderers)
        {
            int count = _billboardsData.Count;

            foreach (var renderer in renderers)
            {
                if (CanUseRenderer(renderer))
                    _billboardsData.Add(CreateBillboardData(renderer));
            }

            count = _billboardsData.Count - count;

            Debug.Log("Added " + count + " Objects");
        }

        public void AddAsGroup(IEnumerable<GameObject> objects)
        {
            int count = _billboardsData.Count;

            foreach (var go in objects)
            {
                List<Renderer> renderers = new List<Renderer>(go.GetComponentsInChildren<Renderer>());

                int i = 0;
                while (i < renderers.Count)
                {
                    if (!CanUseRenderer(renderers[i]))
                        renderers.RemoveAt(i);
                    else
                        i++;
                }

                if (renderers.Count == 0)
                    continue;

                _billboardsData.Add(CreateGroupedBillboardData(renderers.ToArray()));
            }

            count = _billboardsData.Count - count;

            Debug.Log("Added " + count + " Objects");
        }

        public void AddAllStaticObjects()
        {
            Renderer[] renderers = FindObjectsOfType<Renderer>();

            AddRenderers(renderers);

            Debug.Log("All Renderers Added");
        }

        public void RemoveRenderers(IEnumerable<Renderer> renderers)
        {
            int count = _billboardsData.Count;

            foreach (var renderer in renderers)
            {
                for (int i = 0; i < _billboardsData.Count; i++)
                {
                    if (_billboardsData[i].Contains(renderer))
                    {
                        DestroyImmediate(_billboardsData[i].billboard.gameObject);
                        _billboardsData.RemoveAt(i);
                        break;
                    }
                }
            }

            count = count - _billboardsData.Count;

            Debug.Log("Removed " + count + " Objects");
        }

        public void RemoveAllRenderers()
        {
            for (int i = 0; i < _billboardsData.Count; i++)
                DestroyImmediate(_billboardsData[i].billboard.gameObject);

            _billboardsData.Clear();

            Debug.Log("All Renderers Removed");
        }

        private bool CanUseRenderer(Renderer renderer)
        {
            if (!UnityEditor.GameObjectUtility.AreStaticEditorFlagsSet(renderer.gameObject, UnityEditor.StaticEditorFlags.BatchingStatic))
                return false;

            if (renderer.GetComponent<MeshFilter>() == null)
                return false;

            for (int i = 0; i < _billboardsData.Count; i++)
                if (_billboardsData[i].Contains(renderer))
                    return false;

            return true;
        }


        private BillboardData CreateBillboardData(Renderer renderer)
        {
            Bounds rendererBounds = renderer.bounds;

            Renderer billboard = CreateBillboardObject().GetComponent<Renderer>();

            billboard.transform.position = rendererBounds.center;
            billboard.transform.rotation = Quaternion.LookRotation(billboard.transform.position - _camera.transform.position);
            billboard.transform.localScale = Vector3.one * rendererBounds.extents.magnitude * 2f;

            billboard.material = _billboardMaterial;
            billboard.enabled = false;

            return new BillboardData(renderer, billboard);
        }

        private GroupedBillboardData CreateGroupedBillboardData(Renderer[] renderers)
        {
            Bounds bounds = GroupedBillboardData.GetBounds(renderers);

            Renderer billboard = CreateBillboardObject().GetComponent<Renderer>();

            billboard.transform.position = bounds.center;
            billboard.transform.rotation = Quaternion.LookRotation(billboard.transform.position - _camera.transform.position);
            billboard.transform.localScale = Vector3.one * bounds.extents.magnitude * 2f;

            billboard.material = _billboardMaterial;
            billboard.enabled = false;

            return new GroupedBillboardData(renderers, billboard);
        }

        private GameObject CreateBillboardObject()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);

            go.transform.hideFlags = HideFlags.HideInHierarchy;

            DestroyImmediate(go.GetComponent<Collider>());

            return go;
        }
        
        #endif

        #endregion

        private void Start()
        {
            if (_camera == null)
            {
                enabled = false;
                return;
            }

            if (_billboardsData.Count == 0)
            {
                enabled = false;
                return;
            }

            InitializeRenderCamera();

            _texturesManager = new TexturesManager();

            for (int i = 0; i < _billboardsData.Count; i++)
            {
                _billboardsData[i].UpdateTime();
                _billboardsData[i].UpdateDistance(_camera);
                _billboardsData[i].UpdateAngle(_camera);
                _billboardsData[i].UpdateBillboard(_renderCamera, _texturesManager.GetTexture(_billboardsData[i], _camera));
                _billboardsData[i].ToBillboard();
            }
        }

        private void InitializeRenderCamera()
        {
            _renderCamera = new GameObject("Camera").AddComponent<Camera>();

            _renderCamera.transform.hideFlags = HideFlags.HideInHierarchy;

            _renderCamera.transform.position = _camera.transform.position;

            _renderCamera.transform.rotation = _camera.transform.rotation;

            _renderCamera.cullingMask = LayerMask.GetMask(layerName);

            _renderCamera.clearFlags = CameraClearFlags.SolidColor;

            _renderCamera.backgroundColor = new Color(0, 0, 0, 0);

            _renderCamera.useOcclusionCulling = false;

            _renderCamera.enabled = false;

            _renderCamera.transform.SetParent(_camera.transform);
        }

        private void Update()
        {
            _billboardsToUpdate.Clear();
            _frustumPlanes = GeometryUtility.CalculateFrustumPlanes(_camera);

            for (int i = 0; i < _billboardsData.Count; i++)
            {
                BaseBillboardData data = _billboardsData[i];

                data.UpdateTime();

                if (!GeometryUtility.TestPlanesAABB(_frustumPlanes, data.sourceBounds))
                    continue;

                data.UpdateDistance(_camera);

                if (!IsNeedReplace(data))
                {
                    data.ToMesh();
                    continue;
                }

                if (!data.isBillboard)
                    data.ToBillboard();

                data.UpdateAngle(_camera);

                if (!IsNeedUpdate(data))
                    continue;

                _billboardsToUpdate.Add(data);
            }

            int halfUpdatesPerFrame = Mathf.RoundToInt(_updatesPerFrame / 2f);
            int index = 0;

            for (int i = 0; i < Mathf.Min(halfUpdatesPerFrame, _billboardsToUpdate.Count); i++)
            {
                BaseBillboardData data = GetNearest(_billboardsToUpdate, ref index);

                _texturesManager.FreeTextureUsage(data);
                data.UpdateBillboard(_renderCamera, _texturesManager.GetTexture(data, _camera));

                _billboardsToUpdate.RemoveAt(index);
            }

            for (int i = 0; i < Mathf.Min(halfUpdatesPerFrame, _billboardsToUpdate.Count); i++)
            {
                BaseBillboardData data = GetOldest(_billboardsToUpdate, ref index);

                _texturesManager.FreeTextureUsage(data);
                data.UpdateBillboard(_renderCamera, _texturesManager.GetTexture(data, _camera));

                _billboardsToUpdate.RemoveAt(index);
            }
        }

        private bool IsNeedReplace(BaseBillboardData data)
        {
            return data.distance >= _replaceDistance;
        }

        private bool IsNeedUpdate(BaseBillboardData data)
        {
            return data.angle >= _updateAngle;
        }

        private BaseBillboardData GetNearest(List<BaseBillboardData> billboardsData, ref int index)
        {
            index = 0;
            BaseBillboardData data = billboardsData[0];

            int count = billboardsData.Count;

            for (int i = 1; i < count; i++)
            {
                if (billboardsData[i].distance < data.distance)
                {
                    data = billboardsData[i];
                    index = i;
                }
            }

            return data;
        }

        private BaseBillboardData GetOldest(List<BaseBillboardData> billboardsData, ref int index)
        {
            index = 0;
            BaseBillboardData data = billboardsData[0];

            int count = billboardsData.Count;

            for (int i = 1; i < count; i++)
            {
                if (billboardsData[i].lastUpdateTime > data.lastUpdateTime)
                {
                    data = billboardsData[i];
                    index = i;
                }
            }

            return data;
        }
    }
}
