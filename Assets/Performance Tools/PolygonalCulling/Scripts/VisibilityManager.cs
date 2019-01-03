using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NGS.PolygonalCulling
{
    public interface IVisibilityManager
    {
        Camera camera { get; }

        void Start();

        List<Renderer> CalculateVisibility();

        void Quit();
    }

    public class DynamicVisibilityManager : IVisibilityManager
    {
        public Camera camera { get; private set; }
        public Renderer[] renderers { get; private set; }
        public int raysPerFrame { get; private set; }
        public int screenError { get; private set; }
        public float objectsLifetime { get; private set; }
        public int layerMask { get; private set; }

        private float _time;
        private float[] _xPixels;
        private float[] _yPixels;
        private int _haltonIndex = 0;

        private Dictionary<Collider, Renderer> _cache = new Dictionary<Collider, Renderer>();

        private List<Renderer> _hittedObjects = new List<Renderer>();
        private List<Renderer> _visibleObjects = new List<Renderer>();


        public DynamicVisibilityManager(Camera camera, Renderer[] renderers, int raysPerFrame, int screenError, float objectsLifetime, int layerMask)
        {
            this.camera = camera;
            this.renderers = renderers;
            this.raysPerFrame = raysPerFrame;
            this.screenError = screenError;
            this.objectsLifetime = objectsLifetime;
            this.layerMask = layerMask;
        }

        public void Start()
        {
            _time = objectsLifetime;

            int pixelsCount = Mathf.FloorToInt(Screen.width * Screen.height / screenError);

            _xPixels = new float[pixelsCount];
            _yPixels = new float[pixelsCount];

            for (int i = 0; i < pixelsCount; i++)
            {
                _xPixels[i] = HaltonSequence(i, 2);
                _yPixels[i] = HaltonSequence(i, 3);
            }

            for (int i = 0; i < renderers.Length; i++)
                _cache.Add(renderers[i].GetComponent<Collider>(), renderers[i]);
        }

        public List<Renderer> CalculateVisibility()
        {
            _time -= Time.deltaTime;
            if (_time <= 0)
            {
                _time = objectsLifetime;

                _visibleObjects.Clear();

                _visibleObjects.AddRange(_hittedObjects);

                _hittedObjects.Clear();
            }

            RaycastHit hit;
            for (int i = 0; i < raysPerFrame; i++)
            {
                Ray ray = camera.ViewportPointToRay(new Vector3(_xPixels[_haltonIndex], _yPixels[_haltonIndex], 0f));

                _haltonIndex++;

                if (_haltonIndex >= _xPixels.Length)
                    _haltonIndex = 0;

                if (Physics.Raycast(ray, out hit, float.MaxValue, layerMask))
                {
                    Renderer renderer = _cache[hit.collider];

                    _hittedObjects.DistinctAdd(renderer);

                    _visibleObjects.DistinctAdd(renderer);
                }
            }

            return _visibleObjects;
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


        public void Quit()
        {

        }
    }

    public class StaticVisibilityManager : IVisibilityManager
    {
        public Transform cameraTransform { get; private set; }
        public Camera camera { get; private set; }
        public Renderer[] renderers { get; private set; }

        private GraphicCallsHierarchy _hierarchy;
        private List<Renderer> _visibleRenderers = new List<Renderer>();


        public StaticVisibilityManager(Camera camera, Renderer[] renderers, GraphicCallsHierarchy hierarchy)
        {
            cameraTransform = camera.transform;

            this.camera = camera;

            this.renderers = renderers;

            _hierarchy = hierarchy;
        }

        public void Start()
        {

        }

        public List<Renderer> CalculateVisibility()
        {
            _visibleRenderers.Clear();

            List<int> indexes = _hierarchy.GetNodeByPoint(cameraTransform.position).renderersIndexes;

            for (int i = 0; i < indexes.Count; i++)
                _visibleRenderers.Add(renderers[indexes[i]]);

            return _visibleRenderers;
        }

        public void Quit()
        {

        }
    }

    public class MixedVisibilityManager : IVisibilityManager
    {
        public Camera camera { get; private set; }
        public Transform cameraTransform { get; private set; }
        public Renderer[] renderers { get; private set; }
        public int raysPerFrame { get; private set; }
        public int screenError { get; private set; }
        public int layerMask { get; private set; }

        private Dictionary<Collider, int> _cache = new Dictionary<Collider, int>();

        private GraphicCallsHierarchy _hierarchy;
        private string _hierarchyName;
        private List<Renderer> _visibleRenderers = new List<Renderer>();

        private float[] _xPixels;
        private float[] _yPixels;
        private int _haltonIndex = 0;


        public MixedVisibilityManager(Camera camera, Renderer[] renderers, int raysPerFrame, int screenError, int layerMask, GraphicCallsHierarchy hierarchy, string hierarchyName)
        {
            this.camera = camera;

            cameraTransform = camera.transform;

            this.renderers = renderers;
            this.raysPerFrame = raysPerFrame;
            this.screenError = screenError;
            this.layerMask = layerMask;

            _hierarchy = hierarchy;
            _hierarchyName = hierarchyName;
        }

        public void Start()
        {
            for (int i = 0; i < renderers.Length; i++)
                _cache.Add(renderers[i].GetComponent<Collider>(), i);

            int pixelsCount = Mathf.FloorToInt(Screen.width * Screen.height / screenError);

            _xPixels = new float[pixelsCount];
            _yPixels = new float[pixelsCount];

            for (int i = 0; i < pixelsCount; i++)
            {
                _xPixels[i] = HaltonSequence(i, 2);
                _yPixels[i] = HaltonSequence(i, 3);
            }
        }

        public List<Renderer> CalculateVisibility()
        {
            _visibleRenderers.Clear();

            GraphicCallsNode node = _hierarchy.GetNodeByPoint(cameraTransform.position);

            CastRays(node);

            List<int> indexes = node.renderersIndexes;

            for (int i = 0; i < indexes.Count; i++)
                _visibleRenderers.Add(renderers[indexes[i]]);

            return _visibleRenderers;
        }

        private void CastRays(GraphicCallsNode node)
        {
            RaycastHit hit;
            for (int i = 0; i < raysPerFrame; i++)
            {
                Ray ray = camera.ViewportPointToRay(new Vector3(_xPixels[_haltonIndex], _yPixels[_haltonIndex], 0f));

                _haltonIndex++;

                if (_haltonIndex >= _xPixels.Length)
                    _haltonIndex = 0;

                if (Physics.Raycast(ray, out hit, float.MaxValue, layerMask))
                    node.AddRenderer(_cache[hit.collider]);
            }
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


        public void Quit()
        {
#if UNITY_EDITOR

            HierarchyInfoLoader.SaveHierarchy(_hierarchyName, _hierarchy);

#endif
        }
    }
}