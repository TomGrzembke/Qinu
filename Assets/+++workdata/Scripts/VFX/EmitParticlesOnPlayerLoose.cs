using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EmitParticlesOnPlayerLoose : MonoBehaviour
{
    #region Serialized
    [SerializeField] float negativeMultiplier = 7;
    #endregion

    #region Non Serialized
    ParticleSystem particleSys;
    ParticleSystem.EmissionModule emissionModule;
    #endregion

    void Awake()
    {
        particleSys = GetComponent<ParticleSystem>();
        emissionModule = particleSys.emission;
    }

    void OnEnable()
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