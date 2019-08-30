using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MtsuMLAR
{
    public enum RaycasterState { RayHit, UIHit, NoHit, IsDrag }
    public enum RaycasterMode { Linecast, Conecast, Grab }

    public class Raycaster : MonoBehaviour
    {
        #region Private Variables
        //Current state of the raycaster. Are we hitting something?
        private RaycasterState currentRaycasterState;

        [SerializeField,Tooltip("Allows Raycaster to Update This Game Object's Position")]
        private bool matchControllerPosition = true;

        [SerializeField,Tooltip("This sets how the raycaster selects objects")]
        private RaycasterMode currentRaycasterMode; //Currently, this doesn't make a difference

        //Drag check variables
        private bool isDragging = false;

        //Raycast variables
        private MLInputController _controller;     //Used for the origin of the raycast and updating gameobject position
        private bool didRayHit;
        private bool didUIHit;
        private GameObject prevHitObject = null;   

        private RaycastHit hit; // for the event system. set in update hits
        #endregion

        #region Public Properties
        public GameObject PrevHitObject { get => prevHitObject; set => prevHitObject = value; }
        public RaycasterState CurrentRaycasterState { get => currentRaycasterState; }
        public RaycastHit Hit { get => hit; }
        #endregion

        void Start()
        {
            //Setup MLInput and start getting data from the controller
            if (!MLInput.Start().IsOk)
            {
                Debug.LogWarning("MLInput failed to start, disabling Raycaster");
                enabled = false;
            }
            _controller = MLInput.GetController(MLInput.Hand.Left);

            currentRaycasterState = RaycasterState.NoHit;
        }

        private void OnDestroy()
        {
            MLInput.Stop();
        }

        private void Update()
        {
            if (matchControllerPosition)
            {
                //Updates the position and orientation of this game object to match that of the controller's
                transform.position = _controller.Position;
                transform.rotation = _controller.Orientation;
            }
        }

        /*This code is put into late update to make sure that any objects moving
         * in update have already done so before we raycast to check for collisions
         * in Late Update. This makes sure, for example, that the pointer does not 
         * move its end to the center of an object, and then the object moves making
         * the pointer look bad.*/
        void LateUpdate()
        {
            isDragging = MLEventSystem.current.IsDragging;
            UpdateHits();
            StateSwitcher();
        }

        /*This function performs a physics raycast. It updates
         * the flags and distance, as well as changing the 
         * previous hit object by the physics raycast*/
        private void UpdateHits()
        {
            if (!isDragging)
            {
                didRayHit = Physics.Raycast(transform.position, transform.forward, out hit);
                if (didRayHit)
                {
                    GameObject hitObject = hit.transform.gameObject;
                    GameObject hitObjectParent = hit.transform.parent?.gameObject;
                    if (hitObject.GetComponent<IMLEventHandler>() != null)
                    {
                        prevHitObject = hitObject;
                    }
                    else if (hitObjectParent?.GetComponent<IMLEventHandler>() != null)
                    {
                        prevHitObject = hitObjectParent;
                    }
                    else
                    {
                        prevHitObject = hitObject;
                    }
                    if (prevHitObject.tag == "ARUI")
                    {
                        didUIHit = true;
                    }
                    else
                        didUIHit = false;
                }
                else
                {
                    prevHitObject = null;
                    didUIHit = false;
                    didRayHit = false;
                }
            }
        }

        /*This updates the current state of the raycaster
         * based on hit flags and distances*/
        private void StateSwitcher()
        {
            if (isDragging)
                currentRaycasterState = RaycasterState.IsDrag;
            else if (didUIHit)
                currentRaycasterState = RaycasterState.UIHit;
            else if (didRayHit)
                currentRaycasterState = RaycasterState.RayHit;
            else                                               //If no hit flag is true, then nothing is being hit
                currentRaycasterState = RaycasterState.NoHit;
        }
    }
}