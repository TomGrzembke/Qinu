using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary> Loads scenes in the first scene, also used for quicker scene debugging and automatic unloading </summary>
public class StartupManager : MonoBehaviour
{
    [SerializeField] Scenes prefferedEditorScene = Scenes.MainMenu;

    IEnumerator Start()
    {
        yield return SceneLoader.LoadScene(Scenes.Manager);
        yield return SceneLoader.LoadScene(Scenes.Pixelate);
        yield return null;

#if UNITY_EDITOR
        var wantedScene = GetPrefferedStartingScene();
        yield return SceneLoader.LoadScene(wantedScene);
        yield return UnloadExtraScenes(wantedScene);
#else
        yield return SceneLoader.LoadScene(Scenes.MainMenu);
#endif

        SceneLoader.UnloadScene(Scenes.Startup);
    }

    Scenes GetPrefferedStartingScene()
    {
        List<Scenes> activeScenes = new();

        for (int i = (int)Scenes.MainMenu; i <= (int)Scenes.End; i++)
        {
            if (!SceneManager.GetSceneByBuildIndex(i).IsValid()) continue;

            activeScenes.Add(GetSceneEnum(i));
        }

        if (activeScenes.Count == 1) return activeScenes[0];
        if (activeScenes.Contains(prefferedEditorScene)) return prefferedEditorScene;

        if (activeScenes.Count > 1)
        {
            return activeScenes[0];
        }

        return prefferedEditorScene;
    }

    IEnumerator UnloadExtraScenes(Scenes wantedScene)
    {
        for (int i = (int)Scenes.MainMenu; i <= (int)Scenes.End; i++)
        {
            if (wantedScene == GetSceneEnum(i)) continue;
            if (!SceneManager.GetSceneByBuildIndex(i).IsValid()) continue;

            yield return SceneLoader.UnloadScene(GetSceneEnum(i));
        }
    }

    public Scenes GetSceneEnum(int index)
    {
        return (Scenes)index;
    }
}