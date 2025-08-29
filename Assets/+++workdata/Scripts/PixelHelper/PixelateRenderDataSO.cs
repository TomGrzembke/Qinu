using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PixelateLayerSettings
{
    public string name;
    public Color outlineColor = Color.white;
    public int outlineSize = 1;

    [Range(16, 1920)] public float pixelRes = 128;

    public int camOrthoSize = 10;

    [Tooltip(
        "Multiplies the size of one macro pixel (how big a 'pixel' of the pixelation effect is displayed' and snaps the cam in that value")]
    public int macroPixelFollowMultiplier = 10;

    public RT_PixelResSO rTPixelResSO;
}

/// <summary> Can calculate and set values for a pixelart material and enhances it's usability (depending on screen size) </summary>
[CreateAssetMenu]
public class PixelateRenderDataSO : ScriptableObject
{
    #region Serialized

    public List<PixelateLayerSettings> pixelateLayerSettings;

    #endregion

    #region Non Serialized

    #endregion

    void Awake() => ValidateCall();

    void OnValidate() => ValidateCall();

    public void ValidateCall()
    {
        foreach (var setting in pixelateLayerSettings)
        {
            var pixelResCalculator = setting.rTPixelResSO;
            pixelResCalculator.SetPixelationParameter(setting.pixelRes, setting.outlineColor, setting.outlineSize);
        }
    }
}