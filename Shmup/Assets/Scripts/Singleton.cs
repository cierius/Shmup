using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; } = null;

    public bool isUsingController = false; // Keyboard + mouse is default

    public int frameRateLimit = 120; // Default cap is 120 (60fps doesn't feel smooth enough)


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


    public void ResetLevel()
    {
        var currLevel = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currLevel.buildIndex, LoadSceneMode.Single);
    }
}
