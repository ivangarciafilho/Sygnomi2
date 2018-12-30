using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public static Transform currentObjectBeingGrabbed { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == transform.root) return;
        if (other.transform == currentObjectBeingGrabbed) return;

        //StopAllCoroutines();
        currentObjectBeingGrabbed = other.transform;
        other.transform.SetParent(transform);
        other.transform.localPosition = Vector3.zero;
        //StartCoroutine(FollowAndLookAt(other.transform));
    }

    private IEnumerator FollowAndLookAt( Transform _target)
    {
        while (true)
        {
            yield return null;
            _target.transform.position = transform.position;
            _target.transform.LookAt(Head.instance.transform);
        }
    }
}
