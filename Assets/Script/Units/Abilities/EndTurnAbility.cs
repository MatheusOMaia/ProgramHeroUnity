using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/End Turn Ability")]
public class EndTurnAbility : Ability
{
    public override void Use(
        Unit user,
        Unit target,
        CubeCoord targetCoord,
        CombatController combat
    )
    {
        if (combat == null)
            return;

        combat.EndCurrentTurn();
    }
}