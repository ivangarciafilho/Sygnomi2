using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour
{
    public static Head instance { get; private set; }

    private void Awake()
    {
        if (instance!=null)
        {
            if (instance!=this)
            {
                DestroyImmediate(gameObject);
                return;
            }
        }

        instance = this;
    }
}
