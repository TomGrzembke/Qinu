using UnityEngine;

/// <summary> used for local acceleration ups </summary>
[RequireComponent (typeof(Collider2D))]
public class AddSpeedOnTrigger : MonoBehaviour
{
    #region Serialized
    [SerializeField] string triggerTag = "puk";
    [SerializeField] string secondTrigger = "NPC";
    [SerializeField] float strength = 5;
    [SerializeField] ForceMode2D forceMode;
    #endregion

    #region Non Serialized
    Rigidbody2D currentRb;
    #endregion

    void OnTriggerEnter2D(Collider2D collision)
    {
        currentRb = null;

        Vector2 calculatedForce = new(strength, transform.position.y - collision.transform.position.y);

        if (collision.CompareTag(triggerTag) || collision.CompareTag(secondTrigger))
            currentRb = collision.GetComponent<Rigidbody2D>();
        else
            collision.transform.parent.TryGetComponent(out currentRb);

        if (currentRb)
            currentRb.AddForce(calculatedForce, forceMode);
    }
}