using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{

    public string NameScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void EnterSccene()
    {
        SceneManager.LoadScene(NameScene);
    }
    public void Quit()
    {
        //
    }
}
