using System.Collections;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.UI;
using System;

//README ISAAC: I added some TODO comments that suggest how to deal with a "Next Question" button. 
namespace MtsuMLAR
{
    /// <summary>
    /// This class controlls the scene and box for the Molecular Geometry Quiz scene. It handles its own highlighting,
    /// recieves controller events, checks answers, and triggers its correct and incorrect animations
    /// </summary>
    public class Box_Controller : MonoBehaviour, IMLPointerEnterHandler, IMLPointerExitHandler, IMLBeginDragHandler, IMLDragHandler
    {
        //This enum defines the different possible geometries in the quiz
        public enum MoleculeGeometry { Bent, Linear, Tetrahedral, Octahedral, Square_Planar, Square_Pyramidal, Trigonal_Planar, Trigonal_Pyramidal, Trigonal_Bipyramidal }

        public Image progressBarFill;
        #region Private Variables
        //Highlighting/Outline Variables
        [Header("Outline Controls")]
        [SerializeField]
        private float lineThickness = 2f;
        [SerializeField]
        private Color defaultColor = Color.white;
        [SerializeField,Tooltip("These are references to every mesh on this object with an outliner script attatched")]
        private Outline[] outlineScripts = null;
        private bool outlineEnabled = false;
        private MLEventSystem magicES;         //Used to check if the outlined object is still being pointed at or interacted with
        private Animator boxAnim;

        //Drag Variables
        [Header("Box Movement")]
        [SerializeField, Tooltip("Set How fast dragged objects follow controller movement")]
        [Range(0.001f, 0.5f)]private float dragSmoothTime = 0.3f;
        private float dragDistance;
        private Vector3 velocity;
        private Vector3 pointerDirLastFrame;
        private Vector3 pointerPosLastFrame;

        //The cam transform is used to point the box at the user
        [SerializeField,Tooltip("This reference is what the box points at so it can always be seen from the front")]
        private Transform camTransform;

        //Box reset variables
        [Header("Box Reset Controls")]
        [SerializeField,Tooltip("How quickly the box moves to the start position")]
        private float boxResetTime = 0.2f;                               //This sets how quickly the box moves back to its dtarting point
        private bool resetCoroutineRunning = false;
        private bool needToResetBox = false;
        [SerializeField,Tooltip("The position the box starts in every question")]
        private Vector3 startPosition = new Vector3(-0.35f, -0.1f, 1);   //The position the box resets to for every new question
        private bool incorrectPlayed = false;
        private GameObject[] moleculesList = new GameObject[11];         //These will contain references to the molecules and their Lewis structures,
        private GameObject[] lewisStructuresList = new GameObject[11];   //So they can be toggled on and off for each question
        [SerializeField]
        private GameObject nextButton = null;
        [SerializeField]
        private GameObject finishButton = null;

        //Answer Checking Variables
        [Header("Answer Key")]
        [SerializeField,Tooltip("The list of correct answers, must match the order of the molecules and their lewis structures in the Hierarchy")]
        private MoleculeGeometry[] answerKey = new MoleculeGeometry[11];
        [SerializeField]
        private int numQuestionStart = 0;
        private int currentQuestionNumber;
        private MoleculeGeometry triggeredPlatformGeometry;
        private bool checkAnswer = false;
        private int numCorrect = 0;
        private int numIncorrect = 0;

        //Menu reference to enable finish screen
        [SerializeField]
        private GameObject finalMenu = null;
        [SerializeField]
        private Answers answerScript = null; // Myranda added this

        //Values accesed and changed by the platform script upon entering and exiting its trigger
        public MoleculeGeometry TriggeredPlatformGeometry { get => triggeredPlatformGeometry; set => triggeredPlatformGeometry = value; }
        public bool CheckAnswer { get => checkAnswer; set => checkAnswer = value; }
        public bool IncorrectPlayed { get => incorrectPlayed; set => incorrectPlayed = value; }
        #endregion

        #region Unity Methods
        void Awake()
        {
            currentQuestionNumber = numQuestionStart;

            //Set up references
            magicES = GameObject.Find("MLEventSystem").GetComponent<MLEventSystem>();
            boxAnim = GetComponent<Animator>();
            camTransform = GameObject.Find("Main Camera").transform;

            //Make sure Box is at the correct start position
            transform.localPosition = startPosition;

            //Set up references to the Molecule models and lewis structures
            //If I didn't know how many molecules we were testing, I would have to use a list
            //To append gameObjects, then convert to an array.
            int count = 0;
            foreach (Transform t in GameObject.Find("Molecules").transform)
            {
                moleculesList[count] = t.gameObject;
                count++;
            }
            count = 0;
            foreach (Transform t in GameObject.Find("Images").transform)
            {
                lewisStructuresList[count] = t.gameObject;
                count++;
            }

            //Set up the outline on the box
            foreach (Outline outline in outlineScripts)
            {
                outline.OutlineColor = defaultColor;
                outline.OutlineWidth = lineThickness;
            }

            //Enable the first molecule and its Lewis structure so the user can label its geometry
            moleculesList[currentQuestionNumber].SetActive(true);
            lewisStructuresList[currentQuestionNumber].SetActive(true);
        }

        void Update()
        {
            //This code makes the root object in the box hierarchy look at the camera while upright.
            //This doesn't conflict with the animator because the incorrect animation rotates
            //The [Rendering] subobject
            Quaternion rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane((camTransform.position - transform.position), Vector3.up), Vector3.up);
            transform.rotation = rotation;
            //transform.Rotate(Vector3.right, 90f, Space.Self);

            //This gets info about the current state of the animator state machine
            AnimatorStateInfo curState = boxAnim.GetCurrentAnimatorStateInfo(0);

            //Check that we aren't dragging or animating the box before we change its outline or animation state
            if (!magicES.IsDragging && curState.IsName("Default"))
            {
                //Since we know we know we aren't dragging or being animated, if a platform triggered the checkAnswer
                //bool, then we check the answer only if the box isn't being, or about to be reset
                if (checkAnswer && !needToResetBox && !resetCoroutineRunning)
                {
                    //If the answer is correct, set animator state to correct, set reset box bool
                    if (triggeredPlatformGeometry == answerKey[currentQuestionNumber])
                    {
                        boxAnim.Play("Correct State", 0, 0);
                        MLInput.GetController(MLInput.Hand.Left).StartFeedbackPatternEffectLED(
                            MLInputControllerFeedbackEffectLED.Blink,
                            MLInputControllerFeedbackEffectSpeedLED.Medium,
                            MLInputControllerFeedbackPatternLED.None,
                            MLInputControllerFeedbackColorLED.BrightShaggleGreen,
                            0.5f);
                        MLInput.GetController(MLInput.Hand.Left).StartFeedbackPatternVibe(
                            MLInputControllerFeedbackPatternVibe.Tick,
                            MLInputControllerFeedbackIntensity.Low);
                        needToResetBox = true;
                        numCorrect++;
                    }
                    //Only play the incorrect animation once. Resets if put onto another platform
                    else if (incorrectPlayed != true)
                    {
                        boxAnim.Play("Incorrect State", 0, 0);
                        MLInput.GetController(MLInput.Hand.Left).StartFeedbackPatternEffectLED(
                            MLInputControllerFeedbackEffectLED.Blink,
                            MLInputControllerFeedbackEffectSpeedLED.Medium,
                            MLInputControllerFeedbackPatternLED.None,
                            MLInputControllerFeedbackColorLED.BrightMissionRed,
                            0.5f);
                        MLInput.GetController(MLInput.Hand.Left).StartFeedbackPatternVibe(
                            MLInputControllerFeedbackPatternVibe.Buzz,
                            MLInputControllerFeedbackIntensity.Medium);
                        // Myranda added this to send to the Google Form
                        //add reference to Answers.cs here
                        incorrectPlayed = true;
                        numIncorrect++;
                    }
                    checkAnswer = false;
                }
            }
            curState = boxAnim.GetCurrentAnimatorStateInfo(0);
            //If it also isn't being pointed at, and isn't being animated, disable its outlines if needed
            if ((magicES.LastHitObject != gameObject) && curState.IsName("Default") && outlineEnabled)
                {
                    foreach (Outline outline in outlineScripts)
                    {
                        outline.enabled = false;
                    }
                }
        }
        #endregion

        /// <summary>
        /// This function is called at the end of the Correct Reset Animation, making the box move back to its starting position
        /// </summary>
        void ResetBox()
        {
            StartCoroutine("ResetBoxCoroutine");
        }

        /// <summary>
        /// This coroutine moves the box back to its starting position by translating it a little every frame, as well as 
        /// incrementing the molecule question
        /// </summary>
        /// <returns></returns>
        IEnumerator ResetBoxCoroutine()
        {
            //Make the Lewis structure and model not visible
            moleculesList[currentQuestionNumber].SetActive(false);
            lewisStructuresList[currentQuestionNumber].SetActive(false);
            //Move up the question counter
            currentQuestionNumber++;
            //Reset the bool because we are now reseting the box, And set the bool indicating the coroutine is running.
            needToResetBox = false;
            resetCoroutineRunning = true;
            //this loops, updating the position until its close enough to the start position
            while ((transform.localPosition - startPosition).magnitude > 0.05)
            {
                Vector3 vel = new Vector3();
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startPosition, ref vel, boxResetTime);
                yield return null;
            }
            //Make the next molecule and lewis structure visible, only if we haven't gone past the last molecule to test
            if (currentQuestionNumber < answerKey.Length)
            {
                moleculesList[currentQuestionNumber].SetActive(true);
                lewisStructuresList[currentQuestionNumber].SetActive(true);

                //Myranda added this for the progress bar
                progressBarFill.fillAmount = ((float)currentQuestionNumber) / (float)answerKey.Length;
                
                // TODO if (currentQuestionNumber == last question) {change NextQuestionButton text to "Finish" }
            }
            else // TODO this needs to depend on if its' the last question and the NextQuestionButton (which says "Finish") has been clicked
            {
                // for progress bar to fill 100%:
                progressBarFill.fillAmount = 1;
           
                finalMenu.SetActive(true);
                float percent = (float)numCorrect*100 / (float)(numCorrect + numIncorrect);
                string scores = numCorrect + "\n" + numIncorrect + "\n" + percent + "%";
                finalMenu.GetComponentInChildren<Text>().text = scores;


                ////Enable menu on the finish page, and set the values of correct and incorrect answers
                //menuScript.SetScore(numCorrect, numIncorrect);
                answerScript.SendAnswers(numCorrect, numIncorrect); // send the correct and incorrect answers to Answers.SendAnswers for grading
                //menuScript.EnableMenu(MenuSelector.MolGeoMenus.Finish);
            }
            //Indicate the coroutine is no longer running
            resetCoroutineRunning = false;
        }

        public void EnableFinishMenu()
        {
            finalMenu.SetActive(true);
            float percent = (float)numCorrect * 100 / (float)(numCorrect + numIncorrect);
            string scores = numCorrect + "\n" + numIncorrect + "\n" + percent + "%";
            finalMenu.GetComponentInChildren<Text>().text = scores;


            ////Enable menu on the finish page, and set the values of correct and incorrect answers
            //menuScript.SetScore(numCorrect, numIncorrect);
            answerScript.SendAnswers(numCorrect, numIncorrect); // send the correct and incorrect answers to Answers.SendAnswers for grading
                                                                //menuScript.EnableMenu(MenuSelector.MolGeoMenus.Finish);
        }

        public void TurnOnButton()
        {
            if (currentQuestionNumber + 1 == answerKey.Length)
                finishButton.SetActive(true);
            else
                nextButton.SetActive(true);
        }

        public void StartResetAnimation()
        {
            boxAnim.Play("Correct Reset State", 0, 0);
        }

        #region Event Methods
        /// <summary>
        /// This function enables the outline script and sets the outline color when the object is pointed
        /// at but not being actively animated, since the colors are being controlled and the scripts
        /// should already be enabled
        /// </summary>
        /// <param name="eventData"></param>
        public void MLOnPointerEnter(MLEventData eventData)
        {
            AnimatorStateInfo curState = boxAnim.GetCurrentAnimatorStateInfo(0);
            if(curState.IsName("Default"))
            {
                foreach (Outline outline in outlineScripts)
                {
                    outline.enabled = true;
                }
                outlineEnabled = true;
            }
        }

        /// <summary>
        /// This disables the outline scripts when the object is no longer being pointed at, but does not
        /// if it is currently being animated.
        /// </summary>
        /// <param name="eventData"></param>
        public void MLOnPointerExit(MLEventData eventData)
        {
            AnimatorStateInfo curState = boxAnim.GetCurrentAnimatorStateInfo(0);
            if (curState.IsName("Default"))
            {
                foreach (Outline outline in outlineScripts)
                {
                    outline.enabled = false;
                }
                outlineEnabled = false;
            }
        }

        /// <summary>
        /// This function is called OnBeginDrag to set up offsets from where the pointer hit to the 
        /// object itself, making sure the object doesn't move further away or closer with every drag.
        /// </summary>
        /// <param name="eventData"></param>
        public void MLOnBeginDrag(MLEventData eventData)
        {
            pointerDirLastFrame = eventData.PointerTransform.forward;
            pointerPosLastFrame = eventData.PointerTransform.position;
            dragDistance = Mathf.Clamp((transform.position - eventData.PointerTransform.position).magnitude, 0.5f, 3f);
        }

        /// <summary>
        /// This is called every drag update frame, changing the position of the dragged object smoothly
        /// </summary>
        /// <param name="eventData"></param>
        public void MLOnDrag(MLEventData eventData)
        {
            Vector3 deltaControllerPos = (eventData.PointerTransform.position - pointerPosLastFrame);
            float depthChange = Vector3.Project(deltaControllerPos, pointerDirLastFrame).magnitude;
            float scalar = 0.4f * dragDistance + 1.3f;
            depthChange = (Vector3.Angle(deltaControllerPos, pointerDirLastFrame)) > 90f ? depthChange * -scalar : depthChange * scalar;
            dragDistance += depthChange;
            dragDistance = Mathf.Clamp(dragDistance, 0.5f, 1.5f);
            Vector3 targetPosition = eventData.PointerTransform.position + eventData.PointerTransform.forward * dragDistance;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, dragSmoothTime);
            pointerPosLastFrame = eventData.PointerTransform.position;
            pointerDirLastFrame = eventData.PointerTransform.forward;
        }
        #endregion
    }
}
