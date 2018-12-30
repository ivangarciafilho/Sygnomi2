using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FireEventOnEnable : MonoBehaviour
{
    public UnityEvent eventsToFire;
    private void OnEnable() { eventsToFire.Invoke(); }
}
