using UnityEngine;

public class FitWorldCanvasToCamera : MonoBehaviour
{

    [SerializeField] bool matchHeight = true;

    float distanceFromCamera = 10f;
    RectTransform rectTransform;
    Vector2 referenceResolution = new(1920f, 1080f);
    Camera targetCamera;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        referenceResolution = new(rectTransform.rect.width, rectTransform.rect.height);

        targetCamera = Camera.main;
        distanceFromCamera = Vector3.Distance(targetCamera.transform.position, transform.position); 
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        var saveZ = transform.position.z;
        transform.position = targetCamera.transform.position + targetCamera.transform.forward * distanceFromCamera;
        transform.position = transform.position.ChangeZ(saveZ);

        transform.rotation = targetCamera.transform.rotation;

        float worldHeight;
        if (targetCamera.orthographic)
        {
            worldHeight = targetCamera.orthographicSize * 2f;
        }
        else
        {
            float halfFovRad = targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            worldHeight = 2f * distanceFromCamera * Mathf.Tan(halfFovRad);
        }

        float worldWidth = worldHeight * targetCamera.aspect;

        float scale = matchHeight
            ? worldHeight / referenceResolution.y
            : worldWidth / referenceResolution.x;

        float logicalWidth = worldWidth / scale;
        float logicalHeight = worldHeight / scale;

        transform.localScale = Vector3.one * scale;
        rectTransform.sizeDelta = new Vector2(logicalWidth, logicalHeight);
    }
}
