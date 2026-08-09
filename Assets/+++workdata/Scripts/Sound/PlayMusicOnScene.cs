using UnityEngine;
using UnityEngine.Audio;

public class PlayMusicOnScene : MonoBehaviour
{
    [SerializeField] AudioResource musicToPlay;
    void Start()
    {
        SoundManager.Instance.PlayMusic(musicToPlay, 0.1f);
    }
}
