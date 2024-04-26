using UnityEngine;

public class CharNav : MonoBehaviour
{
    #region serialized fields
    [field: SerializeField] public NavCalc NavCalc { get; private set; }
    [field: SerializeField] public NPCNavMinigame NPCNavMinigame { get; private set; }
    #endregion

    #region private fields

    #endregion

    public void ActivateNavCalc(Transform targetPos = null)
    {
        if (NavCalc != null)
            NavCalc.enabled = true;
        if (NPCNavMinigame != null)
            NPCNavMinigame.enabled = false;

        if(targetPos != null)
            NavCalc.SetAgentPosition(targetPos); 
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