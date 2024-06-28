using UnityEngine;

public class AssignMainCamToCanvas : MonoBehaviour
{
    Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
    }
}