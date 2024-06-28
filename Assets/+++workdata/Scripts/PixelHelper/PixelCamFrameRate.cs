using UnityEngine;

/// <summary> Renders the camera in a given frame rate </summary>
[RequireComponent (typeof(Camera))]
public class PixelCamFrameRate : MonoBehaviour
{
    #region Serialized
    [SerializeField] int frameRate = 12;
    #endregion

    #region Non Serialized
    Camera cam;
    float lastRenderTime = 0;
    #endregion

    void Awake() => cam = GetComponent<Camera>();

    void Start() => cam.enabled = false;

    void Update()
    {
        if (Time.time - lastRenderTime > 1f / frameRate)
        {
            lastRenderTime = Time.time;
            cam.Render();
        }
    }
}