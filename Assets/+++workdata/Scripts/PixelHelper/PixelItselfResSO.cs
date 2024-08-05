using MyBox;
using UnityEngine;

/// <summary> Can calculate and set values for a pixelart material and enhances it's usability (depending on screen size) </summary>
[CreateAssetMenu]
public class PixelItselfResSO : ScriptableObject
{
    #region Serialized
    [Range(16, 1920)]
    public float pixelRes = 128;
    public Material pixelartMat;
    public Color col;
    public int lineSize = 1;
    #endregion

    #region Non Serialized
    Vector2 scaling;
    #endregion

    void Awake() => ValidateCall();

    void OnValidate() => ValidateCall();

    public void ValidateCall()
    {
        scaling.x = pixelRes;
        scaling.y = pixelRes;

        pixelartMat.SetVector("_PixelCount", scaling);
        pixelartMat.SetColor("_OutlineColor", col);
        pixelartMat.SetInt("_OutlineThickness", lineSize);
    }

}
