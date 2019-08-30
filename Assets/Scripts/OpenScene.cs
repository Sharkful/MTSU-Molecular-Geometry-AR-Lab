using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenScene : MonoBehaviour
{

    public string nextScene; // change this depending on what scene you want to open. Make sure this scene is included in your Build Settings in Unity.

    // This is a function meant to be attached to a button. When called, the function opens the desired scene in your project. 
    public void NextScene()
    {
        SceneManager.LoadScene(nextScene);
       
    }
}
