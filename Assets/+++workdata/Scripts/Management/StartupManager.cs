using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupManager : MonoBehaviour
{
    [SerializeField] bool skipMainMenu;
    IEnumerator Start()
    {
        yield return null;
        LoadingScreen.Show(this);

        yield return SceneLoader.LoadScene(Scenes.Manager);
        yield return SceneLoader.LoadScene(Scenes.Pixelate);

        if (skipMainMenu)
            yield return SceneLoader.LoadScene(Scenes.Gameplay);

        if (!SceneManager.GetSceneByBuildIndex((int)Scenes.Gameplay).IsValid())
            yield return SceneLoader.LoadScene(Scenes.MainMenu);
        else
            yield return SceneLoader.UnloadScene(Scenes.MainMenu);

        LoadingScreen.Hide(this);
    }

}