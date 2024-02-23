using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Bar : MonoBehaviour
{
    #region serialized fields
    [SerializeField] protected Image bar;
    #endregion
}