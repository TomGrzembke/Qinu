using UnityEngine;
using UnityEngine.AI;

public class NavCalc : MonoBehaviour
{
    #region serialized fields
    [field: SerializeField] public Transform homePos { get; private set; } 
    #endregion

    #region private fields
    protected NavMeshAgent agent;
    #endregion

    void Start()
    {

    }
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

    }

    public void SetAgentPosition(Vector3 targetPos)
    {
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