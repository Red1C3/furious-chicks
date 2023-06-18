using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu:MonoBehaviour
{
    public static void level1()
    {
        SceneManager.LoadScene("Level-1");
    }
    public static void level2()
    {
        SceneManager.LoadScene("Level-2");
    }
    public static void levelGen()
    {
        SceneManager.LoadScene("Level-Gen");
    }
    private void Update(){
        if(Input.GetKeyDown(KeyCode.Escape)){
            Application.Quit();
        }
    }
}
