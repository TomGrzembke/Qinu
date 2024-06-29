using UnityEngine;

public class SFXOnCollision : MonoBehaviour
{
    #region Serialized
    [SerializeField] SoundType sfxToPlay;
    #endregion

    #region Non Serialized

    #endregion

    public void PlaySound()
    {
        SoundManager.Instance.PlaySound(sfxToPlay);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;
        PlaySound();
    }
}