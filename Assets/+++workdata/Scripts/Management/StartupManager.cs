using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> Loads scenes as a script in the first scene, also used for quicker scene debugging and automatic unloading </summary>
public class StartupManager : MonoBehaviour
{
    [SerializeField] Scenes sceneToLoad = Scenes.MainMenu;

    IEnumerator Start()
    {
        yield return SceneLoader.LoadScene(Scenes.Manager);
        yield return SceneLoader.LoadScene(Scenes.Pixelate);

#if UNITY_EDITOR
        yield return SceneLoader.LoadScene(sceneToLoad);
        yield return UnloadExtraScenes();
#else
        yield return SceneLoader.LoadScene(Scenes.MainMenu);
#endif

        SceneLoader.UnloadScene(Scenes.Startup);
    }

    IEnumerator UnloadExtraScenes()
    {
        for (int i = (int)Scenes.MainMenu; i < (int)Scenes.Gameplay + 1; i++)
        {
            if (sceneToLoad == GetSceneEnum(i)) continue;
            if (!SceneManager.GetSceneByBuildIndex(i).IsValid()) continue;
            
            yield return SceneLoader.UnloadScene(GetSceneEnum(i));
        }
    }

    public Scenes GetSceneEnum(int index)
    {
        return (Scenes)index;
    }
}