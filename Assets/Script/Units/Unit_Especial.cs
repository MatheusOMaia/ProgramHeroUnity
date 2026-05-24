using UnityEngine;

public class Unit_Especial : Unit
{

    // UM SCRIPT ÚNICO USADO EM UNIDADES ESPECIAIS, atualmente sem utilidade
    // o polvo tem nada de especial por exemplo, então usa o script Unit básico
    
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