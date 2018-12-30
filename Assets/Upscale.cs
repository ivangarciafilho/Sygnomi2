using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upscale : MonoBehaviour
{
    public Vector3 targetScale;
    public float upscalingSpeed = 1f;

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, Time.smoothDeltaTime*upscalingSpeed);
    }
}
