using UnityEngine;
using System;

namespace NGS.AdvancedRenderSystem
{
    [Serializable]
    public class BillboardData : BaseBillboardData
    {
        [SerializeField]
        private Renderer _source;
        public Renderer source
        {
            get
            {
                return _source;
            }
        }

        [SerializeField]
        private int _layer = 0;
        public int layer
        {
            get
            {
                return _layer;
            }
        }

        public BillboardData(Renderer source, Renderer billboard)
        {
            _source = source;
            _billboard = billboard;

            _sourceBounds = _source.bounds;

            _billboardSize = billboard.bounds.extents.magnitude * 2.0f;
            _layer = AdvancedRenderer.layer;
        }

        public override bool Contains(Renderer renderer)
        {
            return _source == renderer;
        }

        public override void ToMesh()
        {
            source.enabled = true;
            _billboard.enabled = false;
        }

        public override void ToBillboard()
        {
            source.enabled = false;
            billboard.enabled = true;
        }

        public override void UpdateBillboard(Camera renderCamera, RenderTexture texture)
        {
            bool enabled = source.enabled;

            source.enabled = true;

            int cachedLayer = source.gameObject.layer;
            source.gameObject.layer = layer;

            CentralizeCamera(renderCamera);

            renderCamera.targetTexture = texture;
            renderCamera.Render();

            source.gameObject.layer = cachedLayer;

            source.enabled = enabled;

            lastUpdateTime = 0f;

            billboard.transform.rotation = Quaternion.LookRotation(billboard.transform.position - renderCamera.transform.position);
            billboard.material.mainTexture = texture;

            this.texture = texture;
        }
    }
}
