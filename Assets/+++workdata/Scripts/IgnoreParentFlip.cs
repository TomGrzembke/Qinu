using MyBox;
using UnityEngine;

public class IgnoreParentFlip : MonoBehaviour
{
    #region serialized fields

    #endregion

    #region private fields
    Transform parentTrans;
    bool xState; 
    #endregion

    void Awake()
    {
        parentTrans = transform.parent;
    }

    void Update()
    {
        if (xState != parentTrans.localScale.x > 0)
        {
            transform.localScale = transform.localScale.SetX(-transform.localScale.x);
            xState = parentTrans.localScale.x > 0;
        }
    }
}