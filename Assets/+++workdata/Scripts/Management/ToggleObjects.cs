using System.Collections.Generic;
using UnityEngine;

/// <summary> Used for Toggling Objects like Buttons </summary>
public class ToggleObjects : MonoBehaviour
{
    [SerializeField] List<GameObject> toggleObjects;
    [SerializeField] bool defaultState;
    bool active;

    void Awake()
    {
        Toggle(defaultState);
    }

    public void Toggle()
    {
        Toggle(!active);
    }

    public void Toggle(bool state)
    {
        active = state;

        foreach (var obj in toggleObjects)
        {
            obj.SetActive(active);
        }
    }
}