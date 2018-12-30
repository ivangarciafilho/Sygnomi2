using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MoveTowardsWaypoint : MonoBehaviour
{
    public Transform waypoint;
    public float trackingSpeed = 1f;
    public float TriggeringDistance = 0.05f;

    private void OnEnable() { StartCoroutine( Track()); }
    private IEnumerator Track()
    {
        while (Vector3.Distance(transform.position, waypoint.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoint.position, Time.smoothDeltaTime * trackingSpeed);
            yield return null;
        }

        StartCoroutine(WaitForDistanceThreshold());
    }

    private IEnumerator WaitForDistanceThreshold()
    {
        while (Vector3.Distance(transform.position, waypoint.position) < TriggeringDistance)
        {
            yield return null;
        }

        StartCoroutine( Track());
    }
}
