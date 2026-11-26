using UnityEngine;
using UnityEngine.InputSystem;

public class InactivityReset : MonoBehaviour
{
    [SerializeField] ExhibitSettingsSO exhibitSettingsSO;
    [SerializeField] GameObject warningObject;

    float inactiveTime;
    bool resetTriggered;
    InputAction anyInputAction;

    void OnEnable()
    {
        RegisterInput();

        anyInputAction = new(binding: "/*");
        anyInputAction.Enable();
    }

    void OnDisable()
    {
        anyInputAction.Disable();
        anyInputAction.Dispose();
    }

    void Update()
    {
        if (!exhibitSettingsSO.ExhibitMode) return;

        if (resetTriggered) return;

        if (HasInput())
        {
            RegisterInput();
            return;
        }

        inactiveTime += Time.unscaledDeltaTime;

        if (warningObject && !warningObject.activeSelf && inactiveTime >= exhibitSettingsSO.TimeUntilWarning)
        {
            warningObject.SetActive(true);
        }

        if (inactiveTime < exhibitSettingsSO.TimeUntilReset) return;
        if (!GameStateManager.Instance) return;

        resetTriggered = true;
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        GameStateManager.Instance.ResetGame();
    }

    bool HasInput() => anyInputAction.IsPressed();

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
