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

        for (int i = (int)Scenes.MainMenu; i < (int)Scenes.Gameplay + 1; i++)
        {
            if (sceneToLoad != GetSceneEnum(i) && SceneManager.GetSceneByBuildIndex(i).IsValid())
                yield return SceneLoader.UnloadScene(GetSceneEnum(i));
        }

        LoadingScreen.Hide(this);
        SceneLoader.UnloadScene(Scenes.Startup);
    }

    public Scenes GetSceneEnum(int index)
    {
        if (index == (int)Scenes.MainMenu)
            return Scenes.MainMenu;
        else if (index == (int)Scenes.Intro)
            return Scenes.Intro;
        else
            return Scenes.Gameplay;
    }

}