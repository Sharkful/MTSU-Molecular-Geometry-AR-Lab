using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour {

    public Text username = null;
    private string word = null;
    private int letterCount = 0;
	
	public void KeyboardInput(string letter)
    {
        letterCount++;
        word += letter;
        username.text = word;
    }

    public void OnBackspace()
    {
        word = word.Remove(letterCount-1);
        username.text = word;
        
        if (letterCount > 0){
            letterCount--;
        }

        else
        {
            letterCount = 0;
        }
      
    }

    public void OnEnter()
    {
        Debug.Log("Username is " + username.text); // edit to send username to instructor and load next scene
    }
}
