using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box_Opener : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 3.0f;

    private void Update()
    {
        transform.Translate(Vector3.right * Time.deltaTime * Input.GetAxis("Horizontal") * moveSpeed);
    }

    public void OpenBox()
    {
        Animator animator = GetComponent<Animator>();
        if(animator != null)
        {
            bool isOpen = animator.GetBool("Open");
            animator.SetBool("Open", !isOpen);
        }
    }
}
