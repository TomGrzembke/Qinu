using UnityEngine;

public class EmitOnCollision : MonoBehaviour
{
    #region serialized fields
    [SerializeField] ParticleSystem pSystem;
    #endregion

    void OnCollisionEnter2D(Collision2D collision)
    {
        pSystem.Play();
    }
}