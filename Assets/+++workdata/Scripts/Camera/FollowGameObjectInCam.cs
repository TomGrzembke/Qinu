using MyBox;
using UnityEngine;

/// <summary>Moves an object between camera-relative targets and can keep dialogue away from its speaker.</summary>
public class FollowGameObjectInCam : MonoBehaviour
{
    #region Serialized
    [Header("Basic Follow")]
    [SerializeField] bool useTopAndBotTarget;
    [SerializeField] Transform topTarget;
    [SerializeField, ConditionalField(nameof(useTopAndBotTarget))] Transform botTarget;
    [SerializeField] AnimationCurve followCurve;
    [SerializeField] float sensitivity = .9f;
    [SerializeField] float yMargin;

    [Header("Dialogue Placement")]
    [SerializeField, Range(0f, 1f)] float minimumSpeakerClearance = .18f;
    [SerializeField, Range(0f, .5f)] float oppositeSideFallbackImprovement = .08f;
    [SerializeField, Range(0f, .5f)] float anchorSwitchAdvantage = .04f;
    #endregion

    #region Non Serialized
    Transform target;
    Transform speaker;
    Transform topLeft;
    Transform topRight;
    Transform botLeft;
    Transform botRight;
    Camera cam;
    bool useDialoguePlacement;
    bool speakerIsRight;
    #endregion

    void Start()
    {
        target = topTarget;
        cam = Camera.main;
    }

    void Update()
    {
        if (!cam)
            cam = Camera.main;

        if (useDialoguePlacement)
            UpdateDialogueTarget();
        else
            UpdateBasicTarget();

        if (!target) return;

        float distance = Vector3.Distance(transform.position, target.position);
        float followAmount = followCurve.Evaluate(distance * sensitivity);
        transform.position = Vector3.Lerp(transform.position, target.position, followAmount);
    }

    void UpdateBasicTarget()
    {
        target = topTarget;
        if (!useTopAndBotTarget || !botTarget || !cam || !topTarget) return;

        Vector2 bottomLeft = cam.ScreenToWorldPoint(Vector2.zero);
        Vector2 topRightScreen = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)).Add(0, -yMargin);
        target = topTarget.position.RemoveZ().IsBetween(bottomLeft, topRightScreen) ? topTarget : botTarget;
    }

    void UpdateDialogueTarget()
    {
        if (!speaker || !cam) return;

        Transform preferredTop = speakerIsRight ? topRight : topLeft;
        Transform preferredBottom = speakerIsRight ? botRight : botLeft;
        Transform fallbackTop = speakerIsRight ? topLeft : topRight;
        Transform fallbackBottom = speakerIsRight ? botLeft : botRight;

        Transform preferred = GetFartherAnchorFromSpeaker(preferredTop, preferredBottom);
        Transform fallback = GetFartherAnchorFromSpeaker(fallbackTop, fallbackBottom);
        Transform bestTarget = preferred;

        float preferredClearance = GetViewportDistanceFromSpeaker(preferred);
        float fallbackClearance = GetViewportDistanceFromSpeaker(fallback);
        bool preferredOverlapsSpeaker = preferredClearance < minimumSpeakerClearance;
        bool fallbackIsMeaningfullySafer = fallbackClearance >= preferredClearance + oppositeSideFallbackImprovement;

        if (preferredOverlapsSpeaker && fallbackIsMeaningfullySafer)
            bestTarget = fallback;

        if (!target)
        {
            target = bestTarget;
            return;
        }

        float currentClearance = GetViewportDistanceFromSpeaker(target);
        float bestClearance = GetViewportDistanceFromSpeaker(bestTarget);
        bool currentTargetIsTooClose = currentClearance < minimumSpeakerClearance;
        bool newTargetIsClearlyBetter = bestClearance >= currentClearance + anchorSwitchAdvantage;
        bool canReturnToPreferredSide = bestTarget == preferred
            && target != preferred
            && !preferredOverlapsSpeaker;

        if (target != bestTarget
            && (currentTargetIsTooClose || newTargetIsClearlyBetter || canReturnToPreferredSide))
            target = bestTarget;
    }

    Transform GetFartherAnchorFromSpeaker(Transform first, Transform second)
    {
        if (!first) return second;
        if (!second) return first;

        return GetViewportDistanceFromSpeaker(first) >= GetViewportDistanceFromSpeaker(second)
            ? first
            : second;
    }

    float GetViewportDistanceFromSpeaker(Transform anchor)
    {
        if (!anchor || !speaker || !cam) return 0f;

        Vector2 speakerPosition = cam.WorldToViewportPoint(speaker.position);
        Vector2 anchorPosition = cam.WorldToViewportPoint(anchor.position);
        return Vector2.Distance(speakerPosition, anchorPosition);
    }

    /// <summary>Configures the four dialogue anchors and the character the dialogue belongs to.</summary>
    public void SetDialoguePlacement(
        Transform dialogueSpeaker,
        bool isRightSide,
        Transform leftTop,
        Transform rightTop,
        Transform leftBottom,
        Transform rightBottom)
    {
        speaker = dialogueSpeaker;
        speakerIsRight = isRightSide;
        topLeft = leftTop;
        topRight = rightTop;
        botLeft = leftBottom;
        botRight = rightBottom;
        useDialoguePlacement = speaker && (topLeft || topRight || botLeft || botRight);

        if (useDialoguePlacement)
        {
            target = null;
            UpdateDialogueTarget();
        }
    }

    public void SetTargets(Transform _topTarget, Transform _botTarget = null)
    {
        useDialoguePlacement = false;

        if (_topTarget)
            topTarget = _topTarget;

        if (_botTarget)
            botTarget = _botTarget;
    }
}
