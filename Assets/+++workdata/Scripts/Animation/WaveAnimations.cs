using UnityEngine;

public class WaveAnimations : MonoBehaviour
{
    #region serialized fields
    [SerializeField] float delayPerInstance = .02f;
    [SerializeField] float animSpeed;
    [SerializeField] Animator[] animatorsToWave;
    #endregion

    #region private fields

    #endregion

    void Start()
    {
        float a = 0;
        for (int i = 0; i < animatorsToWave.Length; i++)
        {
            animatorsToWave[i].speed = animSpeed;
            animatorsToWave[i].Play(animatorsToWave[i].GetCurrentAnimatorStateInfo(0).shortNameHash, 0, a);
            a += delayPerInstance;
        }
    }
}