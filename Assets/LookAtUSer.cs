using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtUSer : MonoBehaviour
{
    public float adjustmentSpeed = 1f;

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            (Quaternion.LookRotation(Head.instance.transform.position - transform.position)),
            Time.smoothDeltaTime * adjustmentSpeed);
    }
}
