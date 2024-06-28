using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DespawnChars : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("NPC")) return;

        Destroy(collision.gameObject);
        CharManager.Instance.CleanList();

    }
}