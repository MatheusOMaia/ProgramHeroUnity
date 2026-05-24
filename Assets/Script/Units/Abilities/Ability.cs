using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityId = "";
    public string displayName = "";
    public Sprite icon;

    public int energyCost = 0;
    public int range = 1;

    public virtual bool CanUse(Unit user, Unit target, CubeCoord targetCoord)
    {
        if (user == null)
            return false;

        return user.energy >= energyCost;
    }

    public abstract void Use(
        Unit user,
        Unit target,
        CubeCoord targetCoord,
        CombatController combat
    );

    public UnitAction CreateAction(Tool sourceTool = null)
    {
        return new UnitAction
        {
            actionId = abilityId,
            displayName = displayName,
            icon = icon,
            energyCost = energyCost,
            range = range,
            ability = this,
            sourceTool = sourceTool
        };
    }
}