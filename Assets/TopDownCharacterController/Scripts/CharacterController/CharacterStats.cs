using UnityEngine;

[CreateAssetMenu(menuName = "Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("Health")]
    [Tooltip("Maximum amount of health the character can have")]
    public float MaxHealth = 100f;

    [Header("Walk")]
    [Tooltip("Maximum walking speed of the character")]
    public float MaxWalkSpeed = 12.5f;

    [Tooltip("How quickly the character reaches maximum speed")]
    public float Acceleration = 5f;

    [Tooltip("How quickly the character slows down to a stop")]
    public float Deceleration = 5f;
}