using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public enum Scenes
{
    Startup,
    Manager,
    Pixelate,
    MainMenu,
    Gameplay,
    End
}

public class SceneLoader : MonoBehaviour
{
    static SceneLoader _instance;

    public static SceneLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("SceneLoader").AddComponent<SceneLoader>();
                DontDestroyOnLoad(_instance);
            }

            return _instance;
        }
    }

    public Coroutine LoadSceneViaIndex(Scenes scene, Action onLoadingFinished = null)
    {
        return LoadSceneViaIndex((int)scene, onLoadingFinished);
    }

    public Coroutine LoadSceneViaIndex(int index, Action onLoadingFinished = null)
    {
        return StartCoroutine(LoadSceneViaIndexCo(index, onLoadingFinished));
    }

    IEnumerator LoadSceneViaIndexCo(int index, Action onLoadingFinished)
    {

        LoadingScreen.Show(this);
        var scene = SceneManager.GetSceneByBuildIndex(index);
        if (scene.isLoaded)
        {
            onLoadingFinished?.Invoke();
            LoadingScreen.Hide(this);
            yield break;
        }

        yield return SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        LoadingScreen.Hide(this);
        onLoadingFinished?.Invoke();
    }
    public Coroutine UnloadSceneViaIndex(int index, Action onLoadingFinished = null)
    {
        return StartCoroutine(UnloadSceneViaIndexCo(index, onLoadingFinished));
    }
    public Coroutine UnloadSceneViaScene(Scene scene, Action onLoadingFinished = null)
    {
        return StartCoroutine(UnloadSceneViaCor(scene, onLoadingFinished));
    }
    IEnumerator UnloadSceneViaIndexCo(int index, Action onLoadingFinished = null)
    {
        var scene = SceneManager.GetSceneByBuildIndex(index);
        if (!scene.isLoaded)
        {
            onLoadingFinished?.Invoke();
            yield break;
        }

        yield return SceneManager.UnloadSceneAsync(index);
        onLoadingFinished?.Invoke();
    }
    IEnumerator UnloadSceneViaCor(Scene scene, Action onLoadingFinished = null)
    {
        if (!scene.isLoaded)
        {
            onLoadingFinished?.Invoke();
            yield break;
        }

        yield return SceneManager.UnloadSceneAsync(scene);
        onLoadingFinished?.Invoke();
    }

    public static Coroutine LoadScene(Scenes scenes, Action onLoadingFinished = null)
    {
        return Instance.LoadSceneViaIndex(scenes, onLoadingFinished);
    }

    public static Coroutine UnloadScene(Scenes scenes, Action onLoadingFinished = null)
    {
        return Instance.UnloadSceneViaIndex((int)scenes, onLoadingFinished);
    }
    public static Coroutine UnloadScene(Scene scene, Action onLoadingFinished = null)
    {
        return Instance.UnloadSceneViaScene(scene, onLoadingFinished);
    }

#if UNITY_EDITOR
    [MenuItem("ThisGame/Load Startup Scene")]
    static void LoadStartupScene()
    {
        var scene = EditorBuildSettings.scenes[(int)Scenes.Startup];
        EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Additive);
    }
#endif

}