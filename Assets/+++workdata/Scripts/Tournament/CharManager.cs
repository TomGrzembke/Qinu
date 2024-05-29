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
            target = gO.GetComponent<NPCNav>();
            target.SetArenaMode(NPCNav.ArenaMode.ToArena);
        }


        for (int i = 0; i < CharPrefabs.Length; i++)
        {
            if (gO == CharPrefabs[i].gameObject)
            {
                target = CharPrefabs[i].GetComponent<NPCNav>();
                target.SideSettings(isRight);
            }
        }
        return newChar;
    }
}