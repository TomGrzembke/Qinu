using UnityEngine;

public abstract class RBGetter : MonoBehaviour
{
    protected Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        AwakeInternal();
    }

    protected abstract void AwakeInternal();
}
