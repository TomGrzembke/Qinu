using UnityEngine;

public class EmitOnCollision : MonoBehaviour
{
    #region Serialized
    [SerializeField] ParticleSystem pSystem;
    #endregion

    void OnCollisionEnter2D(Collision2D collision)
    {
        pSystem.Play();
    }
}