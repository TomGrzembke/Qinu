using UnityEngine;

/// <summary> Assigns the main camera from another scene on runtime to  solve possible UI Bugs </summary>
public class AssignMainCamToCanvas : MonoBehaviour
{
    #region private fields
    Canvas canvas;
    #endregion

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
    }
}