using UnityEngine;

public class GoalTracker : MonoBehaviour
{
    #region serialized fields
    [SerializeField] MinigameManager minigameManager;
    [SerializeField] bool leftSide;
    [SerializeField] ParticleSystem goalParticles;
    [field: SerializeField] public ParticleSystem wonParticles { get; private set; } 
    #endregion

    #region private fields

    #endregion

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Puk"))
        {
            minigameManager.Goal(leftSide == false ? 0 : 1, this);
            goalParticles.Play();
        }
    }

    public void WonParticle()
    {
        wonParticles.Play();
    }
}