///********************************************************///
///-Please note that OnInitializePotentialDrag has not been 
///integrated into the event logic, so you can only use
///OnBeginDrag
///-If the drag system being used relies on forces applied
///to a rigid body, then the onDrag call and its
///ssurrounding logic need to be put in fixed update
///********************************************************///

using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MtsuMLAR
{
    public class MLEventSystem : MonoBehaviour
    {
        public static MLEventSystem current = null;

        //enum to for the state variable type governing primary button
        enum clickButton { trigger, bumper }

        #region Private Variables
        [SerializeField, Tooltip("Sets primary click, secondary click auto set")]
        private clickButton primaryClick = clickButton.trigger;

        /// <summary>
        /// These variables are used in the Event System decision structure,
        /// and also allow other objects current selected/targetted
        /// objects, as well as the state of the event system.
        /// </summary>
        private GameObject lastHitObject = null;      //This object is what the system considers as hit, and is used to compare to the raycaster value to determine transistions
        private GameObject lastSelectedObject = null; //This is the object the system considers as selected
        private GameObject hitObject;                 //This object holds the reference to the Raycaster's hit object, is used in comparisons in the decision structure
        private bool isDragging = false;              //Flag which causes the system to stop checking some things until draggin stops, also used by Raycaster
        private GameObject clickDownObject = null;    //The object the button was pressed down own. Used to check if it is released on the same object
        private GameObject click_2_DownObject = null;
        private float bumperTimer;                    //These time the interval between a down and an up event, allowing it to be processed as a click or not
        private bool bumperHeld = false;
        private float triggerTimer;

        //Object references used to obtain data like raycast state or controller events
        [SerializeField,Tooltip("The Scene Raycaster, typically on the controller, used to select objects")]
        private Raycaster _raycaster = null;
        MLInputController _controller;

        /*Data sent to handlers when they are called, has information about the hit
         * It is defined in the MtsuMLAR Namespace*/
        private MLEventData eventData;

        /// <summary>
        /// These references are called every frame when the conditions are met.
        /// So instead of Reusing GetComponent<>() Every Frame, they are cached to be
        /// reused.
        /// </summary>
        private IMLPointerStayHandler stayHandler;
        private IMLUpdateSelectedHandler updateSelectedHandler = null;
        private IMLInitializePotentialDragHandler potentialDragHandler;
        private IMLDragHandler dragHandler;

        [SerializeField, Tooltip("Set the time window in which a click must be completed for it to register")]
        private float clickWindow = 0.8f;
        #endregion

        #region Properties
        /// <summary>
        /// Accesible by other objects, lets them interact with objects labeled
        /// by the event system.
        /// </summary>
        public GameObject LastHitObject { get => lastHitObject; }
        public GameObject LastSelectedObject { get => lastSelectedObject; }
        public bool IsDragging { get => isDragging; }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            if (_raycaster == null)
            {
                Debug.LogError("No Raycaster assigned to the MLEventSystem, disabling");
                enabled = false;
            }

            eventData = new MLEventData();

            //Set up the Singleton
            if (current == null)
                current = this;
            else if (current != this)
                Destroy(gameObject);
        }

        // Start initializes references, the MLInput API, and event subscriptions
        void Start()
        {
            if (!MLInput.Start().IsOk)
            {
                Debug.LogWarning("MLInput failed to start, disabling MLEventSystem");
                enabled = false;
            }

            _controller = MLInput.GetController(MLInput.Hand.Left);

            MLInput.OnControllerButtonDown += ControllerButtonDownHandler;
            MLInput.OnControllerButtonUp += ControllerButtonUpHandler;
            MLInput.OnTriggerDown += TriggerDownHandler;
            MLInput.OnTriggerUp += TriggerUpHandler;
        }

        //This manages memory, ensuring no leftover event subscriptions
        private void OnDestroy()
        {
            MLInput.OnControllerButtonDown -= ControllerButtonDownHandler;
            MLInput.OnControllerButtonUp -= ControllerButtonUpHandler;
            MLInput.OnTriggerDown -= TriggerDownHandler;
            MLInput.OnTriggerUp -= TriggerUpHandler;

            MLInput.Stop();
        }

        /// <summary>
        /// Update is primarily responsible for taking hitobject output from the raycaster
        /// and determining when an object is being pointed at, and what transitions have 
        /// occured, and then calls the necessary event handlers. it also calls event 
        /// handlers when they have updates every fram, like Drag.
        /// </summary>
        void Update()
        {
            UpdateEventData(eventData);
            //If we are dragging, then we aren't interrested in interacting with new objects
            if (!isDragging)
            {
                /*Only continue if the raycaster is in the correct state. This makes sure objects aren't sent 
                 * events When interacting with a UI element in front of it*/
                if (_raycaster.CurrentRaycasterState == RaycasterState.RayHit || _raycaster.CurrentRaycasterState == RaycasterState.UIHit)
                {
                    //This sets the reference to the raycaster hitobject, this is the object that is evaluated.
                    hitObject = _raycaster.PrevHitObject;
                    //This is checking if we transitioned from hitting nothing to now hitting an object
                    if (lastHitObject == null)
                    {
                        //This checks if the hit object has an appropriate handler, and that it has methods subscribed before calling it
                        IMLPointerEnterHandler enterHandler = hitObject.GetComponent<IMLPointerEnterHandler>();
                        if (enterHandler != null)
                        {
                            enterHandler.MLOnPointerEnter(eventData);
                        }
                        //This sets upt the cached reference to the Stay handler so that GetComponent isn't called every frame
                        stayHandler = hitObject.GetComponent<IMLPointerStayHandler>();
                        lastHitObject = hitObject;
                    }
                    //This is checking if we are still pointing at the same object
                    else if (hitObject == lastHitObject)
                    {
                        //Check stay handler and call
                        if (stayHandler != null)
                        {
                            stayHandler.MLOnPointerStay(eventData);
                        }
                    }
                    else //We are now hitting a new object, so we call the exit handler on the previous hit object
                    {
                        //check and call the handler
                        IMLPointerExitHandler exitHandler = lastHitObject.GetComponent<IMLPointerExitHandler>();
                        if (exitHandler != null)
                        {
                            exitHandler.MLOnPointerExit(eventData);
                        }
                        //By setting this to null, the next update the system will call the enter handler on the new object
                        lastHitObject = null;
                    }
                }
                else //If the raycaster is not hitting an object, but a UI element or nothing, then do this:
                {
                    //If we switched from hitting an object to not, call its exit handler
                    if (lastHitObject != null)
                    {
                        //chack and call the handler
                        IMLPointerExitHandler exitHandler = lastHitObject.GetComponent<IMLPointerExitHandler>();
                        if (exitHandler != null)
                        {
                            exitHandler.MLOnPointerExit(eventData);
                        }
                        lastHitObject = null;
                    }
                }

                //Place the scene if held bumper
                if(_controller.IsBumperDown && Time.time - bumperTimer > 2.0f && bumperHeld == false)
                {
                    _controller.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.Bump, MLInputControllerFeedbackIntensity.Low);
                    ContentManipulation.current?.PlaceContentRelativeToCamera();
                    bumperHeld = true;
                }
            }
            else  //isDragging == true
            {
                //****If the drag update is physics base, then this must be placed in fixed updat****

                //When a drag starts, the dragged object stays the lastHitObject, since the event system stops updating it until the drag stops
                //If a drag starts, its basically assured there is an object to reference, but this makes sure it hasn't been spontaneously deleted
                if (lastHitObject != null)
                {
                    //check and call the handler
                    if (dragHandler != null)
                    {
                        dragHandler.MLOnDrag(eventData);
                    }
                }
            }

            //Every frame, if an object is selected, check to see if it has a  handler for selectedupdate, regardless of isDragging value
            if (lastSelectedObject != null)
            {
                if (updateSelectedHandler != null)
                {
                    updateSelectedHandler.MLOnUpdateSelected(eventData);
                }
            }
        }
        #endregion //Unity Methods

        /// <summary>
        /// This function updates the event Data class, which will allow recieving methods
        /// to make use of current raycaster information and selected object by the event
        /// system. The drag handler needs the raycaster transform for example, in order
        /// to follow it
        /// </summary>
        /// <param name="eventData">Class containing important data for the recieving methods to use</param>
        private void UpdateEventData(MLEventData eventData)
        {
            eventData.CurrentSelectedObject = lastSelectedObject;
            eventData.PointerRayHitInfo = _raycaster.Hit;
            eventData.PointerTransform = _raycaster.transform;
        }

        #region Button Handlers
        /// <summary>
        /// This function takes the button down event from the controller and decides what events to call based on
        /// other event system values, like the click window, primary button state, or the last hit object
        /// </summary>
        /// <param name="controllerID">Specific ID of the controller that sent the event</param>
        /// <param name="button">This enum contains what button sent the event</param>
        public void ControllerButtonDownHandler(byte controllerID, MLInputControllerButton button)
        {
            //Some controller validation, and making sure the event is from the bumper, else we don't do anything
            if (_controller != null && _controller.Id == controllerID && button == MLInputControllerButton.Bumper)
            {
                UpdateEventData(eventData);
                //Set the start time to test if when the button is released it was quick enough to be considered a click
                bumperTimer = Time.time;

                /* If this is the primary button
                 * Then it already started the drag, and shouldn't call any more button down events anyway. If it is
                 * the secondary button, then we aren't interested in any of its clicks during a drag
                 * also, only try to send events if the raycaster says we aren't hitting UI, because even if it hits
                 * nothing, it has to call exit handlers*/
                if (!isDragging)
                {
                    //This checks if the button was pressed while pointing at an object
                    if (lastHitObject != null)
                    {
                        //If the bumper is the primary clicker, then execute this code
                        if (primaryClick == clickButton.bumper)
                        {
                            //This reference allows the system to check if a click was released on the same object it started on, a criteria for some events
                            clickDownObject = lastHitObject;
                            //Get, check, and call the down handler on the object
                            IMLPointerDownHandler downHandler = lastHitObject.GetComponent<IMLPointerDownHandler>();
                            if (downHandler != null)
                            {
                                downHandler.MLOnPointerDown(eventData);
                            }
                            //If you click on a draggable object, then you may want to start a drag
                            //So check if it has this handler
                            IMLInitializePotentialDragHandler potentialDragHandler = lastHitObject.GetComponent<IMLInitializePotentialDragHandler>();
                            if (potentialDragHandler != null)
                            {
                                potentialDragHandler.MLOnInitializePotentialDrag(eventData);
                                                                                                     //If the object has a drag initializer, then a drag must be initiated through criteria evaluated elsewhere
                            }
                            else //The object has no potentialDragInitializer, so just start the drag
                            {
                                //For an object to be considered draggable, it must implement the beginDrag interface, as well as the Drag
                                IMLBeginDragHandler beginDragHandler = lastHitObject.GetComponent<IMLBeginDragHandler>();
                                if (beginDragHandler != null)
                                {
                                    beginDragHandler.MLOnBeginDrag(eventData);
                                    isDragging = true;
                                    //This assignment prevents the raycast from entering drag state without a prev hit object
                                    _raycaster.PrevHitObject = lastHitObject;
                                    dragHandler = lastHitObject.GetComponent<IMLDragHandler>(); //cache reference to drag handler
                                                                                                //If the object about to be dragged has not all ready been selected, then do this
                                    if (LastHitObject != lastSelectedObject)
                                    {
                                        //This is done so that the last selected object is no longer selected once a drag starts
                                        IMLDeselectHandler deselectHandler = lastSelectedObject?.GetComponent<IMLDeselectHandler>();
                                        if (deselectHandler != null)
                                        {
                                            deselectHandler.MLOnDeselect(eventData);
                                            lastSelectedObject = null;
                                        }
                                    }
                                }
                            }
                        }
                        else //bumper == click_2, not primary
                        {
                            click_2_DownObject = lastHitObject;//To be checked before select and click is called in up handler
                                                               //Get, check, and call handler:
                            IMLPointer_2_DownHandler down_2_Handler = lastHitObject.GetComponent<IMLPointer_2_DownHandler>();
                            if (down_2_Handler != null)
                            {
                                down_2_Handler.MLOnPointer_2_Down(eventData);
                            }
                        }
                    }
                    else //lastHitObject == null, not hitting anything
                    {
                        //set clickdownobjects to null so that the up handler function knows the down click was on nothing
                        if (primaryClick == clickButton.bumper)
                            clickDownObject = null;
                        else
                            click_2_DownObject = null;
                    }
                }
            }
        }

        /// <summary>
        /// This function is a handler for button up events from the controller. It will change the selected object,
        /// call related select handlers, or end drags
        /// </summary>
        /// <param name="controllerID">See above function</param>
        /// <param name="button"></param>
        void ControllerButtonUpHandler(byte controllerID, MLInputControllerButton button)
        {
            //Controller verification, make sure the bumper is the one that was pressed
            if (_controller != null && _controller.Id == controllerID && button == MLInputControllerButton.Bumper)
            {
                bumperHeld = bumperHeld ? !bumperHeld : bumperHeld;
                UpdateEventData(eventData);
                //Only call these if not dragging
                if (!isDragging)
                {
                    //Call this if the bumper was released on an object(RayHit)
                    if (lastHitObject != null)
                    {
                        //If this is the primary clicker, do this(update selected object)
                        if (primaryClick == clickButton.bumper)
                        {
                            //get, check, and call handler
                            IMLPointerUpHandler upHandler = lastHitObject.GetComponent<IMLPointerUpHandler>();
                            if (upHandler != null)
                            {
                                upHandler.MLOnPointerUp(eventData);
                            }
                            //Check to see if the clickdown started and ended on the same object. If it did, consider click or select handlers
                            if (clickDownObject != null && lastHitObject == clickDownObject)
                            {
                                //If the release was quick enough, it is considered a click
                                if (Time.time - bumperTimer < clickWindow)
                                {
                                    //get, check, and call handler
                                    IMLPointerClickHandler clickHandler = lastHitObject.GetComponent<IMLPointerClickHandler>();
                                    if (clickHandler != null)
                                    {
                                        clickHandler.MLOnPointerClick(eventData);
                                    }
                                }
                                //If no object was currently selected, then call the select handler on the new hit object
                                if (lastSelectedObject == null)
                                {
                                    //get, check, and call handler, update selected object only if the hit object has a select handler, indicating it is selectable
                                    IMLSelectHandler selectHandler = lastHitObject.GetComponent<IMLSelectHandler>();
                                    if (selectHandler != null)
                                    {
                                        selectHandler.MLOnSelect(eventData);
                                        lastSelectedObject = lastHitObject;
                                        //This caches the reference to the update select handler if it exists
                                        IMLUpdateSelectedHandler updateSelectedHandler = lastHitObject.GetComponent<IMLUpdateSelectedHandler>();
                                        UpdateEventData(eventData);
                                    }
                                }
                                //If there was a previously selected object, call its deselect only if the newly hit object is selectable, has a select handler
                                else if (lastHitObject != lastSelectedObject)
                                {
                                    IMLSelectHandler selectHandler = lastHitObject.GetComponent<IMLSelectHandler>();
                                    if (selectHandler != null)
                                    {
                                        selectHandler.MLOnSelect(eventData);
                                        IMLDeselectHandler deselectHandler = lastSelectedObject.GetComponent<IMLDeselectHandler>();
                                        if (deselectHandler != null)
                                        {
                                            deselectHandler.MLOnDeselect(eventData);//checks for not null
                                        }
                                        //This caches the reference to the update select handler if it exists
                                        IMLUpdateSelectedHandler updateSelectedHandler = lastHitObject.GetComponent<IMLUpdateSelectedHandler>();
                                        lastSelectedObject = lastHitObject;
                                    }
                                }
                            }
                        }
                        else //bumper == click_2, secondary click
                        {
                            //get, check, and call handler
                            IMLPointer_2_UpHandler up_2_Handler = lastHitObject.GetComponent<IMLPointer_2_UpHandler>();
                            if (up_2_Handler != null)
                            {
                                up_2_Handler.MLOnPointer_2_Up(eventData);
                            }
                            //If click started and ended on the same object, and was within the time window, treat it as a click
                            if (click_2_DownObject != null && lastHitObject == click_2_DownObject && Time.time - bumperTimer < clickWindow)
                            {
                                //get, chack, and call handler
                                IMLPointer_2_ClickHandler click_2_Handler = lastHitObject.GetComponent<IMLPointer_2_ClickHandler>();
                                if (click_2_Handler != null)
                                {
                                    click_2_Handler.MLOnPointer_2_Click(eventData);
                                }
                            }
                        }
                    }
                    else //No Current Hit Object, so released button on empty space
                    {
                        //If bumper is the primary button(can change selection), and it also began the click on empty space, then it is deselecting
                        //Also check if there is a last selected object to now deselect
                        if (primaryClick == clickButton.bumper && clickDownObject == null && lastSelectedObject != null)
                        {
                            //get, check, and call handler
                            IMLDeselectHandler deselectHandler = lastSelectedObject.GetComponent<IMLDeselectHandler>();
                            if (deselectHandler != null)
                            {
                                deselectHandler.MLOnDeselect(eventData);
                            }
                            lastSelectedObject = null; //update selected object to nothing
                        }
                    }
                }
                //if dragging and the primary button is released, stop the drag
                else if (isDragging == true && primaryClick == clickButton.bumper)
                {
                    //get, chack, and call handler
                    IMLEndDragHandler endDragHandler = lastHitObject.GetComponent<IMLEndDragHandler>();
                    if (endDragHandler != null)
                    {
                        endDragHandler.MLOnEndDrag(eventData);
                    }
                    //Double check that the dragged object isn't already the selected object
                    if (lastHitObject != lastSelectedObject)
                    {
                        //Call select handler at the end of the drag, update selected object
                        IMLSelectHandler selectHandler = lastHitObject.GetComponent<IMLSelectHandler>();
                        if (selectHandler != null)
                        {
                            selectHandler.MLOnSelect(eventData);
                            lastSelectedObject = lastHitObject;
                        }
                    }
                    isDragging = false; //End the drag, because primary button was released
                }
            }
        }
        #endregion //Button Handlers

        #region Trigger Handlers
        /// <summary>
        /// This function handles a trigger down event by determining what  events to call
        /// </summary>
        /// <param name="controllerID">see above</param>
        /// <param name="triggerValue"> float from 0 to 1 for how much the trigger was pressed at the time of the event call</param>
        void TriggerDownHandler(byte controllerID, float triggerValue)
        {
            if (_controller != null && _controller.Id == controllerID)
            {
                UpdateEventData(eventData);
                /* If this is the primary button
                 * Then it already started the drag, and shouldn't call any more button down events anyway. If it is
                 * the secondary button, then we aren't interested in any of its clicks during a drag, because even if it hits
                 * nothing, it has to call exit handlers*/
                if (!isDragging)
                {
                    //This checks if the button was pressed while pointing at an object
                    if (lastHitObject != null)
                    {
                        //Set the start time to test if when the trigger is released it was quick enough to be considered a click
                        triggerTimer = Time.time;
                        //If the trigger is the primary clicker, then execute this code
                        if (primaryClick == clickButton.trigger)
                        {
                            //This reference allows the system to check if a click was released on the same object it started on, a criteria for some events
                            clickDownObject = lastHitObject;
                            //Get, check, and call the down handler on the object
                            IMLPointerDownHandler downHandler = lastHitObject.GetComponent<IMLPointerDownHandler>();
                            if (downHandler != null)
                            {
                                downHandler.MLOnPointerDown(eventData);
                            }
                            //If you click on a draggable object, then you may want to start a drag
                            //So check if it has this handler
                            IMLInitializePotentialDragHandler potentialDragHandler = lastHitObject.GetComponent<IMLInitializePotentialDragHandler>();
                            if (potentialDragHandler != null)
                            {
                                potentialDragHandler.MLOnInitializePotentialDrag(eventData);
                                                                                                     //If the object has a potential drag initializer, then a drag must be actually started through criteria evaluated elsewhere
                            }
                            else //The object has no potentialDragInitializer, so just start the drag
                            {
                                //For an object to be considered draggable, it must implement the beginDrag interface, as well as the Drag
                                IMLBeginDragHandler beginDragHandler = lastHitObject.GetComponent<IMLBeginDragHandler>();
                                if (beginDragHandler != null)
                                {
                                    beginDragHandler.MLOnBeginDrag(eventData);
                                    isDragging = true;
                                    //This assignment prevents the raycast from entering drag state without a prev hit object
                                    _raycaster.PrevHitObject = lastHitObject;
                                    dragHandler = lastHitObject.GetComponent<IMLDragHandler>(); //cache reference to drag handler
                                                                                                //If the object about to be dragged has not all ready been selected, then do this
                                    if (lastSelectedObject != null && LastHitObject != lastSelectedObject)
                                    {
                                        //This is done so that the last selected object is no longer selected once a drag starts
                                        IMLDeselectHandler deselectHandler = lastSelectedObject?.GetComponent<IMLDeselectHandler>();
                                        if (deselectHandler != null)
                                        {
                                            deselectHandler.MLOnDeselect(eventData);
                                            lastSelectedObject = null;
                                        }
                                    }
                                }
                            }
                        }
                        else //trigger == click_2, not primary
                        {
                            click_2_DownObject = lastHitObject;//To be checked before select and click is called in up handler
                                                               //Get, check, and call handler:
                            IMLPointer_2_DownHandler down_2_Handler = lastHitObject.GetComponent<IMLPointer_2_DownHandler>();
                            if (down_2_Handler != null)
                            {
                                down_2_Handler.MLOnPointer_2_Down(eventData);
                            }
                        }
                    }
                    else //lastHitObject == null, not hitting anything
                    {
                        //reset clickdownobjects so that the up handler function knows the down click was on nothing
                        if (primaryClick == clickButton.trigger)
                            clickDownObject = null;
                        else
                            click_2_DownObject = null;
                    }
                }
            }
        }

        /// <summary>
        /// handles a trigger up event by sending events based on system logic
        /// </summary>
        /// <param name="controllerID">see above</param>
        /// <param name="triggerValue">a float from 0 to 1 for how much the trigger is pressed at the time of the event</param>
        void TriggerUpHandler(byte controllerID, float triggerValue)
        {
            if (_controller != null && _controller.Id == controllerID)
            {
                UpdateEventData(eventData);
                //Only call these if not dragging
                if (!isDragging)
                {
                    //Call this if the trigger was pressed on an object(RayHit)
                    if (lastHitObject != null)
                    {
                        //If the trigger is the primary clicker, do this(update selected object)
                        if (primaryClick == clickButton.trigger)
                        {
                            //get, check, and call handler
                            IMLPointerUpHandler upHandler = lastHitObject.GetComponent<IMLPointerUpHandler>();
                            if (upHandler != null)
                            {
                                upHandler.MLOnPointerUp(eventData);
                            }
                            //Check to see if the clickdown started and ended on the same object. If it did, consider click or select handlers
                            if (clickDownObject != null && lastHitObject == clickDownObject)
                            {
                                //If the release was quick enough, it is considered a click
                                if (Time.time - triggerTimer < clickWindow)
                                {
                                    //get, check, and call handler
                                    IMLPointerClickHandler clickHandler = lastHitObject.GetComponent<IMLPointerClickHandler>();
                                    if (clickHandler != null)
                                    {
                                        clickHandler.MLOnPointerClick(eventData);
                                    }
                                }

                                //If no object was currently selected, then call the select handler on the new hit object
                                if (lastSelectedObject == null)
                                {
                                    //get, check, and call handler, update selected object only if the hit object has a select handler, indicating it is selectable
                                    IMLSelectHandler selectHandler = lastHitObject.GetComponent<IMLSelectHandler>();
                                    if (selectHandler != null)
                                    {
                                        selectHandler.MLOnSelect(eventData);
                                        lastSelectedObject = lastHitObject;
                                        //This caches the reference to the update select handler if it exists
                                        IMLUpdateSelectedHandler updateSelectedHandler = lastHitObject.GetComponent<IMLUpdateSelectedHandler>();
                                    }
                                }
                                //If there was a previously selected object, call its deselect only if the newly hit object is selectable,ie. has a select handler
                                else if (lastHitObject != lastSelectedObject)
                                {
                                    IMLSelectHandler selectHandler = lastHitObject.GetComponent<IMLSelectHandler>();
                                    if (selectHandler != null)
                                    {
                                        IMLDeselectHandler deselectHandler = lastSelectedObject.GetComponent<IMLDeselectHandler>();
                                        if (deselectHandler != null)
                                        {
                                            deselectHandler.MLOnDeselect(eventData);//checks for not null
                                        }
                                        //This caches the reference to the update select handler if it exists
                                        IMLUpdateSelectedHandler updateSelectedHandler = lastHitObject.GetComponent<IMLUpdateSelectedHandler>();
                                        selectHandler.MLOnSelect(eventData);
                                        lastSelectedObject = lastHitObject;
                                    }
                                }
                            }
                        }
                        else //trigger == click_2, secondary click
                        {
                            //get, check, and call handler
                            IMLPointer_2_UpHandler up_2_Handler = lastHitObject.GetComponent<IMLPointer_2_UpHandler>();
                            if (up_2_Handler != null)
                            {
                                up_2_Handler.MLOnPointer_2_Up(eventData);
                            }
                            //If click started and ended on the same object, and was within the time window, treat it as a click
                            if (click_2_DownObject != null && lastHitObject == click_2_DownObject && Time.time - triggerTimer < clickWindow)
                            {
                                //get, chack, and call handler
                                IMLPointer_2_ClickHandler click_2_Handler = lastHitObject.GetComponent<IMLPointer_2_ClickHandler>();
                                if (click_2_Handler != null)
                                {
                                    click_2_Handler.MLOnPointer_2_Click(eventData);
                                }
                            }
                        }
                    }
                    else //No Current Hit Object, so released button on empty space
                    {
                        //If trigger is the primary button(can change selection), and it also began the click on empty space, then it is deselecting
                        //Also check if there is a last selected object to now deselect
                        if (primaryClick == clickButton.trigger && clickDownObject == null && lastSelectedObject != null)
                        {
                            //get, check, and call handler
                            IMLDeselectHandler deselectHandler = lastSelectedObject.GetComponent<IMLDeselectHandler>();
                            if (deselectHandler != null)
                            {
                                deselectHandler.MLOnDeselect(eventData);
                            }
                            lastSelectedObject = null; //update selected object to nothing
                        }
                    }
                }
                //This makes sure we aredragging, regardless of a UI hit, because we need to be able to end a drag at any time
                else if (isDragging == true)
                {
                    //If he bumper can change selections, is primary button
                    if (primaryClick == clickButton.trigger)
                    {
                        //get, chack, and call handler
                        IMLEndDragHandler endDragHandler = lastHitObject.GetComponent<IMLEndDragHandler>();
                        if (endDragHandler != null)
                        {
                            endDragHandler.MLOnEndDrag(eventData);
                        }
                        //Double check that the dragged object isn't already the selected object
                        if (lastHitObject != lastSelectedObject)
                        {
                            //Call select handler at the end of the drag, update selected object
                            IMLSelectHandler selectHandler = lastHitObject.GetComponent<IMLSelectHandler>();
                            if (selectHandler != null)
                            {
                                selectHandler.MLOnSelect(eventData);
                                lastSelectedObject = lastHitObject;
                            }
                        }
                        isDragging = false; //End the drag, because primary button was released
                    }
                }
            }
        }
        #endregion //Trigger Handlers
    }
}