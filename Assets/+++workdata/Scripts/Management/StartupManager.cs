using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupManager : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;
        LoadingScreen.Show(this);

        if (!SceneManager.GetSceneByBuildIndex((int)Scenes.Manager).IsValid())
            yield return SceneLoader.LoadScene(Scenes.Manager);

        if (!SceneManager.GetSceneByBuildIndex((int)Scenes.Gameplay).IsValid())
            yield return SceneLoader.LoadScene(Scenes.MainMenu);
        else
            yield return SceneLoader.UnloadScene(Scenes.MainMenu);

        LoadingScreen.Hide(this);
    }

}