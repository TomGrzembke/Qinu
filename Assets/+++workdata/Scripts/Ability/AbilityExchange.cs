using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AbilityExchange : MonoBehaviour
{
    public event Action<int> OnSlotSelected;
    PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new();
        inputActions.Player.Ability0.performed += ctx => Input(0);

        inputActions.Player.Ability1.performed += ctx => Input(1);

        inputActions.Player.Ability2.performed += ctx => Input(2);
    }

    void Input(int index)
    {
        if(IsBlocked()) return;

        OnSlotSelected?.Invoke(index);
    }

    public void OnEnable()
    {
        inputActions.Enable();
    }

    public void OnDisable()
    {
        inputActions.Disable();
    }

    bool IsBlocked()
    {
        PointerEventData eventDataCurrentPosition = new(EventSystem.current)
        {
            position = Mouse.current != null ? Mouse.current.position.ReadValue() : Touchscreen.current.primaryTouch.position.ReadValue()
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<Selectable>() == null) continue;

            return true;
        }

        return false;
    }
}

