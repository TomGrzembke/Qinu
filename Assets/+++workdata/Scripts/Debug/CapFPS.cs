using UnityEngine;

public class CapFPS : MonoBehaviour
{
    [SerializeField] private int fpsLimit = 120;

    void RefreshFrameCap()
    {
        Application.targetFrameRate = fpsLimit;
    }

    private void Start()
    {
        RefreshFrameCap();
    }

    private void OnValidate()
    {
        RefreshFrameCap();
    }
}