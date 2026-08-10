using UnityEngine;

public class PostDashVFX : MonoBehaviour
{
    [SerializeField] DashController dashController;
    [Tooltip("Child object shown while the post-dash movement modifier is active.")]
    [SerializeField] ParticleSystem vfx;

    void Awake()
    {
        dashController ??= GetComponent<DashController>();
        SetVFXActive(false);
    }

    void OnEnable()
    {
        dashController.PostDashModifierStarted += ShowVFX;
        dashController.PostDashModifierFinished += HideVFX;
        SetVFXActive(dashController.IsPostDashModifierActive);
    }

    void OnDisable()
    {
        dashController.PostDashModifierStarted -= ShowVFX;
        dashController.PostDashModifierFinished -= HideVFX;
        SetVFXActive(false);
    }

    void ShowVFX()
    {
        SetVFXActive(true);
    }

    void HideVFX()
    {
        SetVFXActive(false);
    }

    void SetVFXActive(bool isActive)
    {
        if (vfx == null) return;
        
        if (isActive)
        {
            vfx.Play();
        }
        else
        {
            vfx.Stop();
        }

    }
}
