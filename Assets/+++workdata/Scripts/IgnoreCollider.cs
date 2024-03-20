using UnityEngine;

public class IgnoreCollider : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Collider2D targetCol;

    #endregion

    #region private fields
    Collider2D col;
    #endregion

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }
    void Start()
    {
        Physics2D.IgnoreCollision(col, targetCol);
    }
}