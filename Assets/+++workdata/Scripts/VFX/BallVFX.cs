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
    Color ballCol;
    Coroutine spriteRoutine;
    #endregion

    void Awake()
    {
        ballSR = GetComponent<SpriteRenderer>();
        originalSprite = ballSR.sprite;
        ballCol = ballSR.color;
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
        ballSR.color = Color.white;
        if (spriteRoutine != null)
            StopCoroutine(spriteRoutine);

        spriteRoutine = StartCoroutine(ResetBallSprite(seconds));
    }

    IEnumerator ResetBallSprite(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ballSR.color = ballCol;
        ballSR.sprite = originalSprite;
    }
}