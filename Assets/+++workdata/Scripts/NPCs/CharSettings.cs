using UnityEngine;

[System.Serializable]
public class PlayerCharSettings
{
    [field: SerializeField] public PlayerRigidSettings CharRigidSettings { get; private set; }
}

[System.Serializable]
public class NPCCharSettings
{
    [field: SerializeField] public CharNPCSettings CharNPCSettings { get; private set; }
    [field: SerializeField] public NPCRigidSettings CharRigidSettings { get; private set; }
}
