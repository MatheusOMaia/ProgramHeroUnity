using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Move Ability")]
public class MoveAbility : Ability
{
    public override void Use(
        Unit user,
        Unit target,
        CubeCoord targetCoord,
        CombatController combat
    )
    {
        if (user == null)
            return;

        if (!CanUse(user, target, targetCoord))
        {
            Debug.Log("Energia insuficiente.");
            return;
        }

        int distance = combat.Topology.GetDistance(
            user.currentCoord,
            targetCoord
        );

        if (distance > user.moveRange)
        {
            Debug.Log("Tile fora do alcance.");
            return;
        }

        user.energy -= energyCost;

        var path = combat.Topology.ShortestPath(
            user.currentCoord,
            targetCoord
        );

        user.FollowPath(path, combat.Planet);

        Debug.Log($"{user.name} começou a se mover.");
    }
}