using UnityEngine;

public class TunaPassiveAbility : Ability
{
    #region serialized fields

    #endregion

    #region private fields

    #endregion

    private void OnEnable()
    {
        try
        {
            //speedModifier = GetComponent<SpeedModifier>();
        }
        catch { }
    }
    protected override void ExecuteInternal()
    {
     
    }
    protected override void DeExecuteInternal()
    {

    }

    protected override void OnInitializedInternal()
    {
    }
}