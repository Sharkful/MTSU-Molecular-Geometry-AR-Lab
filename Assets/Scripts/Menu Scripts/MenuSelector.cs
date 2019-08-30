using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using MtsuMLAR;

namespace MtsuMLAR
{
    public class MenuSelector : MonoBehaviour
    {
        //This enum definition must match the Order of the menus in the hierarchy!!
        public enum MolGeoMenus { Intro, Start, Options, KeyboardPanel, Help, Quiz, Finish}

        [SerializeField]
        private MolGeoMenus currentlyEnabledMenu = MolGeoMenus.Quiz;

        private Transform[] menuArray;
        private bool menuOn = false;

        // Start is called before the first frame update
        void Awake()
        {
            Debug.Log("Awake called in Menu Selector");
            MLInput.Start();
            MLInput.OnControllerButtonDown += ToggleMenu;

            //Create an array of the available menus, so they can be accesed and enabled/disabled as needed
            List<Transform> menuList = new List<Transform>();
            foreach (Transform t in transform)
            {
                menuList.Add(t);
            }
            menuArray = new Transform[menuList.Count];
            menuArray = menuList.ToArray();

            //Make sure only the menu selected in the editor is enabled at the start
            foreach(Transform t in menuArray)
            {
                t.gameObject.SetActive(false);
            }
            menuArray[(int)currentlyEnabledMenu].gameObject.SetActive(true);
            menuOn = true;
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controllerID"></param>
        /// <param name="button"></param>
        void ToggleMenu(byte controllerID, MLInputControllerButton button)
        {
            if (button == MLInputControllerButton.HomeTap)
            {
                if (menuOn)
                {
                    for(int i = 0; i<menuArray.Length; i++)
                    {
                        if (menuArray[i].gameObject.activeSelf)
                            menuArray[i].gameObject.SetActive(false);
                    }
                    menuOn = false;
                }
                else
                {
                    menuArray[(int)currentlyEnabledMenu].gameObject.SetActive(true);
                    menuOn = true;
                }
            }
        }

        public void EnableMenu(MolGeoMenus enableMenu)
        {
            gameObject.SetActive(!gameObject.activeSelf);
            menuArray[(int)currentlyEnabledMenu].gameObject.SetActive(false);
            menuArray[(int)enableMenu].gameObject.SetActive(true);
            currentlyEnabledMenu = enableMenu;
        }

        public void SetScore(int numCorrect, int numIncorrect)
        {
            float percent = (float)numCorrect / (float)(numCorrect + numIncorrect);
            string scores = "{numCorrect}{System.Environment.NewLine}{numIncorrect}{System.Environment.NewLine}{percent}";
            menuArray[(int)MolGeoMenus.Finish].GetComponentInChildren<Text>().text = scores;
        }
    }
}