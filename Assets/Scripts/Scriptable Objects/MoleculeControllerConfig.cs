using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

[CreateAssetMenu]
public class MoleculeControllerConfig : ScriptableObject
{
    public Material targetMaterial;
    public Material selectMaterial;
    private Color targetColor = Color.white;
    public Color TargetColor
    {
        get => targetColor;
        set
        {
            targetColor = value;
            if(targetMaterial != null)
                targetMaterial.color = value;
        }
    }
    private Color selectColor = Color.yellow;
    public Color SelectColor
    {
        get => selectColor;
        set
        {
            selectColor = value;
            if (selectMaterial != null)
                selectMaterial.color = value;
        }
    }
    public float outlineThickness = 0.007f;

    public float smoothTime = 0.3f;
    public int rotationSpeed = 90;
    public float depthSpeed = 1;

    [HideInInspector]
    public float maxSlope;
    private float minDepth = 0.3f;
    public float MinDepth
    {
        get => minDepth;
        set
        {
            minDepth = value;
            maxSlope = (averageDepthDragScalar - 1) / (((minDepth + maxDepth) / 2) - minDepth);
        }
    }
    private float maxDepth = 3f;
    public float MaxDepth
    {
        get => maxDepth;
        set
        {
            maxDepth = value;
            maxSlope = (averageDepthDragScalar - 1) / (((minDepth + maxDepth) / 2) - minDepth);
        }
    }
    private float averageDepthDragScalar = 2;
    public float AverageDepthDragScalar
    {
        get
        {
            return averageDepthDragScalar;
        }
        set
        {
            averageDepthDragScalar = value;
            maxSlope = (averageDepthDragScalar - 1) / (((minDepth + maxDepth) / 2) - minDepth);
        }
    }
    [HideInInspector]
    public float scalarSlope;

    public MLInputControllerFeedbackPatternVibe vibPattern;
    public MLInputControllerFeedbackIntensity vibIntensity;

    public float GetScalar(float depth)
    {
        return (scalarSlope * (depth - (maxDepth + minDepth) / 2) + averageDepthDragScalar);
    }
}
