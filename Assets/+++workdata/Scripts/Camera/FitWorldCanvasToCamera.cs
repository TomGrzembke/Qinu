using UnityEngine;

/// <summary> Scales A canvas to the current resolution, handy when you want to use post processing on UI which causes it to not scale on itself </summary>
public class FitWorldCanvasToCamera : MonoBehaviour
{
    [SerializeField] bool matchHeight = true;

    float distanceFromCamera = 10f;
    RectTransform rectTransform;
    Vector2 referenceResolution = new(1920f, 1080f);
    Camera targetCamera;
    float lastAspect = -1f;
    float lastOrthographicSize = -1f;
    float lastFieldOfView = -1f;
    bool lastWasOrthographic;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetCamera = Camera.main;
    }

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        distanceFromCamera = Vector3.Distance(targetCamera.transform.position, transform.position);
        referenceResolution = new(rectTransform.rect.width, rectTransform.rect.height);
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        var saveZ = transform.position.z;
        transform.position = targetCamera.transform.position + targetCamera.transform.forward * distanceFromCamera;
        transform.position = transform.position.ChangeZ(saveZ);

        transform.rotation = targetCamera.transform.rotation;

        bool projectionChanged = lastWasOrthographic != targetCamera.orthographic;
        bool aspectChanged = !Mathf.Approximately(lastAspect, targetCamera.aspect);
        bool orthographicSizeChanged = targetCamera.orthographic && !Mathf.Approximately(lastOrthographicSize, targetCamera.orthographicSize);
        bool fieldOfViewChanged = !targetCamera.orthographic && !Mathf.Approximately(lastFieldOfView, targetCamera.fieldOfView);
        bool viewChanged = projectionChanged || aspectChanged || orthographicSizeChanged || fieldOfViewChanged;

        if (!viewChanged) return;

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

        lastWasOrthographic = targetCamera.orthographic;
        lastAspect = targetCamera.aspect;
        lastOrthographicSize = targetCamera.orthographicSize;
        lastFieldOfView = targetCamera.fieldOfView;
    }
}
