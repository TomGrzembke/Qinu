using System.Collections.Generic;
using UnityEngine;

public class SituationalDialogue : MonoBehaviour
{
    #region serialized fields
    public static SituationalDialogue Instance;

    #endregion

    #region private fields
    [SerializeField] Dictionary<string, int> CharCounts = new();

    #endregion
    void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(string charName)
    {
        charName = charName.Remove(0,7);

        if (charName.Contains("(Clone)"))
            charName = charName.Split("(")[0];

        if(!CharCounts.ContainsKey(charName)) 
        {
            CharCounts.Add(charName, 0);
        }

        DialogueController.Instance.StartDialogue(charName + CharCounts[charName]++);
    }
}