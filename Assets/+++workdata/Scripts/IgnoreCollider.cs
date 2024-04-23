using UnityEngine;

public class IgnoreCollider : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Collider2D targetCol;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] LayerMask layerToIgnore;
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
        if (targetCol != null)
            Physics2D.IgnoreCollision(col, targetCol);

        if (ignoreLayer.value != 0 && layerToIgnore.value != 0)
            Physics2D.IgnoreLayerCollision(ignoreLayer.GetLayerID(), layerToIgnore.GetLayerID());
    }
}