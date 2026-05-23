using UnityEngine;

public class OctopusUnit : Unit
{
    public override void Awake()
    {
        base.Awake();

        unitType = "octopus";

        maxHp = 10;
        hp = 10;

        maxEnergy = 15;
        energy = 5;
        energyRecovery = 5;

        strength = 5;
        precision = 5;
        magic = 5;

        moveRange = 2;
        initiative = 10;
        turnsPerRound = 1;
    }
}