using MyBox;
using UnityEngine;

[CreateAssetMenu]
public class CharSO : ScriptableObject
{
    [field: SerializeField] public float charScale { get; private set; }

    public float pixelRes = 128;
    public Vector2 screenRes = new(1920, 1080);
    public Material pixelartMat;
    public Color col;
    RangedFloat screenRatio;
    Vector2 scaling;
}