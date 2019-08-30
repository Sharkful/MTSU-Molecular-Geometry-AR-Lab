using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenePlacement : MonoBehaviour
{
    GameObject[] molecules;
    GameObject moleculeCanvas;
    GameObject scenePlacer;
    private GameObject myCamera;
    
    void Start()
    {
        molecules = GameObject.FindGameObjectsWithTag("Molecule");
        moleculeCanvas = GameObject.Find("Molecule Canvas");
        scenePlacer = GameObject.Find("ScenePlacer");
        myCamera = GameObject.Find("Main Camera");
  

        foreach (GameObject mol in molecules)
        {
            mol.SetActive(false);  
        }

        moleculeCanvas.SetActive(false);
    }

   public void PlaceScene()
    {
        scenePlacer.transform.position = new Vector3(myCamera.transform.position.x, myCamera.transform.position.y - 0.2f, myCamera.transform.position.z);
    }

    public void ActivateScene()
    {
        foreach (GameObject mol in molecules)
        {
            mol.SetActive(true);
        }

        moleculeCanvas.SetActive(true);
    }
}
