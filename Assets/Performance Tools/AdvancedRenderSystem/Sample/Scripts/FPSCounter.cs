using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]
    private Text _textOutput = null;

    private int _framesCounter = 0;
    private int _fps = 0;

    private void Start()
    {
        if (_textOutput == null)
        {
            enabled = false;
            return;
        }

        InvokeRepeating("ResetCounter", 1f, 1f);
    }

    private void Update()
    {
        _framesCounter++;
    }

    private void ResetCounter()
    {
        _fps = _framesCounter;

        _textOutput.text = _fps.ToString();

        _framesCounter = 0;
    }
}
