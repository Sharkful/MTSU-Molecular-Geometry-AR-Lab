using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.MagicLeap;

[CustomEditor(typeof(MoleculeControllerConfig))]
public class MolceuleControllerConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MoleculeControllerConfig configFile = (MoleculeControllerConfig)target;

        EditorGUILayout.LabelField("Interaction Controls",EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Nothing in this Section Does anything Currently", MessageType.Info);
        configFile.targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", configFile.targetMaterial, typeof(Material));
        configFile.selectMaterial = (Material)EditorGUILayout.ObjectField("Select Material", configFile.selectMaterial, typeof(Material));
        configFile.TargetColor = EditorGUILayout.ColorField(new GUIContent("Target Color"), configFile.TargetColor, true, true, false);
        configFile.SelectColor = EditorGUILayout.ColorField(new GUIContent("Select Color"), configFile.SelectColor, true, true, false);
        configFile.outlineThickness = EditorGUILayout.Slider("Outline Thickness", configFile.outlineThickness, 0, 0.3f);

        EditorGUILayout.LabelField("Movement Control", EditorStyles.boldLabel);
        configFile.smoothTime = EditorGUILayout.Slider("Smooth Time", configFile.smoothTime, 0.001f, 1);
        configFile.rotationSpeed = (int)EditorGUILayout.Slider("Rotation Speed", configFile.rotationSpeed, 0, 360);
        configFile.depthSpeed = EditorGUILayout.Slider("Depth Speed", configFile.depthSpeed, 0.1f, 3);

        EditorGUILayout.LabelField("Scaled Depth Control", EditorStyles.boldLabel);
        configFile.MinDepth = EditorGUILayout.Slider("Minimum Depth", configFile.MinDepth, 0f, 10f);
        configFile.MaxDepth = EditorGUILayout.Slider("Maximum Depth", configFile.MaxDepth, 0f, 10f);
        configFile.AverageDepthDragScalar = EditorGUILayout.Slider("Average Depth Scale", configFile.AverageDepthDragScalar, 1f, 5f);
        configFile.scalarSlope = EditorGUILayout.Slider("Scale Slope", configFile.scalarSlope, 0, configFile.maxSlope);

        EditorGUILayout.LabelField("Controller Feedback", EditorStyles.boldLabel);
        configFile.vibPattern = (MLInputControllerFeedbackPatternVibe)EditorGUILayout.EnumPopup("Controller Vibration Pattern", configFile.vibPattern);
        configFile.vibIntensity = (MLInputControllerFeedbackIntensity)EditorGUILayout.EnumPopup("Vibration Intensity", configFile.vibIntensity);
    }
}
