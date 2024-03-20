using UnityEngine;

public class AttachToGO : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Transform attachTo;
    #endregion

    #region private fields

    #endregion
    void Update()
    {
        transform.position = attachTo.position;
    }
}