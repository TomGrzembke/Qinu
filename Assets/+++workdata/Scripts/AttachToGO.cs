using UnityEngine;

public class AttachToGO : MonoBehaviour
{
    [SerializeField] Transform attachTo;

    #region private fields
   
    #endregion

    void Update()
    {
        transform.position = attachTo.position;
    }
}