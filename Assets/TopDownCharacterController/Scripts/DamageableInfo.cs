using UnityEngine;

public enum DamageableKind
{
    Character,
    Surface
}

public class DamageableInfo : MonoBehaviour
{
    public DamageableKind Kind;

    [Header("If Kind = Character")]
    public CharacterType CharacterType;

    [Header("If Kind = Surface")]
    public MaterialType MaterialType;
}