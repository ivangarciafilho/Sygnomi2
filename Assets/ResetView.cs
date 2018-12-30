using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetView : MonoBehaviour
{
    public Transform xrRig;
    public Transform camera;

    void Start()
    {
        Vector3 positionOffset = camera.position - xrRig.position;
        positionOffset.y = 0;
        xrRig.position = xrRig.position + positionOffset;
    }

}
