using Cinemachine;
using UnityEngine;

public class GoalParticles : MonoBehaviour
{
    #region Serialized
    [SerializeField] MinigameManager minigameManager;
    [SerializeField] bool leftSide;
    [SerializeField] ParticleSystem goalParticles;
    [field: SerializeField] public ParticleSystem wonParticles { get; private set; }

    [SerializeField]  CinemachineImpulseSource impulseSource;
    #endregion

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Puk")) return;

        minigameManager.Goal(leftSide == true ? 0 : 1, this);
        goalParticles.Play();

        if(impulseSource == null) return;
        impulseSource.GenerateImpulseAt(impulseSource.transform.position, impulseSource.m_DefaultVelocity * Random.Range(0.2f, 2f));

    }

    public void WonParticle()
    {
        wonParticles.Play();
    }
}