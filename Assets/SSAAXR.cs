using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


[DisallowMultipleComponent]
public class SSAAXR : MonoBehaviour
{
    [SerializeField] private float renderscale = 1.5f;

#if UNITY_EDITOR
    private float previousRenderScale;
#endif

    private void Awake(){UpdateRenderScale(); }

#if UNITY_EDITOR
    private void FixedUpdate()
    {
        if (previousRenderScale == renderscale) return;
        UpdateRenderScale();
    }
#endif

    private void UpdateRenderScale()
    {
        XRSettings.eyeTextureResolutionScale = renderscale;

#if UNITY_EDITOR
        previousRenderScale = renderscale;
#endif
    }
}
