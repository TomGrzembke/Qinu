using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuitButton : MonoBehaviour
{
    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        Screen.fullScreen = false;
#else
        Application.Quit();
#endif
    }
}
