using MyBox;
using UnityEngine;

public class FollowGameObjectInCam : MonoBehaviour
{
    #region serialized fields
    [SerializeField] bool useTopAndBotTarget = false;
    [SerializeField] Transform topTarget;
    [SerializeField, ConditionalField(nameof(useTopAndBotTarget))] Transform botTarget;
    [SerializeField] AnimationCurve followCurve;
    [SerializeField] float sensitivity = .9f;
    [SerializeField] float yMargin;
    #endregion

    #region private fields
    Transform target;
    Camera cam;
    float width;
    float height;
    #endregion
    void Start()
    {
        target = topTarget;
        cam = Camera.main;
        width = Screen.width;
        height = Screen.height;
    }
    void Update()
    {
        Vector2 bottomLeft = cam.ScreenToWorldPoint(Vector2.zero);
        Vector2 topRight = cam.ScreenToWorldPoint(new(width, height)).Add(0, -yMargin);

        target = topTarget;
        if (useTopAndBotTarget)
            if (topTarget.position.RemoveZ().IsBetween(bottomLeft, topRight))
                target = topTarget;
            else
                if (botTarget)
                target = botTarget;


        transform.position =
            Vector3.Lerp(transform.position, target.position, followCurve.Evaluate(Vector3.Distance(transform.position, target.position) * sensitivity));
    }

    void OnDrawGizmos()
    {

    }

    public void SetTargets(Transform _topTarget, Transform _botTarget = null)
    {
        if (_topTarget != null)
            topTarget = _topTarget;

        if (_botTarget != null)
            botTarget = _botTarget;
    }
}