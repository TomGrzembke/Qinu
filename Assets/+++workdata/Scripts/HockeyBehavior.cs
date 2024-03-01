using UnityEngine;

public class HockeyBehavior : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Collider2D middleObj;

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
        Physics2D.IgnoreCollision(col, middleObj);
    }
}