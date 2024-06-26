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
        NPCNav target;
        if (!CheckIfCloneAvailable(gO, out GameObject newChar))
        {
            newChar = Instantiate(gO, isRight ? rightSpawn.position : leftSpawn.position, Quaternion.identity, transform);
            CharsSpawned.Add(newChar);
        }

        for (int i = 0; i < CharsSpawned.Count; i++)
        {
            if (newChar == CharsSpawned[i])
            {
                target = newChar.GetComponent<NPCNav>();
                target.SideSettings(isRight);
                target.ToArena();
            }
        }
        return newChar;
    }

    bool CheckIfCloneAvailable(GameObject inGO, out GameObject outGO)
    {
        outGO = inGO;
        CleanList();

        for (int i = 0; i < CharsSpawned.Count; i++)
        {
            if (CharsSpawned[i].name.Contains(inGO.name))
            {
                outGO = CharsSpawned[i];
                return true;
            }
        }

        return false;
    }

    public void CleanList()
    {
        CharsSpawned.CleanList();
    }
}