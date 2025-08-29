using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary> Allows for pixelcamsnap settings to achieve pixelate recalculation as wished, CamChangeEditorListener utilizes this for settings updates</summary>
public class CamFollowAfterPixel : MonoBehaviour
{
    [Header("Data Set")]
#pragma warning disable
    [SerializeField, ShowOnly]
    private string info =
        $"Utilizes {nameof(PixelateDataSO)} with {nameof(RT_PixelateCalculatorSO)} as context, as data settings";
#pragma warning restore

    [SerializeField] private PixelateDataSO pixelateDataSO;

    [FormerlySerializedAs("pixelResSO")] [SerializeField]
    RT_PixelateCalculatorSO pixelateCalculatorSo;

    [Header("Scene Related")] [SerializeField]
    Camera cam;

    [SerializeField] Transform pixelPlane;
    [SerializeField] Material pixelMat;

    [Header("Debug")] [SerializeField, ShowOnly]
    Vector2 camFollowPixelDistance;

    [SerializeField, ShowOnly] Vector2 macroPixelSize;
    [SerializeField, ShowOnly] Vector2 pixelCount;

    [Tooltip("Higher means it will wait more macro pixels until it moves along")]
    float macroPixelFollowMultiplier = 1;

    Camera mainCam;
    Transform mainCamTrans;

    void Awake()
    {
        mainCam = Camera.main;
        mainCamTrans = mainCam.transform;
    }

    void Update()
    {
        AdjustPosition();
    }

    private void OnValidate()
    {
        SubscribeDataChangeValidate();

        CalculateMarginPixelValues();
    }

    private void SubscribeDataChangeValidate()
    {
        if (pixelateDataSO == null) return;

        pixelateDataSO.onChanged -= CalculateMarginPixelValues;
        pixelateDataSO.onChanged += CalculateMarginPixelValues;
    }

    public void CalculateMarginPixelValues()
    {
        ExtractData();

        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;

        macroPixelSize.x = viewWidth / pixelCount.x;
        macroPixelSize.y = viewHeight / pixelCount.y;

        camFollowPixelDistance.x = macroPixelSize.x * macroPixelFollowMultiplier / 2;
        camFollowPixelDistance.y = macroPixelSize.y * macroPixelFollowMultiplier;

        pixelPlane.localScale = new(viewWidth, viewHeight, pixelPlane.localScale.z);
    }

    private void ExtractData()
    {
        if (pixelateDataSO == null) return;

        var layerSettings = pixelateDataSO.GetPixelateLayerSettings(pixelateCalculatorSo);
        macroPixelFollowMultiplier = layerSettings.macroPixelFollowMultiplier;
        cam.orthographicSize = layerSettings.camOrthoSize;
        pixelCount = pixelateCalculatorSo.GetPixelCount();
    }

    public void HandleCameraChange()
    {
        CalculateMarginPixelValues();
    }

    private void AdjustPosition()
    {
        Vector3 newPosition = mainCamTrans.position;


        newPosition.x = Mathf.Round(newPosition.x / camFollowPixelDistance.x) * camFollowPixelDistance.x;
        newPosition.y = Mathf.Round(newPosition.y / camFollowPixelDistance.y) * camFollowPixelDistance.y;

        transform.position = newPosition;
    }
}