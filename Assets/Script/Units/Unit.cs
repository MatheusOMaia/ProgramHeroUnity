using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum Team
    {
        PlayerMain,
        PlayerAlly,
        Enemy,
        Rebel
    }

    [Header("Identity")]
    public string unitType = "base";
    public Sprite portrait;
    public Team team = Team.PlayerMain;

    [Header("Life")]
    public int maxHp = 10;
    public int hp = 10;

    [Header("Energy")]
    public int energy = 5;
    public int maxEnergy = 15;
    public int energyRecovery = 5;

    [Header("Attributes")]
    public int strength = 5;
    public int precision = 5;
    public int magic = 5;

    [Header("Movement")]
    public int moveRange = 2;
    public int initiative = 10;
    public int turnsPerRound = 1;

    [Header("Movement Runtime")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 8f;

    private Queue<CubeCoord> pathQueue = new();
    private bool moving = false;
    private CubePlanet currentPlanet;

    [Header("Actions")]
    public List<UnitAction> availableActions = new();

    [Header("Basic Abilities")]
    public List<Ability> innateAbilities = new();

    [Header("Tools")]
    public int maxToolSlots = 2;
    public List<Tool> equippedTools = new();

    public CubeCoord currentCoord;

    public event Action<Unit> OnDied;

    public virtual void Awake()
    {
        ClampStats();
    }

    protected virtual void Update()
    {
        if (moving)
            StepMove();
    }

    public virtual void ClampStats()
    {
        hp = Mathf.Clamp(hp, 0, maxHp);
        energy = Mathf.Clamp(energy, 0, maxEnergy);
    }

    public virtual List<UnitAction> GetAvailableActions()
    {
        List<UnitAction> actions = new();

        // Ações manuais antigas, se você quiser usar
        foreach (UnitAction action in availableActions)
        {
            if (action == null)
                continue;

            actions.Add(action);
        }

        // Habilidades próprias da unidade
        foreach (Ability ability in innateAbilities)
        {
            if (ability == null)
                continue;

            actions.Add(ability.CreateAction(null));
        }

        // Habilidades vindas das ferramentas equipadas
        foreach (Tool tool in equippedTools)
        {
            if (tool == null)
                continue;

            actions.AddRange(tool.CreateActions());
        }

        return actions;
    }

    public int GetUsedToolSlots()
    {
        int used = 0;

        foreach (Tool tool in equippedTools)
        {
            if (tool == null)
                continue;

            used += tool.slotCost;
        }

        return used;
    }

    public bool CanEquipTool(Tool tool)
    {
        if (tool == null)
            return false;

        return GetUsedToolSlots() + tool.slotCost <= maxToolSlots;
    }

    public virtual void RecoverEnergy()
    {
        energy += energyRecovery;
        energy = Mathf.Min(energy, maxEnergy);
    }

    public virtual void TakeDamage(int amount)
    {
        hp -= amount;
        hp = Mathf.Max(hp, 0);

        Debug.Log($"{name} recebeu {amount} de dano. HP: {hp}/{maxHp}");

        if (hp <= 0)
            Die();
    }

    public virtual void Die()
    {
        Debug.Log($"{name} foi derrotado");

        OnDied?.Invoke(this);

        Destroy(gameObject);
    }

    public bool IsPlayerTeam()
    {
        return team == Team.PlayerMain || team == Team.PlayerAlly;
    }

    public bool IsHostileTo(Unit other)
    {
        if (other == null)
            return false;

        if (team == Team.Rebel)
            return other.team != Team.Rebel;

        if (other.team == Team.Rebel)
            return true;

        if (IsPlayerTeam())
            return !other.IsPlayerTeam();

        return other.IsPlayerTeam();
    }

    public void FollowPath(List<CubeCoord> path, CubePlanet planet)
    {
        if (path == null || path.Count == 0)
            return;

        currentPlanet = planet;
        pathQueue = new Queue<CubeCoord>(path);
        moving = true;
    }

    private void StepMove()
    {
        if (pathQueue.Count == 0)
        {
            moving = false;
            return;
        }

        CubeCoord nextCoord = pathQueue.Peek();

        Vector3 target = currentPlanet.GetUnitWorldPosition(nextCoord);

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        Quaternion targetRotation = currentPlanet.GetUnitRotationForFace(nextCoord.face);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentCoord = nextCoord;
            transform.position = target;
            transform.rotation = targetRotation;

            pathQueue.Dequeue();
        }
    }
    
}