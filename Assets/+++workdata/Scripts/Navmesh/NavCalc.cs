using UnityEngine;
using UnityEngine.AI;

public class NavCalc : MonoBehaviour
{
    public Transform DespawnPos => MinigameManager.Instance.DespawnPos;
    protected NavMeshAgent agent;
    protected CharSOHolder sOHolder;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        sOHolder = GetComponent<CharSOHolder>();
    }

    public void SetAgentPosition(Vector3 targetPos)
    {
        if (agent.isOnNavMesh)
            agent.SetDestination(targetPos);
    }

    public void SetAgentPosition(Transform targetTrans)
    {
        SetAgentPosition(targetTrans.position);
    }

    void OnDrawGizmosSelected()
    {
    }
}
