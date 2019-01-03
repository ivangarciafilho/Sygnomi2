using UnityEngine;
using System;

namespace NGS.AdvancedRenderSystem
{
    [Serializable]
    public abstract class BaseBillboardData : ScriptableObject
    {
        [SerializeField]
        protected Bounds _sourceBounds;
        public Bounds sourceBounds
        {
            get
            {
                return _sourceBounds;
            }
        }

        [SerializeField]
        protected Renderer _billboard;
        public Renderer billboard
        {
            get
            {
                return _billboard;
            }
        }

        public RenderTexture texture { get; protected set; }

        [SerializeField]
        protected float _billboardSize;
        public float billboardSize
        {
            get
            {
                return _billboardSize;
            }
        }

        public bool isBillboard
        {
            get
            {
                return billboard.enabled;
            }
        }

        public float distance { get; protected set; }
        public float angle { get; protected set; }
        public float lastUpdateTime { get; protected set; }

        public abstract bool Contains(Renderer renderer);

        public abstract void ToMesh();

        public abstract void ToBillboard();

        public void UpdateTime()
        {
            lastUpdateTime += Time.deltaTime;
        }

        public void UpdateDistance(Camera camera)
        {
            Vector3 cameraPosition = camera.transform.position;

            float dx = Mathf.Max(sourceBounds.min.x - cameraPosition.x, 0, cameraPosition.x - sourceBounds.max.x);
            float dy = Mathf.Max(sourceBounds.min.y - cameraPosition.y, 0, cameraPosition.y - sourceBounds.max.y);
            float dz = Mathf.Max(sourceBounds.min.z - cameraPosition.z, 0, cameraPosition.z - sourceBounds.max.z);

            distance = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public void UpdateAngle(Camera camera)
        {
            angle = Vector3.Angle(-billboard.transform.forward, camera.transform.position - billboard.transform.position);
        }

        public abstract void UpdateBillboard(Camera renderCamera, RenderTexture texture);

        protected void CentralizeCamera(Camera camera)
        {
            float size = _sourceBounds.extents.magnitude * 2f;

            camera.transform.LookAt(_sourceBounds.center);

            Vector3 cameraDirection = camera.transform.position - _sourceBounds.center;

            float dist = Vector3.Distance(camera.transform.position, _sourceBounds.center);
            float fov = 2.0f * Mathf.Atan(size / (2.0f * dist)) * (180 / Mathf.PI);

            camera.fieldOfView = fov;
            camera.nearClipPlane = Mathf.Max(cameraDirection.magnitude - size, 0.01f);
            camera.farClipPlane = cameraDirection.magnitude + size;
        }
    }
}
