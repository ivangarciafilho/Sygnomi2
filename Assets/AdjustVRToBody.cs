using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RootMotion.FinalIK;

public class AdjustVRToBody : MonoBehaviour
{
    public Transform xrRig;
    public Transform modelHeadTarget;
    public Transform userHeadTarget;
    
    private void OnEnable()
    {
        if (userHeadTarget.position.y == 0) return;
        float heightAjustRatio = modelHeadTarget.position.y / userHeadTarget.position.y;
        xrRig.localScale = Vector3.one * heightAjustRatio;

        Vector3 positionOffset = modelHeadTarget.position - userHeadTarget.position;
        xrRig.position = xrRig.position + positionOffset;

        GetComponent<VRIK>().solver.spine.headTarget = userHeadTarget;
    }

}
