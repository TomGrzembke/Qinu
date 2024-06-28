using UnityEngine;
using UnityEngine.AI;

public class NavCalc : MonoBehaviour
{
    #region Non Serialized
    public Transform DespawnPos => MinigameManager.Instance.DespawnPos;
    protected NavMeshAgent agent;
    protected CharSOHolder sOHolder;
    #endregion

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        sOHolder = GetComponent<CharSOHolder>();

    }

    public void SetAgentPosition(Vector3 targetPos)
    {
        if (Vector3.Distance(transform.position, targetPos) > sOHolder.CharSO.CharSettings.CharRigidSettings.StoppingDistance)
            agent.SetDestination(targetPos);

        agent.velocity = Vector2.zero;
    }

    public void SetAgentPosition(Transform targetTrans)
    {
        SetAgentPosition(targetTrans.position);
    }

    void OnDrawGizmosSelected()
    {
    }
}