using System.Collections;
using UnityEngine;

public class EndLoader : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Scenes sceneToLoad = Scenes.End;
    #endregion

    #region private fields

    #endregion

    IEnumerator Start()
    {
        yield return null;
        TournamentManager.Instance.RegisterOnPlayerMatchEnd(OnValueChanged, true);
    }

    void OnDisable()
    {
        TournamentManager.Instance.OnPlayerMatchEnd -= OnValueChanged;
    }

    void OnValueChanged(float value)
    {
        if (value == TournamentManager.Instance.RoundsTilWin)
        {
            SceneLoader.LoadScene(Scenes.End);
            SceneLoader.UnloadScene(Scenes.Gameplay);
        }
    }
}