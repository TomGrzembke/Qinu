using MyBox;
using System.Collections;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    static GameStateManager Instance;

    [SerializeField] SceneReference gamePlayScene;
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(Instance.gameObject);
    }


    public static void StartGame()
    {
        Instance.StartCoroutine(Instance.LoadScenesCoroutine((int)Scenes.MainMenu, Instance.GetSceneID(Instance.gamePlayScene)));
    }

    public static void OptionsWindow()
    {
        PauseManager.Instance.PauseLogic();
    }
    public static void CloseOptionsWindow()
    {
        PauseManager.Instance.PauseLogic();
    }

    public static void GoToMainMenu()
    {
        Instance.StartCoroutine(Instance.LoadScenesCoroutine(Instance.GetSceneID(Instance.gamePlayScene), (int)Scenes.MainMenu));
    }

    public static void ReloadGameScene()
    {
        Instance.StartCoroutine(Instance.ReloadGameSceneCoroutine());
    }

    /// <summary> Depends on the naming (0_Scene)</summary>
    int GetSceneID(SceneReference sceneRef)
    {
        return sceneRef.GetSceneIndex();
    }

    IEnumerator LoadScenesCoroutine(int oldScene, int newScene)
    {
        yield return null;
        LoadingScreen.Show(this);
        yield return SceneLoader.Instance.LoadSceneViaIndex(newScene);
        yield return SceneLoader.Instance.UnloadSceneViaIndex(oldScene);
        LoadingScreen.Hide(this);
    }

    IEnumerator ReloadGameSceneCoroutine()
    {
        LoadingScreen.Show(this);
        yield return new WaitForSeconds(.3f);
        yield return SceneLoader.Instance.UnloadSceneViaIndex(GetSceneID(Instance.gamePlayScene));
        yield return SceneLoader.Instance.LoadSceneViaIndex(GetSceneID(Instance.gamePlayScene));
        LoadingScreen.Hide(this);
    }
}
