using UnityEngine;

[System.Serializable]
public class CharSettings
{
    [field: SerializeField] public CharNPCSettings CharNPCSettings { get; private set; }
    [field: SerializeField] public CharRigidSettings CharRigidSettings { get; private set; }
}