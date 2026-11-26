using UnityEngine;
using UnityEngine.UI;

public class ExhibitModeDelegator : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    [SerializeField] ExhibitSettingsSO settingsSO;

    void Start()
    {
        toggle.isOn = settingsSO.ExhibitMode;
        toggle.onValueChanged.AddListener(ModeToggle);
    }

    void ModeToggle(bool condition)
    {
        settingsSO.SetExhibitMode(condition);
    }
}
