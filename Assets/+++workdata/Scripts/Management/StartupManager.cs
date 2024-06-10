using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupManager : MonoBehaviour
{
    [SerializeField] Scenes sceneToLoad = Scenes.MainMenu;
    IEnumerator Start()
    {
        yield return null;
        LoadingScreen.Show(this);

        yield return SceneLoader.LoadScene(Scenes.Manager);
        yield return SceneLoader.LoadScene(Scenes.Pixelate);

        yield return SceneLoader.LoadScene(sceneToLoad);

        if (!SceneManager.GetSceneByBuildIndex((int)Scenes.Gameplay).IsValid() && 
            !SceneManager.GetSceneByBuildIndex((int)Scenes.Intro).IsValid())
            yield return SceneLoader.LoadScene(Scenes.MainMenu);
        else
            yield return SceneLoader.UnloadScene(Scenes.MainMenu);

        LoadingScreen.Hide(this);
        SceneLoader.UnloadScene(Scenes.Startup);
    }

}