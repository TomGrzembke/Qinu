using System.Collections;
using UnityEngine;

public class EndLoader : MonoBehaviour
{
    #region serialized fields

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
        
    }
}