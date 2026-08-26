using UnityEngine;

[System.Serializable]
public struct ObjectDefinition
{
    public bool canFly;
    public bool canSwim;
    public bool canAttack;
    public Sprite objectiveSprite; // Replaces the string name
}