using UnityEngine;

public class EmitProblemsOnLoose : MonoBehaviour
{
    #region serialized fields
    [SerializeField] ParticleSystem problemPS;
    [SerializeField] float negativeMultiplier = 7;
    #endregion

    #region private fields
    ParticleSystem.EmissionModule emissionModule;
    #endregion

    void Awake()
    {
        emissionModule = problemPS.emission;
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
        emissionModule.rateOverTime = -(amount - TournamentManager.Instance.RoundsTilWin);
        problemPS.Play();
    }
}