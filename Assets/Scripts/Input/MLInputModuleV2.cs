using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MtsuMLAR
{
    /// <summary>
    /// This class manages input from the magic leap into objects with event handlers. 
    /// It allows different input methods paired with different input tools to generate
    /// a variety of input schemes
    /// **THIS CURRENTLY DOES NOT WORK WITH HANDS, CONECAST, OR SWITCHING BETWEEN INPUT TYPES AT RUNTIME**
    /// </summary>
    public class MLInputModuleV2 : MonoBehaviour
    {
        //Singleton instance
        //public static MLInputModuleV2 instance;

        #region Enums
        //defines how objects are targeted, whether by ray or proximity
        public enum InputMethod { Raycast, Conecast, ProximitySphere};

        //This indicates what object is used as the origin for the input methods
        public enum InputTool { Controller, LeftHand, RightHand};

        //Stores current State of the input module
        public enum HitState { ObjectHit, NoHit};
        #endregion

        #region Private Variables
        //Input Configuration Variables
        private InputMethod currentInputMethod = InputMethod.Raycast;
        private InputTool currentInputTool = InputTool.Controller;
        private HitState currentHitState;

        //Object references for origin transforms for the input tools
        public GameObject controllerObject = null;
        public GameObject rightHandObject = null;
        public GameObject leftHandObject = null;

        //The size of the proximity sphere that triggers interaction
        private float proximitySphereradius = 0.2f;

        //Stores objects targetted by the input module
        private List<GameObject> allHitObjects = null;
        //Object most likely to be interacted with
        private GameObject primaryHitObject;
        //Hit distance for a raycast, distance to transform of hit object for a proximity sphere
        private float primaryHitObjectDistance;
        #endregion

        #region Properties
        //These will eventually be expanded to allowing setting of input types, allowing other scripts to modify input behavior at runtime
        public InputMethod CurrentInputMethod { get => currentInputMethod; set => currentInputMethod = value; }
        public InputTool CurrentInputTool { get => currentInputTool; set => currentInputTool = value; }
        //Gets a reference to whatever object is selected as the primary input tool
        public GameObject PrimaryInputPointerObject
        {
            get
            {
                switch(currentInputTool)
                {
                    case InputTool.LeftHand:
                        return leftHandObject;
                    case InputTool.RightHand:
                        return rightHandObject;
                    default:
                        {
                            if(controllerObject == null)
                            {
                                controllerObject = GameObject.Find("Controller");
                            }
                            return controllerObject;
                        }
                }
            }
        }

        public HitState CurrentHitState { get => currentHitState; }
        public float ProximitySphereRadius { get => proximitySphereradius; set => proximitySphereradius = value; }

        //Output values of the input module, to be used by other classes when dealing with hit objects
        public List<GameObject> AllHitObjects { get => allHitObjects; }
        public GameObject PrimaryHitObject { get => primaryHitObject; }
        public float PrimaryHitObjectDistance { get => primaryHitObjectDistance; }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            //Singleton Setup
            //if (instance == null)
            //    instance = this;
            //else if (instance != this)
            //    Destroy(this);

            //Setup based on what is selected for input
            switch (currentInputTool)
            {
                case InputTool.LeftHand:
                    {
                        Debug.LogWarning("Hands Input is currently not Implemented, please use the controller. Disabling script");
                        enabled = false;
                        //Add handedness controls, checking for assigned gameobjects, and other implementations
                        break;
                    }
                case InputTool.RightHand:
                    {
                        Debug.LogWarning("Hands Input is currently not Implemented, please use the controller. Disabling script");
                        enabled = false;
                        //Add handedness controls, checking for assigned gameobjects, and other implementations
                        break;
                    }
                case InputTool.Controller:
                    {
                        if(controllerObject == null)
                        {
                            Debug.LogWarning("No controller Gameobject assigned!, disabling script");
                            enabled = false;
                        }

                        if(currentInputMethod == InputMethod.ProximitySphere && controllerObject.GetComponent<SphereSelector>() == null)
                        {
                            //For proximity, we need a trigger collider, which is added through a managing script also added to the controller
                            SphereSelector controllerSphere = (SphereSelector)controllerObject.AddComponent(typeof(SphereSelector));
                        }
                        break;
                    }
                default:
                    break;
            }

            allHitObjects = new List<GameObject>();

            //initialize state
            currentHitState = HitState.NoHit;
        }

        /// <summary>
        /// Selection hits arre put into late update to make sure all other objects have finished moving
        /// before we check to see if they are being targetted, reducing glitchy selection behavior
        /// </summary>
        private void LateUpdate()
        {
            bool hit;
            switch (currentInputMethod)
            {
                case InputMethod.Raycast:
                    {
                        if (currentInputTool == InputTool.Controller)
                            hit = Raycast(controllerObject.transform);
                        else
                            //add Hand or Headpose implementation of control later
#pragma warning disable CS0642 // Possible mistaken empty statement
                            ;
#pragma warning restore CS0642 // Possible mistaken empty statement
                        break;
                    }
                case InputMethod.ProximitySphere:
                    {
                        //Check to see if a new object is closer, and therefore more likely to be interacted with
                        if(allHitObjects.Count > 1)
                        {
                            CheckProximity();
                        }
                        //This updates the inputModule hit state
                        currentHitState = primaryHitObject != null ? HitState.ObjectHit : HitState.NoHit;
                        break;
                    }
                default:
                    break;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Performs a raycast from the origin in its forward direction, 
        /// and returns true if it hit anything. It also updates the hitobject
        /// list and primary object distance based on the raycastHit distance
        /// </summary>
        /// <param name="origin">Source and forward direction of the raycast</param>
        /// <returns></returns>
        private bool Raycast(Transform origin)
        {
            //Reset list of hit objects
            allHitObjects.Clear();
            allHitObjects.TrimExcess();
            RaycastHit[] hits = Physics.RaycastAll(origin.position, origin.forward);
            //If we hit something, then add to our lists nd check for the closest one
            if (hits.Length > 0)
            {
                GameObject closestObject = hits[0].transform.gameObject;
                float closestDistance = hits[0].distance;
                for (int i = 0; i < hits.Length; i++)
                {
                    //GameObject searchedObject = SearchForEventHandlerInAncestors(hits[i].transform.gameObject);
                    //if (searchedObject == null)
                    //{
                    //    allHitObjects.Add(hits[i].transform.gameObject);
                    //    if (closestDistance > hits[i].distance)
                    //    {
                    //        closestObject = hits[i].transform.gameObject;
                    //        closestDistance = hits[i].distance;
                    //    }
                    //}
                    //else
                    //{
                    //    allHitObjects.Add(searchedObject);
                    //    if (closestDistance > hits[i].distance)
                    //    {
                    //        closestObject = searchedObject;
                    //        closestDistance = hits[i].distance;
                    //    }
                    //}

                    allHitObjects.Add(hits[i].transform.gameObject);
                    if(hits[i].distance < closestDistance)
                    {
                        closestObject = hits[i].transform.gameObject;
                        closestDistance = hits[i].distance;
                    }
                }
                primaryHitObject = closestObject;
                primaryHitObjectDistance = closestDistance;
                currentHitState = HitState.ObjectHit;
                return true;
            }
            else
            {
                currentHitState = HitState.NoHit;
                return false;
            }
        }

        /// <summary>
        /// This function is called by SphereSelector whenever an object enters its collider trigger
        /// adding it to the list of hit objects
        /// </summary>
        /// <param name="go">the  object that hit the trigger</param>
        public void AddProximityObject(GameObject go)
        {
            allHitObjects.Add(go);
            if ((go.transform.position - controllerObject.transform.position).sqrMagnitude < (primaryHitObject.transform.position - controllerObject.transform.position).sqrMagnitude)
                primaryHitObject = go;
        }

        /// <summary>
        /// This function is called by SphereSelector whenever an object leaves the collider
        /// trigger, removing it from the list of hit obejcts.
        /// </summary>
        /// <param name="go">object that left the trigger</param>
        public void RemoveProximityObject(GameObject go)
        {
            if (allHitObjects.Contains(go))
            {
                allHitObjects.Remove(go);
                if(primaryHitObject = go)
                {
                    CheckProximity();
                }
            }
        }

        /// <summary>
        /// Checks all the objects in the current hit objects llist to see if any of them are
        /// now closer than the current closest object.
        /// </summary>
        private void CheckProximity()
        {
            foreach(GameObject go in allHitObjects)
            {
                if ((go.transform.position - controllerObject.transform.position).sqrMagnitude < (primaryHitObject.transform.position - controllerObject.transform.position).sqrMagnitude)
                    primaryHitObject = go;
            }
        }
        #endregion
    }
}