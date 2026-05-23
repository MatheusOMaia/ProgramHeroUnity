using System.Collections.Generic;
using UnityEngine;

public abstract class Tool : ScriptableObject
{
    public string toolId = "";
    public string displayName = "";
    public Sprite icon;

    public int slotCost = 1;

    public List<Ability> abilities = new();

    public virtual List<UnitAction> CreateActions()
    {
        List<UnitAction> actions = new();

        foreach (Ability ability in abilities)
        {
            if (ability == null)
                continue;

            actions.Add(ability.CreateAction(this));
        }

        return actions;
    }
}