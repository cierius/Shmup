using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Awake()
    {
          
    }


    public void OnPlayButton()
    {
        print("Loading Level_1");
        SceneManager.LoadScene("Level_1");
    }


    public void OnExitButton()
    {
        print("Exitting Game");
        Application.Quit();
    }


    public void OnCreditsButton()
    {
        print("Loading Credits");
    }


    public void OnSettingsButton()
    {
        print("Openning Settings Menu");
    }
}
