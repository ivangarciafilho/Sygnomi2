using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NGS.AdvancedRenderSystem
{
    [Serializable]
    public class GroupedBillboardData : BaseBillboardData
    {
        [SerializeField]
        private Renderer[] _sources;
        public Renderer[] sources
        {
            get
            {
                return _sources;
            }
        }

        [SerializeField]
        private int _layer;
        public int layer
        {
            get
            {
                return _layer;
            }
        }

        public GroupedBillboardData(IEnumerable<Renderer> sources, Renderer billboard)
        {
            _sources = sources.ToArray();
            _billboard = billboard;

            _sourceBounds = GetBounds(sources);

            _billboardSize = billboard.bounds.extents.magnitude * 2.0f;
            _layer = AdvancedRenderer.layer;
        }

        public static Bounds GetBounds(IEnumerable<Renderer> renderers)
        {
            Vector3 min = Vector3.one * Mathf.Infinity;
            Vector3 max = Vector3.one * Mathf.NegativeInfinity;

            foreach (var renderer in renderers)
            {
                Bounds bounds = renderer.bounds;

                min.x = Mathf.Min(min.x, bounds.min.x);
                min.y = Mathf.Min(min.y, bounds.min.y);
                min.z = Mathf.Min(min.z, bounds.min.z);

                max.x = Mathf.Max(max.x, bounds.max.x);
                max.y = Mathf.Max(max.y, bounds.max.y);
                max.z = Mathf.Max(max.z, bounds.max.z);
            }

            return new Bounds((min + max) / 2, max - min);
        }

        public override bool Contains(Renderer renderer)
        {
            for (int i = 0; i < _sources.Length; i++)
                if (_sources[i] == renderer)
                    return true;

            return false;
        }

        public override void ToMesh()
        {
            for (int i = 0; i < _sources.Length; i++)
                _sources[i].enabled = true;

            billboard.enabled = false;
        }

        public override void ToBillboard()
        {
            for (int i = 0; i < _sources.Length; i++)
                _sources[i].enabled = false;

            billboard.enabled = true;
        }

        public override void UpdateBillboard(Camera renderCamera, RenderTexture texture)
        {
            int cachedLayer = _sources[0].gameObject.layer;

            for (int i = 0; i < _sources.Length; i++)
            {
                _sources[i].enabled = true;
                _sources[i].gameObject.layer = layer;
            }

            CentralizeCamera(renderCamera);

            renderCamera.targetTexture = texture;
            renderCamera.Render();

            for (int i = 0; i < _sources.Length; i++)
            {
                _sources[i].enabled = false;
                _sources[i].gameObject.layer = cachedLayer;
            }

            lastUpdateTime = 0f;

            billboard.transform.rotation = Quaternion.LookRotation(billboard.transform.position - renderCamera.transform.position);
            billboard.material.mainTexture = texture;

            this.texture = texture;
        }
    }
}