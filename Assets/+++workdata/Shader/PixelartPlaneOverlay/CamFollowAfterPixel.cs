using UnityEngine;

public class CamFollowAfterPixel : MonoBehaviour
{
    private Camera mainCam;
    private Transform mainCamTrans;
    [SerializeField] private Vector2 camFollowPixelDistance;

    [SerializeField] private PixelResSO pixelResSO;

    void Awake()
    {
        mainCam = Camera.main;
        mainCamTrans = mainCam.transform;
    }

    void Update()
    {
        var xDifference = mainCamTrans.position.x - transform.position.x;
        var yDifference = mainCamTrans.position.y - transform.position.y;

        if (Mathf.Abs(xDifference) >= camFollowPixelDistance.x)
        {
            var newX = xDifference > 0 ? camFollowPixelDistance.x : -camFollowPixelDistance.x;

            transform.position = transform.position.AddX(newX);

            var xPixelDifference = transform.position.x % camFollowPixelDistance.x;

            
            if (xPixelDifference < camFollowPixelDistance.x) //Tried to force grid here
            {
                //transform.position = transform.position.AddX(-xPixelDifference);
            }
            else
            {
                //transform.position = transform.position.AddX(camFollowPixelDistance.x - xPixelDifference);
            }
        }

        if (Mathf.Abs(yDifference) >= camFollowPixelDistance.y)
        {
            transform.position =
                transform.position.AddY(yDifference > 0 ? camFollowPixelDistance.y : -camFollowPixelDistance.y);
        }
    }
}