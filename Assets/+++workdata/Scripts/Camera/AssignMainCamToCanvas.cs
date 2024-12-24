using System.Collections;
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

    IEnumerator Start()
    {
        canvas.worldCamera = Camera.main;
        yield return new WaitForSeconds(1);
        canvas.worldCamera = Camera.main;
    }
}