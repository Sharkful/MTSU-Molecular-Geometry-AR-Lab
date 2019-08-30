using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MtsuMLAR
{
    /// <summary>
    /// General Content Manipulation, allowing for easy placement
    /// and visibility toggling of many objects
    /// </summary>
    public class ContentManipulation : MonoBehaviour
    {
        public static ContentManipulation current;

        [SerializeField]
        private GameObject[] contentToHideOnAwakeShowOnStart = null;
        [SerializeField]
        private GameObject[] contentToPlace = null;
        [SerializeField]
        private Vector3 defaultOffset = new Vector3(0, 0, 0);
        [SerializeField]
        private GameObject[] contentToResetPositions = null;

        private Dictionary<GameObject, Vector3> resetPositions = null;
        private Dictionary<GameObject, Quaternion> resetRotations = null;

        void Awake()
        {
            if (current == null)
                current = this;
            else if (current != this)
                Destroy(gameObject);

            ///Assume content to place relative to the camera is 
            ///the object this is placed on
            if(contentToPlace == null)
            {
                Debug.LogFormat("No Gameobject Assigned to contentToPlace, assigning {0}", gameObject.name);
                contentToPlace[0] = gameObject;
            }

            if(contentToHideOnAwakeShowOnStart != null)
            {
                foreach (GameObject go in contentToHideOnAwakeShowOnStart)
                {
                    if (go.activeSelf == true)
                        go.SetActive(false);
                }
            }
            
            resetPositions = new Dictionary<GameObject, Vector3>();
            resetRotations = new Dictionary<GameObject, Quaternion>();
        }

        ///Gets the initial positions of all resettable objects assigned
        ///in the inspector
        public void InitializeResetPositions()
        {
            foreach(GameObject go in contentToResetPositions)
            {
                resetPositions.Add(go, go.transform.localPosition);
                resetRotations.Add(go, go.transform.rotation);
            }
        }

        /// <summary>
        /// Adds or updates an on=bjects reset position based on its current
        /// position.
        /// </summary>
        /// <param name="go">Object to add to the Reset dictionary</param>
        /// <returns>true on succesfully adding to the dictionary</returns>
        public void SetResetPosition(GameObject go)
        {
            if (go?.transform != null)
            {
                if (resetPositions.ContainsKey(go))
                {
                    resetPositions[go] = go.transform.localPosition;
                    resetRotations[go] = go.transform.rotation;
                }
                else
                {
                    resetPositions.Add(go, go.transform.localPosition);
                    resetRotations.Add(go, go.transform.rotation);
                }
            }   
        }

        /// <summary>
        /// Function overload allowing for manual placement of the 
        /// object's reset location
        /// </summary>
        /// <param name="go">object to add to the reset dictionary</param>
        /// <param name="position">reset location</param>
        /// <param name="rotation">reset rotation</param>
        /// <returns>true on succesfully adding to the dictionary</returns>
        public void SetResetPosition(GameObject go, Vector3 position, Quaternion rotation)
        {
            if (resetPositions.ContainsKey(go))
            {
                resetRotations[go] = rotation;
                resetPositions[go] = position;
            }
            else
            {
                resetPositions.Add(go, position);
                resetRotations.Add(go, rotation);
            }
        }

        /// <summary>
        /// resets all resettable objects
        /// </summary>
        public void ResetObjects()
        {
            foreach(GameObject go in resetPositions.Keys)
            {
                go.transform.localPosition = resetPositions[go];
                go.transform.rotation = resetRotations[go];
            }
        }

        public void HideContent()
        {
            foreach (GameObject go in contentToHideOnAwakeShowOnStart)
            {
                if (go.activeSelf == true)
                    go.SetActive(false);
            }
        }

        public void ShowContent()
        {
            foreach (GameObject go in contentToHideOnAwakeShowOnStart)
            {
                if (go.activeSelf == false)
                    go.SetActive(true);
            }
        }

        /// <summary>
        /// Places content relative to the camera
        /// </summary>
        /// <param name="offset"> offset from camera position in World Space</param>
        public void PlaceContentRelativeToCamera(Vector3 offset)
        {
            foreach(GameObject go in contentToPlace)
            {
                go.transform.position = Camera.main.transform.position + offset;
            }
        }

        /// <summary>
        /// Overload for a default offset set in the inspector
        /// </summary>
        public void PlaceContentRelativeToCamera()
        {
            foreach(GameObject go in contentToPlace)
            {
                go.transform.position = Camera.main.transform.position + defaultOffset;
            }
        }
    }
}