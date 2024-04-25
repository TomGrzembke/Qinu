using UnityEngine;

public class CharNav : MonoBehaviour
{
    #region serialized fields
    [field: SerializeField] public NavCalc NavCalc { get; private set; }
    [field: SerializeField] public NPCNavMinigame NPCNavMinigame { get; private set; }
    #endregion

    #region private fields

    #endregion

    public void ActivateNavCalc()
    {
        NavCalc.enabled = true;
        NPCNavMinigame.enabled = false;
    }

    public void ChangeToArena(int sideID)
    {
        NavCalc.enabled = false;
        NPCNavMinigame.enabled = true;
        NPCNavMinigame.SideSettings(sideID != 0);
    }

    void OnEnable()
    {

    }
}