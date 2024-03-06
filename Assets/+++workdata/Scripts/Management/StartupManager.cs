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

        if (!SceneManager.GetSceneByBuildIndex((int)Scenes.Manager).IsValid())
            yield return SceneLoader.LoadScene(Scenes.Manager);

        if(skipMainMenu)
            yield return SceneLoader.LoadScene(Scenes.Gameplay);


        if (!SceneManager.GetSceneByBuildIndex((int)Scenes.Gameplay).IsValid())
            yield return SceneLoader.LoadScene(Scenes.MainMenu);
        else
            yield return SceneLoader.UnloadScene(Scenes.MainMenu);

        LoadingScreen.Hide(this);
    }

}