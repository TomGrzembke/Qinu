using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EmitParticlesOnPlayerLoose : MonoBehaviour
{
    [SerializeField] float negativeMultiplier = 7;

    ParticleSystem particleSys;
    ParticleSystem.EmissionModule emissionModule;

    void Awake()
    {
        particleSys = GetComponent<ParticleSystem>();
        emissionModule = particleSys.emission;

    }

    private void OnEnable()
    {
        TournamentManager.Instance.RegisterOnPlayerMatchEnd(Emit);
    }

    void OnDisable()
    {
        TournamentManager.Instance.OnPlayerMatchEnd -= Emit;
    }

    public void Emit(float amount)
    {
        if (amount < 0)
            amount *= negativeMultiplier;
        emissionModule.rateOverTime = -(amount - TournamentManager.Instance.WinPoints);
        particleSys.Play();
    }
}