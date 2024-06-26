using UnityEngine;
using UnityEngine.UI;

public class OnHoverChangeCol : MonoBehaviour
{
    #region serialized fields
    [SerializeField] Image[] objects;
    [SerializeField] Color toSetCol;
    #endregion

    #region private fields
    Color defaultCol;

    #endregion

    void Awake()
    {
        if (objects.Length > 0)
            defaultCol = objects[0].color;
    }

    public void OnHover(bool condition)
    {
        if (condition)
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i])
                    objects[i].color = toSetCol;
            }
        else
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i])
                    objects[i].color = defaultCol;
            }
    }
}