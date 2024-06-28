using UnityEngine;

public class GoalParticles : MonoBehaviour
{
    #region Serialized
    [SerializeField] MinigameManager minigameManager;
    [SerializeField] bool leftSide;
    [SerializeField] ParticleSystem goalParticles;
    [field: SerializeField] public ParticleSystem wonParticles { get; private set; }
    #endregion

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Puk")) return;

        minigameManager.Goal(leftSide == false ? 0 : 1, this);
        goalParticles.Play();

    }

    public void WonParticle()
    {
        wonParticles.Play();
    }
}