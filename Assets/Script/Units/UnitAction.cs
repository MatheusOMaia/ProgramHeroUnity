using UnityEngine;

[System.Serializable]
public class UnitAction
{
    public string actionId = "";
    public string displayName = "";
    public Sprite icon;

    public int energyCost = 0;
    public int damage = 0;
    public int range = 1;

    public Tool sourceTool;
    public Ability ability;
}