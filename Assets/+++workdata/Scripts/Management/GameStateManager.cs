using System.Collections;
using MyBox;
using UnityEngine;

/// <summary> Used for calling management methods from buttons or in scene besides the Manager scene</summary>
public class GameStateManager : MonoBehaviour
{
    [SerializeField] SceneReference introScene;

    public static GameStateManager Instance { get; private set; }

    Coroutine resetRoutine;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Cursor.visible = false;
    }

    public void StartGame()
    {
        StartCoroutine(LoadScenesCoroutine((int)Scenes.MainMenu, GetSceneID(introScene)));
    }

    public void ResetGame()
    {
        if (resetRoutine != null)
        {
            return;
        }

        resetRoutine = StartCoroutine(RestartGameCoroutine());
    }

    public void GoToMainMenu()
    {
        StartCoroutine(ToMainMenuCor());
    }

    /// <summary> Depends on the naming (0_Scene)</summary>
    int GetSceneID(SceneReference sceneRef)
    {
        return sceneRef.GetSceneIndex();
    }

    public void ShowLoadScreen(bool condition)
    {
        if (LoadingScreen.Instance == null) return;

        if (condition)
        {
            LoadingScreen.Instance.Show(this);
            return;
        }

        LoadingScreen.Instance.Hide(this);
    }

    IEnumerator LoadScenesCoroutine(int oldScene, int newScene)
    {
        yield return null;
        ShowLoadScreen(true);
        yield return SceneLoader.Instance.LoadSceneViaIndex(newScene);
        yield return SceneLoader.Instance.UnloadSceneViaIndex(oldScene);
        ShowLoadScreen(false);
    }

    IEnumerator RestartGameCoroutine()
    {
        ShowLoadScreen(true);
        yield return ResetCor();
        ShowLoadScreen(false);
        resetRoutine = null;
    }

    IEnumerator ResetCor()
    {
        yield return SceneLoader.Instance.UnloadSceneViaIndex((int)Scenes.Gameplay);
        yield return SceneLoader.Instance.UnloadSceneViaIndex((int)Scenes.MainMenu);
        yield return SceneLoader.Instance.UnloadSceneViaIndex((int)Scenes.End);
        yield return SceneLoader.Instance.UnloadSceneViaIndex((int)Scenes.Manager);

        yield return SceneLoader.Instance.LoadSceneViaIndex((int)Scenes.Startup);

        Destroy(gameObject);
    }

    IEnumerator ToMainMenuCor()
    {
        yield return SceneLoader.Instance.UnloadSceneViaIndex((int)Scenes.Gameplay);
        yield return SceneLoader.Instance.UnloadSceneViaIndex((int)Scenes.End);
        yield return SceneLoader.Instance.LoadSceneViaIndex((int)Scenes.MainMenu);
    }
}