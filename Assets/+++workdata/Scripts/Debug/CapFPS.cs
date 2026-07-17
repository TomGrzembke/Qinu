using UnityEngine;

public class CapFPS : MonoBehaviour
{
    [SerializeField] int fpsLimit = 120;

    void RefreshFrameCap()
    {
        Application.targetFrameRate = fpsLimit;
    }

    void Start()
    {
        RefreshFrameCap();
    }

    void OnValidate()
    {
        RefreshFrameCap();
    }
}