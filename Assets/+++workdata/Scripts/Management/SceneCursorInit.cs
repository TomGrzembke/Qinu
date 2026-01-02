using System;
using UnityEngine;

enum SceneCursorInitState
{
    CursorUsable = 0,
    CurorLocked = 10
}

/// <summary> Tool for easy setting of start Cursor states in scenes</summary>
public class SceneCursorInit : MonoBehaviour
{
    [SerializeField] SceneCursorInitState state;

    void Start()
    {
        switch (state)
        {
            case SceneCursorInitState.CursorUsable:
                InputManager.Instance.InitMainMenu();
                break;
            case SceneCursorInitState.CurorLocked:
                InputManager.Instance.InitGameplayScene();

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}