using UnityEngine;
using UnityEngine.UI;

public class ChangePixelScaleMultiplier : MonoBehaviour
{
    [SerializeField] RT_PixelateCalculatorSO[] pixelCalculators;

    [SerializeField] Slider slider;

    [SerializeField] float valueOnMaxValue = 3.5f;
    [SerializeField] bool resetToDefaultOnDestroy = true;

    void Start()
    {
        slider.onValueChanged.RemoveListener(SetPixelScaleMultiplier);
        slider.onValueChanged.AddListener(SetPixelScaleMultiplier);

    }

    public void SetPixelScaleMultiplier(float value)
    {
        if(Mathf.Approximately(slider.maxValue, value))
        {
            value = valueOnMaxValue;
        }

        foreach (var entry in pixelCalculators)
        {
            entry.SetPixelScaleMultiplier(value);
        }
    }

    void OnDestroy()
    {
        if(!resetToDefaultOnDestroy) return;
        
        SetPixelScaleMultiplier(1);
    }
}
