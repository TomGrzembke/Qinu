using System.Collections.Generic;
using UnityEngine;

public class CharManager : MonoBehaviour
{
    #region serialized fields
    public static CharManager Instance;
    [field: SerializeField] public GameObject[] CharPrefabs { get; private set; }
    [field: SerializeField] public List<GameObject> CharsSpawned { get; private set; }
    [SerializeField] Transform leftSpawn;
    [SerializeField] Transform rightSpawn;
    #endregion

    #region private fields

    #endregion

    void Awake()
    {
        Instance = this;
    }

    public GameObject InitializeChar(GameObject gO, bool isRight)
    {
        GameObject newChar = gO;
        NPCNav target;
        if (!CharsSpawned.Contains(gO))
        {
            newChar = Instantiate(gO, transform);
            CharsSpawned.Add(gO);
        }

        for (int i = 0; i < CharsSpawned.Count; i++)
        {
            if (gO == CharsSpawned[i])
            {
                target = newChar.GetComponent<NPCNav>();
                target.SideSettings(isRight);
                target.SetArenaMode(NPCNav.ArenaMode.ToArena);
            }
        }
        return newChar;
    }
}