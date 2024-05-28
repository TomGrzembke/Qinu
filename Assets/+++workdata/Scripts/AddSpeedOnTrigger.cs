using UnityEngine;

public class AddSpeedOnTrigger : MonoBehaviour
{
    #region serialized fields
    [SerializeField] string triggerTag = "puk";
    [SerializeField] string secondTrigger = "NPC";
    [SerializeField] float strength = 5;
    [SerializeField] ForceMode2D forceMode;
    #endregion

    #region private fields

    #endregion

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(triggerTag))
            collision.GetComponent<Rigidbody2D>().AddForce(new(strength, transform.position.y - collision.transform.position.y), forceMode);

        if (collision.CompareTag(secondTrigger))
            if (collision.GetComponent<Rigidbody2D>())
                collision.GetComponent<Rigidbody2D>().AddForce(new(strength, transform.position.y - collision.transform.position.y), forceMode);
            else if (collision.transform.parent.GetComponent<Rigidbody2D>())
                collision.transform.parent.GetComponent<Rigidbody2D>().AddForce(new(strength, transform.position.y - collision.transform.position.y), forceMode);

    }
}