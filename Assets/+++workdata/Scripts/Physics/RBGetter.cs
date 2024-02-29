using UnityEngine;

public class RBGetter : MonoBehaviour
{
    protected Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

}
