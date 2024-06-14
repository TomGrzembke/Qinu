
using UnityEngine;

public class DialogueSoundPlayer : MonoBehaviour
{
    [SerializeField] int soundWillPlayerEvery = 1;
    [SerializeField] bool randomspeakRange = true;
    int soundCounter;

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
