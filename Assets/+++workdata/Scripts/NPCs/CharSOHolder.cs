using System;
using UnityEngine;

public class CharSOHolder : MonoBehaviour
{
    [field: SerializeField] public CharSO CharSO { get; private set; }
    public event Action<CharSO> CharSOChanged;

    public void ChangeCharSO(CharSO charSO)
    {
        if (!charSO)
        {
            Debug.LogError($"Cannot assign a null character settings asset to {name}.", this);
            return;
        }

        CharSO = charSO;
        CharSOChanged?.Invoke(charSO);
    }
}
