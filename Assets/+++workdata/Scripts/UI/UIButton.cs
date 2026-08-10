using MyBox;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary> Attach this to the text component of a button to match its naming and get some basic scale animations </summary>
public class UIButton : MonoBehaviour
{
    [SerializeField] string buttonName;
    [SerializeField] bool additionalSettings;
    [SerializeField, ConditionalField(nameof(additionalSettings))] float scaleOnClick = .75f;
    [SerializeField, ConditionalField(nameof(additionalSettings))] float scaleTime = .075f;
    [SerializeField, ConditionalField(nameof(additionalSettings))] float scaleHover = .95f;
    [SerializeField, Min(0f), Tooltip("How long the cursor must remain outside before hover visuals are removed. Higher values prevent flickering near the edge of a button.")] float hoverSwitchCooldown = .08f;
    [SerializeField] SoundType onClickSFX = SoundType.ButtonClick;

    const string BUTTON_NAME_SYNTAX = "[Button]";
    const string TEXT_NAME_SYNTAX = "[Text]";
    TextMeshProUGUI textComponent;
    Coroutine scaleRoutine;
    Coroutine hoverSwitchRoutine;
    BoolLock hoveredBoolLock = new();
    bool pointerHovered;

    void OnValidate() => OnValidateCall();

    void Awake() => OnValidateCall();

    void OnDisable()
    {
        hoverSwitchRoutine = null;
        pointerHovered = false;
        hoveredBoolLock.RemoveInstigator(this);
        RefreshHoverScale();
    }

    private void OnValidateCall()
    {
        name = BUTTON_NAME_SYNTAX + " " + buttonName;
        textComponent = GetComponentInChildren<TextMeshProUGUI>();

        if (textComponent == null) return;

        textComponent.text = buttonName;
        textComponent.name = TEXT_NAME_SYNTAX + " " + buttonName;
    }

    public void ClickedAnim()
    {
        transform.localScale = Vector3.one;

        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
        }

        scaleRoutine = StartCoroutine(ScaleAnim());

        SoundManager.Instance.PlaySound(onClickSFX);
    }

    IEnumerator ScaleAnim()
    {
        float scaledTime = 0;
        while (scaledTime < scaleTime)
        {
            scaledTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(scaleOnClick, scaleOnClick), scaledTime / scaleTime);
            yield return null;
        }

        scaledTime = 0;
        while (scaledTime < scaleTime)
        {
            scaledTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, scaledTime / scaleTime);
            yield return null;
        }
    }

    public void OnHover(bool condition)
    {
        pointerHovered = condition;

        if (hoverSwitchRoutine != null)
        {
            StopCoroutine(hoverSwitchRoutine);
            hoverSwitchRoutine = null;
        }

        if (condition)
        {
            hoveredBoolLock.AddInstigator(this);
            RefreshHoverScale();
            return;
        }

        hoverSwitchRoutine = StartCoroutine(RemoveHoverAfterCooldown());
    }

    IEnumerator RemoveHoverAfterCooldown()
    {
        yield return new WaitForSecondsRealtime(hoverSwitchCooldown);

        if (!pointerHovered)
        {
            hoveredBoolLock.RemoveInstigator(this);
            RefreshHoverScale();
        }

        hoverSwitchRoutine = null;
    }

    void RefreshHoverScale()
    {
        if (hoveredBoolLock.IsLocked)
        {
            transform.localScale = new Vector3(scaleHover, scaleHover);

        }
        else
        {
            transform.localScale = Vector3.one;

        }
    }

    public void SetPressed(GameObject requestor, bool condition)
    {
        if (condition)
        {
            hoveredBoolLock.AddInstigator(requestor);
        }
        else
        {
            hoveredBoolLock.RemoveInstigator(requestor);
        }

        RefreshHoverScale();
    }
}
