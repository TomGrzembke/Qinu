using UnityEngine;

/// <summary> Initializes static scripts </summary>
public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance;

    [SerializeField] LoadingScreen loadingScreen;
    public LoadingScreen LoadingScreen => loadingScreen;

    void Awake()
    {
        Instance = this;
        loadingScreen.Initialize();
    }
}