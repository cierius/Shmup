using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; } = null;

    public bool isUsingController = false; // Keyboard + mouse is default

    private int frameRateLimit = 60;


    private void Awake()
    {
        // Singleton method, only one instance
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        Application.targetFrameRate = frameRateLimit;
    }

    public void SwitchControlScheme()
    {
        if(isUsingController)
        {
            isUsingController = false;
        }
        else
        {
            isUsingController = true;
        }
        print("Using Controller: " + isUsingController);
    }
}
