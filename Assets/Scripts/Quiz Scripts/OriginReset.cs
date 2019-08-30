using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtsuMLAR
{
    public class OriginReset : MonoBehaviour
    {
        [SerializeField]
        private GameObject box = null;
        [SerializeField]
        private GameObject mainCamera = null;
        [SerializeField]
        private GameObject myCanvas = null;
        [SerializeField]
        private GameObject platforms = null;
        [SerializeField]
        private GameObject content = null;

        public void SetOrigin()
        {
            content.transform.position = mainCamera.transform.position;
        }

        public void MakeVisible()
        {
            box.SetActive(true);
            platforms.SetActive(true);
            myCanvas.SetActive(true);
        }
    }
}