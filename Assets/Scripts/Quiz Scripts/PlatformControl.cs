using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MtsuMLAR
{
    /// <summary>
    /// This class controls the platforms in the Molecular Geometry Lab Quiz Scene. It
    /// Has an answer choice associated with it, and notifies scene control when it interacts
    /// with the box holding the molecule the students must label
    /// </summary>
    public class PlatformControl : MonoBehaviour
    {
        #region Private Variables
        //Set the Molecular Geometry this platform corresponds to
        [SerializeField]
        private Box_Controller.MoleculeGeometry platformGeometry = Box_Controller.MoleculeGeometry.Bent;
        private Box_Controller BC;

        [SerializeField]
        private MeshRenderer platformRenderer = null;
        private Material platformMaterial;
        private Color answerHoverColor = Color.white;
        private Color defaultColor;
        #endregion

        //Property to be checked by Box Control to see if user chose correctly
        public Box_Controller.MoleculeGeometry PlatformGeometry { get => platformGeometry; }

        void Awake()
        {
            //Set up reference to quiz control
            BC = GameObject.Find("Box").GetComponent<Box_Controller>();
            if(platformRenderer == null)
            {
                Debug.LogWarningFormat("No renderer assigned to a platform control script, disabling script on {0]", gameObject.transform.parent.name);
                enabled = false;
            }
            platformMaterial = platformRenderer.material;
            defaultColor = platformMaterial.color;
        }

        /// <summary>
        /// This function tells QC what Answer is currently being hovered over by the box
        /// And sets the bool CheckAnswer telling BC to check the answer once the box is placed
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            BC.TriggeredPlatformGeometry = platformGeometry;
            BC.CheckAnswer = true;
            platformMaterial.color = answerHoverColor;
        }

        /// <summary>
        /// This tells BC it no longer needs to check  any answers since the user removed the
        /// box from the platform. It also resets the bool limiting the incorrect animation to one playthrough
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            BC.CheckAnswer = false;
            if(BC.IncorrectPlayed)
            {
                BC.IncorrectPlayed = false;
            }
            platformMaterial.color = defaultColor;
        }
    }
}