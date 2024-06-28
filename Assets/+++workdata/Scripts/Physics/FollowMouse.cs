using UnityEngine;
using UnityEngine.InputSystem;

public class FollowMouse : MonoBehaviour
{
    #region Serialized
    [SerializeField] Camera cam;
    #endregion

    #region Non Serialized
    Transform trans;
    Vector3 camPos;
    #endregion

    void Awake()
    {
        trans = transform;
    }

    void Update()
    {
        camPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        trans.position = new (camPos.x, camPos.y, 0);
    }
}