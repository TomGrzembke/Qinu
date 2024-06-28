using UnityEngine;

public class AttachToGO : MonoBehaviour
{
    [SerializeField] Transform attachTo;

    void Update()
    {
        transform.position = attachTo.position;
    }
}