using UnityEngine;

public class AbilityInput : MonoBehaviour
{
    #region Serialized
    [SerializeField] AbilitySlotManager abilitySlotManager;
    #endregion

    #region Non Serialized
    PlayerInputActions inputActions;
    #endregion

    void Awake()
    {
        inputActions = new();
        inputActions.Player.Ability0.performed += ctx => Input(0, true);
        inputActions.Player.Ability0.canceled += ctx => Input(0, false);

        inputActions.Player.Ability1.performed += ctx => Input(1, true);
        inputActions.Player.Ability1.canceled += ctx => Input(1, false);

        inputActions.Player.Ability2.performed += ctx => Input(2, true);
        inputActions.Player.Ability2.canceled += ctx => Input(2, false);

        inputActions.Player.Ability3.performed += ctx => Input(3, true);
        inputActions.Player.Ability3.canceled += ctx => Input(3, false);
    }

    void Input(int index, bool performed)
    {
        abilitySlotManager.ActivateSlot(index, performed);
    }

    #region OnEnable/Disable
    public void OnEnable()
    {
        inputActions.Enable();
    }

    public void OnDisable()
    {
        inputActions.Disable();
    }
    #endregion
}