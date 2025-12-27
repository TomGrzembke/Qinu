using UnityEngine;

/// <summary> Smoothes a value it gives to an animator, which is used interface liked (has to have specified string as input)</summary>
public class SpeedAnimInput : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Rigidbody2D rb;
    [SerializeField, Range(0.001f,1)] float smoothAlpha = .5f;
    
    private const string SpeedString = "speed";
    CharSOHolder charSOHolder;
    CharSO charSO;
    float maxSpeed => charSO.CharSettings.CharRigidSettings.MaxSpeed;

    float smoothedSpeed;
    
    void Awake()
    {
        GetIfNotAssigned(ref anim);
        GetIfNotAssigned(ref rb);
        GetIfNotAssigned(ref charSOHolder);
        charSO = charSOHolder.CharSO;
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
        anim.SetFloat(SpeedString, smoothedSpeed);
    }
}