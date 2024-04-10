using UnityEngine;

public class FollowGameObjectInCam : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Transform target;
    [SerializeField] AnimationCurve followCurve;
    [SerializeField] float sensitivity = .9f;
    #endregion

    #region private fields
    Camera cam;
    #endregion
    void Start()
    {
        cam = Camera.main;
    }
    void Update()
    {
        if (!target.position.RemoveZ().IsBetween(cam.rect.min, cam.rect.max))
        {
            print("a");
        }
        transform.position =
            Vector3.Lerp(transform.position, target.position, followCurve.Evaluate(Vector3.Distance(transform.position, target.position) * sensitivity));
    }
}