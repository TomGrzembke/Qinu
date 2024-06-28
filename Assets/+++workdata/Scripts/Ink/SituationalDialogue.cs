using System.Collections.Generic;
using UnityEngine;

/// <summary> Handles the input from a spawned character and converts the name to a usable default dialogue string </summary>
public class SituationalDialogue : MonoBehaviour
{
    #region Serialized
    [SerializeField] Dictionary<string, int> CharCounts = new();
    #endregion

    #region Non Serialized
    public static SituationalDialogue Instance;
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

        charName = charName.Trim();

        if(!CharCounts.ContainsKey(charName)) 
        {
            
            CharCounts.Add(charName, 0);
        }

        DialogueController.Instance.StartDialogue(charName + CharCounts[charName]++);
    }
}