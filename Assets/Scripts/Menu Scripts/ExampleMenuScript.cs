using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

namespace MtsuMLAR
{
    public class ExampleMenuScript : MonoBehaviour
    {
        [Header("ExampleButton References")]
        [SerializeField, Tooltip("This links to a button to give it actual funcitonality")]
        private ARButtonScript exampleButtonScript = null;
        [SerializeField, Tooltip("This is a link to an object the button interacts with, in this case it toggles the image visibility")]
        private Image exampleImage = null;

        void Start()
        {
            //Reference checking
            if(exampleButtonScript == null)
            {
                Debug.LogErrorFormat("No button assigned to example menu script on {0}, disabling", name);
                enabled = false;
            }
            if(exampleImage == null)
            {
                Debug.LogErrorFormat("No Image assigned to example menu script on {0}, disbabling", name);
                enabled = false;
            }

            MLInput.Start();

            //Assign function to action on button, adding functionality to the button.
            //exampleButtonScript.PointerDown += ToggleImage;
        }

        private void OnDestroy()
        {
            //Memory management
            //exampleButtonScript.PointerDown -= ToggleImage;
            MLInput.Stop();
        }

        //The function to be added to our button
        void ToggleImage()
        {
            exampleImage.enabled = !exampleImage.enabled;
        }
    }
}