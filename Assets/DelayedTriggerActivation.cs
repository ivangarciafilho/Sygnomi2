using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayedTriggerActivation : MonoBehaviour
{
    [Serializable]
    public struct DelayedEvent
    {
        public float delay;
        public UnityEvent triggeredEvent;
    }

    public DelayedEvent[] triggeredEvents;

    private void OnEnable() { StopAllCoroutines(); StartCoroutine(FireEvents());}

    private IEnumerator FireEvents()
    {
        for (int i = 0; i < triggeredEvents.Length; i++)
        {
            yield return new WaitForSeconds(triggeredEvents[i].delay);
            triggeredEvents[i].triggeredEvent.Invoke();
        }
    }
}
