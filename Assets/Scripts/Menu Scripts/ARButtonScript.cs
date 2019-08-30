using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.MagicLeap;

namespace MtsuMLAR
{   
    public class ARButtonScript : MonoBehaviour, IMLPointerEnterHandler, IMLPointerExitHandler, IMLPointerDownHandler, IMLPointerUpHandler
    {
        #region Private Variables
        //Colors for the button in different states
        private Color idleColor = Color.white;
        [SerializeField]
        private Color highlightColor = Color.yellow;
        [SerializeField]
        private Color pressColor = Color.green;

        //button background reference
        [SerializeField]
        private Image buttonImage = null;
        [SerializeField]
        private Collider buttonCollider = null;

        //Actions to be assigned to by the menu control script, allowing button customization
        public UnityEvent PointerDown;
        
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            idleColor = buttonImage.color;
            if (PointerDown == null)
                PointerDown = new UnityEvent();
        }

        void OnEnable()
        {
            buttonImage.enabled = true;
            buttonCollider.enabled = true;
        }

        void OnDisable()
        {
            buttonImage.enabled = false;
            buttonCollider.enabled = false;
        }

        public void MLOnPointerEnter(MLEventData eventData)
        {
            buttonImage.color = highlightColor;
            MLInput.GetController(MLInput.Hand.Left).StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
        }

        public void MLOnPointerExit(MLEventData eventData)
        {
            buttonImage.color = idleColor;
        }

        public void MLOnPointerDown(MLEventData eventData)
        {
            buttonImage.color = pressColor;
            PointerDown.Invoke();
        }

        public void MLOnPointerUp(MLEventData eventData)
        {
            buttonImage.color = highlightColor;
        }
    }
}