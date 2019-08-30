using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenMenu : MonoBehaviour
{
    public GameObject Panel; // the panel that is animated
    public Text moreLess; // the button text you want to change

    public void OpenPanel() // this function sets a bool that triggers the panel to open/close
    {
        Animator animator = Panel.GetComponent<Animator>();
        bool isOpen = animator.GetBool("open");
        animator.SetBool("open", !isOpen);

        
        if (isOpen == false)
        {
            moreLess.text = "Less Info"; // if the panel is open (showing information), change button text to say "Less Info"
        }
        else if (isOpen == true)
        {
            moreLess.text = "More Info"; // if the panel is closed (not showing information), change button text to say "More Info"
        }
    }
}