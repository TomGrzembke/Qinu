using UnityEngine;

public class PixelCamFrameRate : MonoBehaviour
{
    Camera cam;
    float lastRenderTime = 0;
    [SerializeField] int frameRate = 12;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        cam.enabled = false;
    }

    void Update()
    {
        if (Time.time - lastRenderTime > 1f / frameRate)
        {
            lastRenderTime = Time.time;
            cam.Render();
        }
    }
}