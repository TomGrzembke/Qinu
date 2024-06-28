using UnityEngine;

public class CharNav : MonoBehaviour
{
    #region Serialized
    [field: SerializeField] public NavCalc NavCalc { get; private set; }
    [field: SerializeField] public NPCNav NPCNavMinigame { get; private set; }
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

    public void ChangeToArena()
    {
        NavCalc.enabled = false;
        NPCNavMinigame.enabled = true;
    }
}