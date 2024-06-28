using MyBox;
using UnityEngine;

public class IgnoreParentFlip : MonoBehaviour
{
    #region Non Serialized
    Transform parentTrans;
    bool xState;
    #endregion

    void Awake()
    {
        parentTrans = transform.parent;
    }

    void Update()
    {
        if (!parentTrans)
            parentTrans = transform.parent;

        if (xState != parentTrans.localScale.x > 0)
        {
            transform.localScale = transform.localScale.SetX(-transform.localScale.x);
            xState = parentTrans.localScale.x > 0;
        }
    }
}