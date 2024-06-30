using System.Collections;
using UnityEngine;

public class BallVFX : MonoBehaviour
{
    #region Serialized
    [SerializeField] ParticleSystem TPVisualVFX;
    [SerializeField] ParticleSystem TPReachedVFX;
    #endregion

    #region Non Serialized
    SpriteRenderer ballSR;
    Sprite originalSprite;
    Coroutine spriteRoutine;
    #endregion

    void Awake()
    {
        ballSR = GetComponent<SpriteRenderer>();
        originalSprite = ballSR.sprite;
    }

    public void PlayTPVisual()
    {
        TPVisualVFX.Play();
    }

    public void PlayTPReachedVFX()
    {
        TPReachedVFX.Play();
    }

    public void ChangeSprite(Sprite sprite, float seconds)
    {
        ballSR.sprite = sprite;
        if (spriteRoutine != null)
            StopCoroutine(spriteRoutine);

        spriteRoutine = StartCoroutine(ResetBallSprite(seconds));
    }

    IEnumerator ResetBallSprite(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ballSR.sprite = originalSprite;
    }
}