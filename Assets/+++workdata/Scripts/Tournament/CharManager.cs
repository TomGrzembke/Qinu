using UnityEngine;

public class CharManager : MonoBehaviour
{
    #region serialized fields
    public static CharManager Instance;
    [field: SerializeField] public CharNav[] CharNavs { get; private set; }
    #endregion

    #region private fields

    #endregion

    void Awake()
    {
        Instance = this;
    }

    public void PathGOTo(GameObject gO, Vector3 pos)
    {
        for (int i = 0; i < CharNavs.Length; i++)
        {
            if (gO == CharNavs[i].gameObject)
            {
                CharNavs[i].ActivateNavCalc();
                CharNavs[i].NavCalc.SetAgentPosition(pos);
            }
        }
    }
}