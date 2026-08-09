using UnityEngine;
using UnityEngine.InputSystem;

public class InactivityReset : MonoBehaviour
{
    [SerializeField, Min(0f)] float timeUntilWarning = 60f;
    [SerializeField, Min(0f)] float timeUntilReset = 75f;
    [SerializeField] GameObject warningObject;

    float inactiveTime;
    bool resetTriggered;

    void OnEnable()
    {
        RegisterInput();
    }

    void Update()
    {
        if (resetTriggered) return;

        if (HasInput())
        {
            RegisterInput();
            return;
        }

        inactiveTime += Time.unscaledDeltaTime;

        if (warningObject && !warningObject.activeSelf && inactiveTime >= timeUntilWarning)
        {
            warningObject.SetActive(true);
        }

        if (inactiveTime < timeUntilReset) return;
        if (!GameStateManager.Instance) return;

        resetTriggered = true;
        GameStateManager.Instance.ResetGame();
    }

    void OnValidate()
    {
        timeUntilWarning = Mathf.Max(0f, timeUntilWarning);
        timeUntilReset = Mathf.Max(timeUntilWarning, timeUntilReset);
    }

    bool HasInput()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.isPressed) return true;

        if (Mouse.current != null)
        {
            bool mouseButtonPressed = Mouse.current.leftButton.isPressed
                || Mouse.current.rightButton.isPressed
                || Mouse.current.middleButton.isPressed;
            bool mouseMoved = Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f;
            bool mouseScrolled = Mouse.current.scroll.ReadValue().sqrMagnitude > 0.01f;

            if (mouseButtonPressed || mouseMoved || mouseScrolled) return true;
        }

        foreach (Gamepad gamepad in Gamepad.all)
        {
            bool stickMoved = gamepad.leftStick.ReadValue().sqrMagnitude > 0.04f
                || gamepad.rightStick.ReadValue().sqrMagnitude > 0.04f;
            bool triggerPressed = gamepad.leftTrigger.ReadValue() > 0.2f
                || gamepad.rightTrigger.ReadValue() > 0.2f;
            bool buttonPressed = gamepad.buttonSouth.isPressed
                || gamepad.buttonNorth.isPressed
                || gamepad.buttonEast.isPressed
                || gamepad.buttonWest.isPressed
                || gamepad.leftShoulder.isPressed
                || gamepad.rightShoulder.isPressed
                || gamepad.leftStickButton.isPressed
                || gamepad.rightStickButton.isPressed
                || gamepad.startButton.isPressed
                || gamepad.selectButton.isPressed
                || gamepad.dpad.IsActuated();

            if (stickMoved || triggerPressed || buttonPressed) return true;
        }

        return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
    }

    void RegisterInput()
    {
        inactiveTime = 0f;
        resetTriggered = false;

        if (warningObject)
        {
            warningObject.SetActive(false);
        }
    }
}
