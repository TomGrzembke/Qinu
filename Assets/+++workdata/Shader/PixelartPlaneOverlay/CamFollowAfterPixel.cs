using MyBox;
using UnityEngine;

/// <summary> Allows for pixelcamsnap settings to achieve pixelate recalculation as wished, CamChangeEditorListener utilizes this for settings updates</summary>
public class CamFollowAfterPixel : MonoBehaviour
{
    private Camera mainCam;
    private Transform mainCamTrans;
    [SerializeField] private Camera cam;

    [SerializeField] private PixelResSO pixelResSO;

    [SerializeField] private Transform pixelPlane;

    [SerializeField] private Material pixelMat;

    [SerializeField] private float macroPixelMultiplier = 1;
    [SerializeField, ShowOnly] private Vector2 camFollowPixelDistance;
    [SerializeField, ShowOnly] private Vector2 macroPixelSize;

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

    [ButtonMethod]
    public void CalculateMarginPixelValues()
    {
        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;
        var pixelCount = pixelMat.GetVector("_PixelCount");
        macroPixelSize.x = viewWidth / pixelCount.x;
        macroPixelSize.y = viewHeight / pixelCount.y;

        camFollowPixelDistance.x = macroPixelSize.x * macroPixelMultiplier;
        camFollowPixelDistance.y = macroPixelSize.y * macroPixelMultiplier / 2;


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