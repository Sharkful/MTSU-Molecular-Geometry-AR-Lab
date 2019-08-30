using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UnityEngine.XR.MagicLeap;

namespace MtsuMLAR
{
    [System.Serializable]
    public class ToggleEvent : UnityEvent<bool> { }

    public class ARToggleScript : MonoBehaviour, IMLPointerEnterHandler, IMLPointerExitHandler, IMLPointerDownHandler, IMLPointerUpHandler
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
        private Image togglebox = null;
        [SerializeField]
        private Image checkmark = null;
        [SerializeField]
        private Collider toggleCollider = null;

        //Actions to be assigned to by the menu control script, allowing button customization
        public ToggleEvent ValueChanged;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            idleColor = togglebox.color;
            if (ValueChanged == null)
                ValueChanged = new ToggleEvent();
        }

        void OnEnable()
        {
            togglebox.enabled = true;
            toggleCollider.enabled = true;
        }

        void OnDisable()
        {
            togglebox.enabled = false;
            toggleCollider.enabled = false;
        }

        public void MLOnPointerEnter(MLEventData eventData)
        {
            togglebox.color = highlightColor;
            MLInput.GetController(MLInput.Hand.Left).StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.Click, MLInputControllerFeedbackIntensity.Low);
        }

        public void MLOnPointerExit(MLEventData eventData)
        {
            togglebox.color = idleColor;
        }

        public void MLOnPointerDown(MLEventData eventData)
        {
            bool state = !checkmark.enabled;
            togglebox.color = pressColor;
            checkmark.enabled = state;
            ValueChanged.Invoke(state);
        }

        public void MLOnPointerUp(MLEventData eventData)
        {
            togglebox.color = highlightColor;
        }
    }
}