using UnityEngine;

public class LockScaleFlipCollider : MonoBehaviour
{
    [SerializeField] Transform flipParentTransform;

    float startScaleX;
    int previousFlipSign;

    void Awake()
    {
        startScaleX = Mathf.Abs(transform.localScale.x);
        UpdateScale();
    }

    void LateUpdate()
    {
        UpdateScale();
    }

    void UpdateScale()
    {
        int flipSign = flipParentTransform.localScale.x < 0f ? -1 : 1;

        if (flipSign == previousFlipSign) return;

        previousFlipSign = flipSign;
        Vector3 currentScale = transform.localScale;
        currentScale.x = startScaleX * flipSign;
        transform.localScale = currentScale;
    }
}