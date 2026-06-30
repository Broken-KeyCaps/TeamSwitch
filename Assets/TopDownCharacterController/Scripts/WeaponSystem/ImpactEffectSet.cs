using UnityEngine;

[CreateAssetMenu(menuName = "Impact Effect Set")]
public class ImpactEffectSet : ScriptableObject
{
    [Header("Default Fallback")]
    public GameObject DefaultImpact;

    [Header("Character Effects")]
    public GameObject HumanImpact;
    public GameObject AlienImpact;

    [Header("Material Effects")]
    public GameObject MetalImpact;
    public GameObject StoneImpact;

    public GameObject GetEffect(CharacterType? characterType, MaterialType? materialType)
    {
        if (characterType.HasValue)
        {
            switch (characterType.Value)
            {
                case CharacterType.Human: return HumanImpact;
                case CharacterType.Alien: return AlienImpact;
            }
        }

        if (materialType.HasValue)
        {
            switch (materialType.Value)
            {
                case MaterialType.Metal: return MetalImpact;
                case MaterialType.Stone: return StoneImpact;
            }
        }

        return DefaultImpact;
    }
}
