using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LightScheduler : MonoBehaviour
{
    public Light[] managedLights;
    private int currrentActiveLight;
    private int amountOfLights;
    private float nextUpdate;
    private Light[] lightsToDeactivate;


    private void OnEnable() { LoadParams(); }
    private void Update() { SwitchCurrentActiveLight(); }

    private void LoadParams()
    {
        amountOfLights = managedLights.Length;
        lightsToDeactivate = new Light[amountOfLights -1];
    }

    private void SwitchCurrentActiveLight()
    {
        currrentActiveLight++;
        if (currrentActiveLight >= amountOfLights) { currrentActiveLight = 0; }
        managedLights[currrentActiveLight].enabled = true;
        lightsToDeactivate = managedLights.Where(_x => _x != managedLights[currrentActiveLight]).ToArray();
        TurnOffLights();
    }

    private void TurnOffLights()
    {
        for (int i = 0; i < lightsToDeactivate.Length; i++)
        {
            lightsToDeactivate[i].enabled = false;
        }
    }
}
