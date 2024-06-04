using MyBox;
using UnityEngine;

[CreateAssetMenu]
public class PixelResSO : ScriptableObject
{
    [Range(16, 1920)]
    public float pixelRes = 128;
    public Vector2 screenRes = new(1920, 1080);
    public Material pixelartMat;
    public Color col;
    public int lineSize = 1;
    RangedFloat screenRatio;
    Vector2 scaling;

    void Awake()
    {
        ValidateCall();
    }

    void OnValidate()
    {
        ValidateCall();
    }

    public void ValidateCall()
    {
        float greatestCommonFactor = CalculateGCF(screenRes.x, screenRes.y);

        screenRatio.Min = screenRes.x / greatestCommonFactor;
        screenRatio.Max = screenRes.y / greatestCommonFactor;

        scaling.x = pixelRes;
        scaling.y = pixelRes / screenRatio.Min * screenRatio.Max;

        pixelartMat.SetVector("_PixelCount", scaling);
        pixelartMat.SetColor("_OutlineColor", col);
        pixelartMat.SetInt("_OutlineThickness", lineSize);
    }

    public float CalculateGCF(float a, float b)
    {
        if (b == 0)
            return a;
      
        else 
            return CalculateGCF(b, a % b);
    }
}
