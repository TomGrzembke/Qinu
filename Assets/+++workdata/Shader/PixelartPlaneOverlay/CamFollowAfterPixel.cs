using UnityEngine;

/// <summary> Allows for pixelcamsnap settings to achieve pixelate recalculation as wished, CamChangeEditorListener utilizes this for settings updates</summary>
public class CamFollowAfterPixel : MonoBehaviour
{
    Camera mainCam;
    Transform mainCamTrans;
    [SerializeField] Camera cam;

    [SerializeField] RT_PixelResSO pixelResSO;

    [SerializeField] Transform pixelPlane;

    [SerializeField] Material pixelMat;

    [Tooltip("Higher means it will wait more macro pixels until it moves along"), SerializeField]
    float macroPixelFollowMultiplier = 1;

    [SerializeField, ShowOnly] Vector2 camFollowPixelDistance;
    [SerializeField, ShowOnly] Vector2 macroPixelSize;
    [SerializeField, ShowOnly] Vector2 pixelCount;

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
        CalculateMarginPixelValues();
    }
    
    public void CalculateMarginPixelValues()
    {
        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;
        pixelCount = pixelResSO.GetPixelCount();
        macroPixelSize.x = viewWidth / pixelCount.x;
        macroPixelSize.y = viewHeight / pixelCount.y;

        camFollowPixelDistance.x = macroPixelSize.x * macroPixelFollowMultiplier / 2;
        camFollowPixelDistance.y = macroPixelSize.y * macroPixelFollowMultiplier;


        pixelPlane.localScale = new(viewWidth, viewHeight,
            pixelPlane.localScale.z);
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