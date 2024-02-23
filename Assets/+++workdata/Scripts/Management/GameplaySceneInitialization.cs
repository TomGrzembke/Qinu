using UnityEngine;

public class GameplaySceneInitialization : MonoBehaviour
{
    #region serialized fields

    #endregion

    #region private fields

    #endregion

    void Awake()
    {
        PauseManager.Instance.PauseLogic(false);
        MusicManager.Instance.PlaySong(MusicManager.Songs.Ingame);
    }
}