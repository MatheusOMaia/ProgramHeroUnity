using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Weapon Ability")]
public class WeaponAbility : Ability
{
    public enum ScalingStat
    {
        Strength,
        Precision,
        Magic
    }

    public int baseDamage = 1;
    public ScalingStat scalingStat = ScalingStat.Strength;
    public float scalingMultiplier = 1f;

    public bool isRanged = false;

    public int GetDamage(Unit user)
    {
        int statValue = 0;

        switch (scalingStat)
        {
            case ScalingStat.Strength:
                statValue = user.strength;
                break;

            case ScalingStat.Precision:
                statValue = user.precision;
                break;

            case ScalingStat.Magic:
                statValue = user.magic;
                break;
        }

        return baseDamage + Mathf.FloorToInt(statValue * scalingMultiplier);
    }

    public override void Use(
        Unit user,
        Unit target,
        CubeCoord targetCoord,
        CombatController combat
    )
    {
        if (target == null)
        {
            Debug.Log("Nenhum alvo.");
            return;
        }

        if (!CanUse(user, target, targetCoord))
        {
            Debug.Log("Energia insuficiente.");
            return;
        }

        int distance = combat.Topology.GetDistance(
            user.currentCoord,
            target.currentCoord
        );

        if (distance > range)
        {
            Debug.Log("Alvo fora de alcance.");
            return;
        }

        user.energy -= energyCost;

        int damage = GetDamage(user);
        target.TakeDamage(damage);

        Debug.Log($"{user.name} usou {displayName} em {target.name}, causando {damage} de dano.");
    }
}