using UnityEngine;

public class GoalTracker : MonoBehaviour
{
    #region serialized fields
    [SerializeField] HockeyController hockeyController;
    [SerializeField] bool leftSide;
    [SerializeField] ParticleSystem goalParticles;
    #endregion

    #region private fields

    #endregion

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Puk"))
        {
            hockeyController.Goal(leftSide);
            goalParticles.Play();
        }
    }
}