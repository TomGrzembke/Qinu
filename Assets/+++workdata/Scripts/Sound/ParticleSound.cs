using UnityEngine;

/// <summary> Emits sounds depending on a ParticleSystem</summary>
public class ParticleSound : MonoBehaviour
{
    enum PlayType
    {
        IsEmmiting,
        ParticleCount
    }

    #region Serialized
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] AudioSource audioSource;
    [SerializeField] SoundType soundType;
    [SerializeField] PlayType playType;
    [SerializeField] int frequencyDividedBy = 5;
    [SerializeField] bool randomizePitch;
    [SerializeField] float soundAmountMultiplier = 1;
    #endregion

    #region Non Serialized
    int frequencyCounter;
    float SoundMultiplier => -(-1 + (_particleSystem.particleCount * 2.5f + 1) / 100f);
    #endregion

    void Update()
    {
        frequencyCounter++;
        if (playType == PlayType.IsEmmiting)
            if (frequencyCounter < frequencyDividedBy) return;

        if (playType == PlayType.ParticleCount)
            if (frequencyCounter < frequencyDividedBy + (soundAmountMultiplier * SoundMultiplier)) return;

        if (randomizePitch)
            audioSource.pitch = Random.Range(0.7f, 2);

        if (playType == PlayType.IsEmmiting)
            if (_particleSystem.isEmitting)
            {
                SoundManager.Instance.PlaySound(soundType, audioSource);
            }

        if (playType == PlayType.ParticleCount)
            if (_particleSystem.particleCount > 0)
            {
                SoundManager.Instance.PlaySound(soundType, audioSource);
            }

        frequencyCounter = 0;
    }

}