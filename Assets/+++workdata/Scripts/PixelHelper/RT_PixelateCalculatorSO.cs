using MyBox;
using UnityEngine;

/// <summary> Can calculate and set values for a pixelart material and enhances it's usability (depending on screen size) </summary>
[CreateAssetMenu]
public class RT_PixelateCalculatorSO : ScriptableObject
{
    #region Serialized

    public Material pixelartMat;

    public RenderTexture pixelateTexture;

    [SerializeField, ShowOnly] Vector2 pixelCount;

    #endregion

    #region Non Serialized

    float pixelRes = 128;
    Color col;
    int lineSize = 1;

    RangedFloat assetRatio;

    #endregion

    void Awake() => ValidateCall();

    void OnValidate() => ValidateCall();

    public void ValidateCall()
    {
        var planesRes = new Vector2(pixelateTexture.width, pixelateTexture.height);

        float greatestCommonFactor = CalculateGCF(planesRes.x, planesRes.y);

        assetRatio.Min = planesRes.x / greatestCommonFactor;
        assetRatio.Max = planesRes.y / greatestCommonFactor;

        pixelCount.x = pixelRes;
        pixelCount.y = pixelRes / assetRatio.Min * assetRatio.Max;

        pixelartMat.SetVector("_PixelCount", pixelCount);
        pixelartMat.SetColor("_OutlineColor", col);
        pixelartMat.SetInt("_OutlineThickness", lineSize);
    }

    /// <summary> GCF = Greatest common factor </summary>
    public float CalculateGCF(float a, float b)
    {
        if (b == 0)
            return a;

        else
            return CalculateGCF(b, a % b);
    }

    public Vector2 GetPixelCount()
    {
        return pixelCount;
    }

    public void SetPixelationParameter(float pixelRes, Color col, int lineSize)
    {
        this.pixelRes = pixelRes;
        this.col = col;
        this.lineSize = lineSize;
        ValidateCall();
    }
}