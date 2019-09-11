using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MtsuMLAR;

[CustomEditor(typeof(MtsuMLAR.MLInputModuleV2))]
public class MLInputModuleV2Editor : Editor
{
    public override void OnInspectorGUI()
    {
        MLInputModuleV2 inputModule = (MLInputModuleV2)target;
        inputModule.CurrentInputMethod = (MLInputModuleV2.InputMethod)EditorGUILayout.EnumPopup("Current Input Method", inputModule.CurrentInputMethod);
        if (inputModule.CurrentInputMethod == MLInputModuleV2.InputMethod.ProximitySphere)
            inputModule.ProximitySphereRadius = EditorGUILayout.FloatField("Radius of Proximity Sphere", inputModule.ProximitySphereRadius);
        inputModule.CurrentInputTool = (MLInputModuleV2.InputTool)EditorGUILayout.EnumPopup("Current Input Tool", inputModule.CurrentInputTool);
        switch(inputModule.CurrentInputTool)
        {
            case MLInputModuleV2.InputTool.Controller:
                {
                    inputModule.controllerObject = (GameObject)EditorGUILayout.ObjectField("Controller Object", inputModule.controllerObject, typeof(GameObject), true);
                    break;
                }
            case MLInputModuleV2.InputTool.LeftHand:
                {
                    inputModule.leftHandObject = (GameObject)EditorGUILayout.ObjectField("Left Hand GameObject", inputModule.leftHandObject, typeof(GameObject), true);
                    break;
                }
            case MLInputModuleV2.InputTool.RightHand:
                {
                    inputModule.rightHandObject = (GameObject)EditorGUILayout.ObjectField("Right Hand GameObject", inputModule.rightHandObject, typeof(GameObject), true);
                    break;
                }
        }
    }
}
