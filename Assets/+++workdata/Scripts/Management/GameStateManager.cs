using System.Collections;
using MyBox;
using UnityEditor.SearchService;
using UnityEngine;
using Scene = UnityEngine.SceneManagement.Scene;

/// <summary> Used for calling management methods from buttons or in scene besides the Manager scene</summary>
public class GameStateManager : MonoBehaviour
{
    [SerializeField] SceneReference introScene;

    static GameStateManager Instance;

    static Coroutine resetRoutine;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(Instance.gameObject);
        Cursor.visible = false;
    }

    public static void StartGame()
    {
        Instance.StartCoroutine(Instance.LoadScenesCoroutine((int)Scenes.MainMenu,
            Instance.GetSceneID(Instance.introScene)));
    }

    public static void ResetGame()
    {
        if (resetRoutine != null)
        {
            return;
        }

        resetRoutine = Instance.StartCoroutine(Instance.RestartGameCoroutine());
    }

    public static void GoToMainMenu()
    {
        Instance.StartCoroutine(Instance.ToMainMenuCor());
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