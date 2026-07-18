using UnityEngine;
using UnityEngine.UI;

public class OnHoverChangeCol : MonoBehaviour
{
    [SerializeField] Image[] objects;
    [SerializeField] Color toSetCol;

    Color defaultCol;

    void Awake()
    {
        if (objects.Length > 0)
        {
            defaultCol = objects[0].color;
        }
    }

    public void OnHover(bool condition)
    {
        var newCol = condition ? toSetCol : defaultCol;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null) continue;

            objects[i].color = newCol;
        }
    }
}