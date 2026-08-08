using UnityEngine;

public abstract class CharSO : ScriptableObject
{
    [field: SerializeField] public GameObject VisualPrefab { get; private set; }
    [field: SerializeField] public CharAestheticSettings charAestheticSettings { get; private set; }
    [field: SerializeField] public DashSettings DashSettings { get; private set; }

    public abstract float MaxSpeed { get; }
    public abstract float StoppingDistance { get; }
}
