using UnityEngine;

public class CharSOHolder : MonoBehaviour
{
    [field: SerializeField] public CharSO CharSO { get; private set; }

    public void ChangeCharSO(CharSO charSO)
    {
        this.CharSO = charSO;
    }

}