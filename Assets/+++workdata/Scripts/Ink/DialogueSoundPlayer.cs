using UnityEngine;


public class DialogueSoundPlayer : MonoBehaviour
{
    #region Serialized
    [SerializeField] int soundWillPlayerEvery = 1;
    [SerializeField] bool randomspeakRange = true;
    #endregion

    #region Non Serialized
    int soundCounter;
    #endregion

    public void PlaySound(SoundType soundType)
    {
        soundCounter++;
        if (soundCounter >= soundWillPlayerEvery)
        {
            soundCounter = 0;

            if (randomspeakRange)
                soundWillPlayerEvery = Random.Range(1, soundWillPlayerEvery += 2);

            SoundManager.Instance.PlaySound(soundType);
        }
    }
}
