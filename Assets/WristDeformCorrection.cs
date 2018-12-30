using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristDeformCorrection : MonoBehaviour
{
    public Transform leftWrist;
    public Transform leftWristDeformer;

    public Transform rightWrist;
    public Transform rightWristDeformer;

    void Start()
    {        
    }

    void Update()
    {
        leftWristDeformer.localEulerAngles  = new Vector3(0, leftWrist.localEulerAngles.y-90, 0);
        rightWristDeformer.localEulerAngles = new Vector3(0, rightWrist.localEulerAngles.y+90, 0);
    }

}
