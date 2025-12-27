using System.Linq;
using UnityEngine;

/// <summary> Smoothes a value it gives to an animator, which is used interface liked (has to have specified string as input)</summary>
public class SpeedAnimInput : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Rigidbody2D rb;
    [SerializeField, Range(0.001f,1)] float smoothAlpha = .5f;

    private const string SpeedString = "speed";
    readonly int SpeedHash = Animator.StringToHash(SpeedString);
    
    CharSOHolder charSOHolder;
    CharSO charSO;
    float maxSpeed => charSO.CharSettings.CharRigidSettings.MaxSpeed;

    float smoothedSpeed;
    
    private bool hasSpeedParam;
    
    void Awake()
    {
        GetIfNotAssigned(ref anim);
        GetIfNotAssigned(ref rb);
        GetIfNotAssigned(ref charSOHolder);
        charSO = charSOHolder.CharSO;
        
        hasSpeedParam = anim.parameters.Any(p => p.name == SpeedString);
    }
    
    void GetIfNotAssigned<T>(ref T component) where T : Component
    {
        if (component != null) return;

        if (!TryGetComponent(out component))
        {
            Debug.LogError($"{name} has no {typeof(T).Name} provided or attached!", gameObject);
        }
    }

    void Update()
    {
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, rb.velocity.magnitude / maxSpeed, smoothAlpha);

        if (!hasSpeedParam) return;

        anim.SetFloat(SpeedHash, smoothedSpeed);
    }
}