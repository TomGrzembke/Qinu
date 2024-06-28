using MyBox;
using System.Collections;
using UnityEngine;

/// <summary> Used for calling management methods from buttons or in scene besides the Manager scene</summary>
public class GameStateManager : MonoBehaviour
{
    #region Serialized
    [SerializeField] SceneReference introScene;
    #endregion

    #region Non Serialized
    static GameStateManager Instance;
    #endregion

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(Instance.gameObject);
        Cursor.visible = false;
    }

    public static void StartGame()
    {
        Instance.StartCoroutine(Instance.LoadScenesCoroutine((int)Scenes.MainMenu, Instance.GetSceneID(Instance.introScene)));
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
        Instance.StartCoroutine(Instance.LoadScenesCoroutine(Instance.GetSceneID(Instance.introScene), (int)Scenes.MainMenu));
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
        yield return SceneLoader.Instance.UnloadSceneViaIndex(GetSceneID(Instance.introScene));
        yield return SceneLoader.Instance.LoadSceneViaIndex(GetSceneID(Instance.introScene));
        LoadingScreen.Hide(this);
    }
}
