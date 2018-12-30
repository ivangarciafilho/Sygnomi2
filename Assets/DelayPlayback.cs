using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DelayPlayback : MonoBehaviour
{
    public VideoPlayer managedVideo;
    public float delay = 2f;


    private IEnumerator Start()
    {
        managedVideo.Pause();

        yield return new WaitForSeconds(delay);

        managedVideo.Play();
    }
}
