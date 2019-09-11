using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtsuMLAR
{
    /// <summary>
    /// This class recieves trigger callbacks from the colllider, and then
    /// triggers the input module to either add or subtract the interacting
    /// object from its list of hit objects
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class SphereSelector : MonoBehaviour
    {
        private float radius;
        private MLInputModuleV2 inputModule;

        //If the radius is changed on the controller, it will update in the input module
        public float Radius
        {
            get => radius;
            set
            {
                if (inputModule.ProximitySphereRadius != value)
                    inputModule.ProximitySphereRadius = value;
                radius = value;
            }
        }

        private void Start()
        {
            inputModule = GameObject.FindGameObjectWithTag("MLEventSystem").GetComponent<MLInputModuleV2>();
            radius = inputModule.ProximitySphereRadius;
            SphereCollider proximitySphere = gameObject.GetComponent<SphereCollider>();
            proximitySphere.isTrigger = true;
            proximitySphere.center = Vector3.zero;
            proximitySphere.radius = radius;// This is done to make the radius accessible through this script after assignement
        }

        private void OnTriggerEnter(Collider other)
        {
            inputModule.AddProximityObject(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            inputModule.RemoveProximityObject(other.gameObject);
        }
    }
}