using UnityEngine;
using UnityEngine.InputSystem;

public class FollowMouse : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Camera cam;
    #endregion

    #region private fields
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
        trans.position = new Vector3(camPos.x, camPos.y, 0);
    }
}