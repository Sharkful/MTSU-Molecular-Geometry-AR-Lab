using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    // The goal of this function is to allow the user to reset all molecule positions to their original positions, achieved by pressing a button using the Reset() function
    private GameObject[] molecules;
    private Vector3[] originalPositions = {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero }; // array that holds molecule positions

    
    public void InitializeReset()
    {
        int i = 0;
        molecules = GameObject.FindGameObjectsWithTag("Molecule"); // assigns all molecules to the array molecules
        
        foreach (GameObject mol in molecules)
        {
            originalPositions[i] = (mol.transform.localPosition); // assigns the initial positions to array originalPositions
            i++;
        }
    }

    public void Reset()
    {
        int i = 0;
        foreach (GameObject mol in molecules)
        {
            mol.transform.localPosition = originalPositions[i]; // sets the position of each molecule to be its original position
            i++;
        }
    } 
}