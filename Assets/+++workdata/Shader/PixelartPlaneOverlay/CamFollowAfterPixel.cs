using UnityEngine;

public class CamFollowAfterPixel : MonoBehaviour
{
    private Camera mainCam;
    private Transform mainCamTrans;
    [SerializeField] private Vector2 camFollowPixelDistance;
    [SerializeField] private Camera cam;

    [SerializeField] private PixelResSO pixelResSO;
    
    [SerializeField] private Vector2 macroPixelSize;
    
    [SerializeField] private Transform pixelPlane;

    [SerializeField] private Material pixelMat;

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
        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;
        var pixelCount = pixelMat.GetVector("_PixelCount");
        macroPixelSize.x = viewWidth / pixelCount.x;
        macroPixelSize.y = viewHeight / pixelCount.y;

        pixelPlane.localScale = new(viewWidth, viewHeight,
            pixelPlane.localScale.z);
    }

    private void AdjustPosition()
    {
        // var xDifference = mainCamTrans.position.x - transform.position.x;
        // var yDifference = mainCamTrans.position.y - transform.position.y;
        //
        // if (Mathf.Abs(xDifference) >= camFollowPixelDistance.x)
        // {
        //     transform.position = transform.position.ChangeX(mainCamTrans.position.x);
        //
        //     var xPixelDifference = transform.position.x % camFollowPixelDistance.x;
        //
        //     transform.position = transform.position.AddX(-xPixelDifference);
        // }
        //
        // if (Mathf.Abs(yDifference) >= camFollowPixelDistance.y)
        // {
        //     transform.position =
        //         transform.position.AddY(yDifference > 0 ? camFollowPixelDistance.y : -camFollowPixelDistance.y);
        // }
        
        Vector3 newPosition = mainCamTrans.position;
        
        newPosition.x = Mathf.Round(newPosition.x / camFollowPixelDistance.x) * camFollowPixelDistance.x;
        newPosition.y = Mathf.Round(newPosition.y / camFollowPixelDistance.y) * camFollowPixelDistance.y;
        
        transform.position = newPosition;
    }
}