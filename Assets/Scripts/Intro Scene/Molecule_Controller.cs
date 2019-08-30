using System.Collections;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MtsuMLAR
{
    public class Molecule_Controller : MonoBehaviour, IMLPointerEnterHandler, IMLPointerExitHandler, IMLSelectHandler, IMLDeselectHandler, IMLBeginDragHandler, IMLDragHandler
    {
        #region Variables
        public MoleculeControllerConfig config = null;

        public GameObject selectSphere;
        public GameObject targetSphere;

        //Rendering Variables
        private MeshRenderer myMRend;
        private Material[] matList;

        //Drag Variables
        private float dragDistance;
        private Vector3 pointerPosLastFrame;
        private Vector3 pointerDirLastFrame;
        private Vector3 velocity;
        private Transform mainCamTransform;

        //Used to call depth and rotation update enumerators
        private bool rotationRunning = false;
        private bool depthRunning = false;
        #endregion

        #region Unity Methods
        void Awake()
        {
            if(config == null)
            {
                Debug.LogWarning("Molecule controller config object not assigned on " + gameObject.name + ".\nDeactivating Molecule Controller Script");
                enabled = false;
            }

            //Set up references for material manipulation
            myMRend = GetComponent<MeshRenderer>();
            matList = new Material[myMRend.materials.Length];
            matList = myMRend.materials;
        }

        void Start()
        {
            //Start input API
            MLInput.Start();

            //Used for updating depth change relative to the ML headset
            mainCamTransform = Camera.main.transform;
        }

        void OnDestroy()
        {
            RemoveMethodsFromControllerEvents();
            MLInput.Stop();
        }

        void Update()
        {
            if(depthRunning || rotationRunning)
            {
                if(!MLInput.GetController(MLInput.Hand.Left).Touch1Active)
                {
                    StopAllCoroutines();
                    rotationRunning = false;
                    depthRunning = false;
                }
            }
        }
        #endregion

        #region Event Methods
        /// <summary>
        /// This function enables emission in the molecule's materials, and sets the emission
        /// colors to yellow to indicate the object is targeted
        /// </summary>
        /// <param name="eventData"></param>
        public void MLOnPointerEnter(MLEventData eventData)
        {   
            //Check if this is the current selected object. if it is not, then set the color to the target color
            if (eventData.CurrentSelectedObject != gameObject)
            {
                /*foreach (Material _mat in matList)
                {
                    //if (!_mat.IsKeywordEnabled("_EMISSION"))
                    //    _mat.EnableKeyword("_EMISSION");
                    //if (_mat.GetColor("_EmissionColor") != config.targetColor)
                    //    _mat.SetColor("_EmissionColor", config.targetColor);
                    _mat.shader = Shader.Find("Custom/Outline");
                    _mat.SetColor("_OutlineColor", Color.yellow);
                    _mat.SetFloat("_Outline", config.outlineThickness);
                }*/
                targetSphere.SetActive(true);
                selectSphere.SetActive(false);
            }
            MLInput.GetController(MLInput.Hand.Left).StartFeedbackPatternVibe(config.vibPattern, config.vibIntensity);
        }

        /// <summary>
        /// This negates the previous targetting coloring when the object becomes detargeted.
        /// </summary>
        /// <param name="eventData"></param>
        public void MLOnPointerExit(MLEventData eventData)
        {
            //If the object is the current selected object, we don't want to disable the highlighting
            if (eventData.CurrentSelectedObject != gameObject)
            {
                /*foreach (Material _mat in matList)
                {
                    //if (_mat.IsKeywordEnabled("_EMISSION"))
                    // _mat.DisableKeyword("_EMISSION");
                    //_mat.shader = Shader.Find("Standard");
                }*/
                targetSphere.SetActive(false);
                selectSphere.SetActive(false);
            }
        }

        /// <summary>
        /// This switches the highlight color to white to indicate selection
        /// </summary>
        /// <param name="eventData"></param>
        public void MLOnSelect(MLEventData eventData)
        {
            /*foreach (Material _mat in matList)
            {
                 if (!_mat.IsKeywordEnabled("_EMISSION"))
                     _mat.EnableKeyword("_EMISSION");
                 if (_mat.GetColor("_EmissionColor") != config.selectColor)
                     _mat.SetColor("_EmissionColor", config.selectColor);
                _mat.shader = Shader.Find("Custom/Outline");
                _mat.SetColor("_OutlineColor", Color.white);
                _mat.SetFloat("_Outline", config.outlineThickness);
            }*/
            targetSphere.SetActive(false);
            selectSphere.SetActive(true);

            //Add touchpad gesture handlers when selected
            AddMethodsToControllerEvents();
        }

        /// <summary>
        /// this subscribes rotation and depth toggle functions to the magic leap global events once the object becomes selected
        /// </summary>
        /// <param name="eventData"></param>
        void AddMethodsToControllerEvents()
        {
            MLInput.OnControllerTouchpadGestureStart += ToggleRotation;
            MLInput.OnControllerTouchpadGestureStart += ToggleDepth;
        }

        /// <summary>
        /// This disables the highlighting when the object is deselected, 
        /// done by clicking on empty space
        /// </summary>
        /// <param name="eventData"></param>
        public void MLOnDeselect(MLEventData eventData)
        {
            /* foreach (Material _mat in matList)
             {
                 //if (_mat.IsKeywordEnabled("_EMISSION"))
                 // _mat.DisableKeyword("_EMISSION");
                 _mat.shader = Shader.Find("Standard");
             }*/

            targetSphere.SetActive(false);
            selectSphere.SetActive(false);

            //Remove touchpad gesture handlers when deselcted
            RemoveMethodsFromControllerEvents();
        }

        /// <summary>
        /// This unsubscribes the functions from ML events when the object is deselected
        /// </summary>
        /// <param name="eventData"></param>
        void RemoveMethodsFromControllerEvents()
        {
            MLInput.OnControllerTouchpadGestureStart -= ToggleRotation;
            MLInput.OnControllerTouchpadGestureStart -= ToggleDepth;
        }

        /// <summary>
        /// This function is called ONBeginDrag to set up offsets from where the pointer hit to the 
        /// object itself, making sure the object doesn't move further away or closer with every drag.
        /// </summary>
        /// <param name="eventData"></param>
        public void MLOnBeginDrag(MLEventData eventData)
        {
            pointerDirLastFrame = eventData.PointerTransform.forward;
            pointerPosLastFrame = eventData.PointerTransform.position;
            dragDistance = Mathf.Clamp((transform.position - eventData.PointerTransform.position).magnitude,0.5f,3f);
        }

        /// <summary>
        /// This is called every drag update frame, changing the position of the dragged object smoothly
        /// </summary>
        /// <param name="eventData"></param>
        public void MLOnDrag(MLEventData eventData)
        {
            Vector3 deltaControllerPos = (eventData.PointerTransform.position - pointerPosLastFrame);
            float depthChange = Vector3.Project(deltaControllerPos, pointerDirLastFrame).magnitude;
            float scalar = config.GetScalar(dragDistance);
            depthChange = (Vector3.Angle(deltaControllerPos, pointerDirLastFrame)) > 90f ? depthChange * -scalar : depthChange * scalar;
            dragDistance += depthChange;
            dragDistance = Mathf.Clamp(dragDistance, config.MinDepth, config.MaxDepth);
            Vector3 targetPosition = eventData.PointerTransform.position + eventData.PointerTransform.forward * dragDistance;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, config.smoothTime);
            pointerPosLastFrame = eventData.PointerTransform.position;
            pointerDirLastFrame = eventData.PointerTransform.forward;
        }

        /// <summary>
        /// This event function takes parameters from the Magic Leap events. If the touchpad
        /// press is more horizontal, then it starts the update rotation enumerator, which
        /// rotates the object at a constant speed.
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="touchpadGesture"></param>
        void ToggleRotation(byte controllerId, MLInputControllerTouchpadGesture touchpadGesture)
        {
            //The only way to start a rotation is by pressing on the touchpad
            if (touchpadGesture.Type == MLInputControllerTouchpadGestureType.ForceTapDown)
            {
                //Store the value(get value from the nullable type) and check if the x component is large enough to indicate a rotation
                Vector3 touchPosAndForce = touchpadGesture.PosAndForce.Value;
                //If the coroutine is already running, then don't call it again
                if (!rotationRunning && Mathf.Abs(touchPosAndForce.x) > 0.5f)
                {
                    //Set the enumerator to the rotation enumerator with the current touchpad value
                    StartCoroutine(UpdateRotation(touchPosAndForce.x));
                }
            }
        }

        /// <summary>
        /// This enumerator is called every frame until it is stopped, rotating the object at a constant speed
        /// </summary>
        /// <param name="touchPosAndForceX"></param>
        /// <returns></returns>
        IEnumerator UpdateRotation(float touchPosAndForceX)
        {
            //set the running flag, and direction of movement
            rotationRunning = true;
            int direction = (touchPosAndForceX > 0) ? -1 : 1;
            //Loop endlessly until this is shut off, rotate
            for (; ; )
            {
                transform.Rotate(Vector3.up, config.rotationSpeed * Time.deltaTime * direction, Space.World);
                yield return null;
            }
        }

        /// <summary>
        /// This responds to a MAgic Leap Event call for the touchpad. If the press is largely on the y-axis
        /// This starts the depth enumerator
        /// </summary>
        /// <param name="controllerId"></param>
        /// <param name="touchpadGesture"></param>
        void ToggleDepth(byte controllerId, MLInputControllerTouchpadGesture touchpadGesture)
        {
            //Only proceed if the gesture is a press on the touchpad
            if (touchpadGesture.Type == MLInputControllerTouchpadGestureType.ForceTapDown)
            {
                //Get and store the vector for the press position
                Vector3 touchPosAndForce = touchpadGesture.PosAndForce.Value;
                //If the coroutine is not already running, and the press is mostly vertical, start the depth update
                if (!depthRunning && Mathf.Abs(touchPosAndForce.y) > 0.5f)
                {
                    //Set the enumerator to the depth update enumerator, and call it
                    StartCoroutine(UpdateDepth(touchPosAndForce.y));
                }
            }
        }

        /// <summary>
        /// This enumerator updates the depth of the object every frame until it is stopped
        /// </summary>
        /// <param name="touchPosAndForceY"></param>
        /// <returns></returns>
        IEnumerator UpdateDepth(float touchPosAndForceY)
        {
            //Set the running flag and direction of movement
            depthRunning= true;
            int direction = (touchPosAndForceY > 0) ? 1 : -1;
            //Loop until this enumerator is stopped
            for (; ; )
            {
                //transform.Translate(Vector3.ProjectOnPlane((transform.position - mainCamTransform.position).normalized * direction * depthSpeed * Time.deltaTime, Vector3.up), Space.World);
                transform.Translate((transform.position - mainCamTransform.position).normalized * direction * config.depthSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
        }
        #endregion
    }
}