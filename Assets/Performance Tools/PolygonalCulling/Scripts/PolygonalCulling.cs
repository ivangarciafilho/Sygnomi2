using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NGS.PolygonalCulling
{
    public enum VisibilityManagerType { Realtime, Baked, Mixed, Standard_Occlusion }

    public class PolygonalCulling : MonoBehaviour
    {
        [SerializeField]
        private Camera[] _cameras;

        public bool enableFrustum = false;

        public const string layerName = "PCulling_Layer";
        public static int layerMask
        {
            get
            {
                return 1 << LayerMask.NameToLayer(layerName);
            }
        }

        [SerializeField]
        private GameObject _polyDataRoot;
        public GameObject polyDataRoot
        {
            get
            {
                return _polyDataRoot;
            }
        }

        [SerializeField]
        private int _raysPerFrame = 500;
        public int raysPerFrame
        {
            get
            {
                return _raysPerFrame;
            }

            set
            {
                _raysPerFrame = Mathf.Clamp(value, 1, int.MaxValue);
            }
        }

        [SerializeField]
        private int _screenError = 4;
        public int screenError
        {
            get
            {
                return _screenError;
            }

            set
            {
                _screenError = Mathf.Clamp(value, 1, int.MaxValue);
            }
        }

        [SerializeField]
        private float _objectsLifetime = 1;
        public float objectsLifetime
        {
            get
            {
                return _objectsLifetime;
            }

            set
            {
                _objectsLifetime = Mathf.Clamp(value, Mathf.Epsilon, float.MaxValue);
            }
        }

        [SerializeField]
        private VisibilityManagerType _managersType;
        public VisibilityManagerType managersType
        {
            get
            {
                return _managersType;
            }
        }

        private IVisibilityManager[] _managers;

        [SerializeField]
        private string _hierarchyName = "null";
        private GraphicCallsHierarchy _hierarchy = null;

        [SerializeField]
        private Renderer[] _srcRenderers;
        public Renderer[] srcRenderers
        {
            get
            {
                return _srcRenderers;
            }
        }

        [SerializeField]
        private Renderer[] _renderers;
        public Renderer[] renderers
        {
            get
            {
                return _renderers;
            }
        }

        private List<Renderer> _visibleRenderers = new List<Renderer>();


        public void SetValues(Camera[] cameras, GameObject polyDataRoot, Renderer[] renderers, Renderer[] srcRenderers, VisibilityManagerType type, string hierarchyName)
        {
            _cameras = cameras;

            _polyDataRoot = polyDataRoot;

            _renderers = renderers;

            _srcRenderers = srcRenderers;

            _managersType = type;

            _hierarchyName = hierarchyName;
        }

        public void Clear()
        {
            if (_polyDataRoot != null)
                DestroyImmediate(_polyDataRoot);

            _srcRenderers.ForEach(r => r.enabled = true);

            _cameras = null;
            _renderers = null;
            _srcRenderers = null;

            if (_hierarchyName != "null")
            {
                HierarchyInfoLoader.DeleteHierarchyInfo(_hierarchyName);
                _hierarchyName = "null";
            }
        }


        private void Start()
        {
            if (_polyDataRoot == null)
            {
                enabled = false;
                return;
            }

            if (_managersType != VisibilityManagerType.Realtime && _managersType != VisibilityManagerType.Standard_Occlusion)
            {
                _hierarchy = HierarchyInfoLoader.LoadHierarchy(_hierarchyName);

                if (_hierarchy == null)
                {
                    Debug.Log("No hierarchy found");
                    enabled = false;
                    return;
                }
            }

            _srcRenderers.ForEach(r => r.enabled = false);

            if (_managersType == VisibilityManagerType.Standard_Occlusion)
            {
                _renderers.ForEach(r => r.enabled = true);
                StaticBatchingUtility.Combine(_polyDataRoot);
                enabled = false;
                return;
            }

            _renderers.ForEach(r => { r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false; });

            _managers = new IVisibilityManager[_cameras.Length];

            for (int i = 0; i < _managers.Length; i++)
            {
                if (_managersType == VisibilityManagerType.Baked)
                    _managers[i] = new StaticVisibilityManager(_cameras[i], _renderers, _hierarchy);

                else if (_managersType == VisibilityManagerType.Realtime)
                    _managers[i] = new DynamicVisibilityManager(_cameras[i], _renderers, _raysPerFrame, _screenError, _objectsLifetime, layerMask);

                else
                    _managers[i] = new MixedVisibilityManager(_cameras[i], _renderers, _raysPerFrame, _screenError, layerMask, _hierarchy, _hierarchyName);
            }

            _managers.ForEach(m => m.Start());
        }

        private void Update()
        {
            try
            {
                _visibleRenderers.ForEach(r => r.enabled = false);

                _visibleRenderers.Clear();

#if UNITY_EDITOR

                #region EditorCode

                if (enableFrustum)
                {
                    for (int i = 0; i < _managers.Length; i++)
                    {
                        if (!_managers[i].camera.enabled)
                            continue;

                        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_managers[i].camera);

                        List<Renderer> renderers = _managers[i].CalculateVisibility();

                        for (int c = 0; c < renderers.Count; c++)
                            if (GeometryUtility.TestPlanesAABB(planes, renderers[c].bounds))
                                _visibleRenderers.Add(renderers[c]);
                    }
                }
                else
                {
                    for (int i = 0; i < _managers.Length; i++)
                    {
                        if (!_managers[i].camera.enabled)
                            continue;

                        _visibleRenderers.AddRange(_managers[i].CalculateVisibility());
                    }
                }

                #endregion

#else

                #region BuildCode

            for (int i = 0; i < _managers.Length; i++)
            {
                if (!_managers[i].camera.enabled)
                    continue;

                _visibleRenderers.AddRange(_managers[i].CalculateVisibility());
            }

                #endregion
          
#endif

                _visibleRenderers.ForEach(r => r.enabled = true);
            }
            catch (System.Exception ex)
            {
#if UNITY_EDITOR

                Debug.Log(ex.Message);

#endif

                _renderers.ForEach(r => r.enabled = true);
            }
        }

        private void OnApplicationQuit()
        {
            if (_managers == null || _managers.Length == 0)
                return;

            _managers.ForEach(m => m.Quit());
        }
    }
}
