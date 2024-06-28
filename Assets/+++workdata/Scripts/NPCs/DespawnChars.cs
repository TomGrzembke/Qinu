using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DespawnChars : MonoBehaviour
{
    #region serialized fields

    #endregion

    #region private fields

    #endregion

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("NPC"))
        {
            Destroy(collision.gameObject);
            CharManager.Instance.CleanList();
        }
    }
}