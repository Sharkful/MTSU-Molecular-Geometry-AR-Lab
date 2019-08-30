using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using MtsuMLAR;

public class incorrectAnimationTrigger : MonoBehaviour
{
    private MLEventSystem isDrag;
    private bool calledAnimation = false;

    private void Awake()
    {
        isDrag = GameObject.Find("MLEventSystem").GetComponent<MLEventSystem>();
    }

    private void OnTriggerStay(Collider other)
    {
        Animator animator = other.GetComponent<Animator>();
        if (animator != null && !isDrag.IsDragging && !calledAnimation)
        {
            animator.SetTrigger("Incorrect");
            calledAnimation = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        calledAnimation = false;
    }
}
