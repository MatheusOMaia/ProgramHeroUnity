using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CombatController : MonoBehaviour
{
    [Header("References")]
    public CubePlanet planet;
    public Camera mainCamera;

    [Header("UI")]
    public Transform actionPanel;
    public GameObject actionButtonPrefab;

    [Header("Highlights")]
    public GameObject tileHighlightPrefab;
    public float highlightHeight = 0.08f;
    public float highlightGap = 0.015f;

    private readonly List<GameObject> activeHighlights = new();
    private readonly Dictionary<CubeCoord, bool> highlightedTiles = new();

    public CubeTopology Topology { get; private set; }
    public CubePlanet Planet => planet;

    private List<Unit> units = new();
    private List<Unit> turnQueue = new();

    public Unit currentUnit;
    public UnitAction selectedAction;

    private int currentTurnIndex = 0;
    private int roundNumber = 1;

    private bool combatReady = false;

    private void Start()
    {
        StartCoroutine(InitializeCombat());
    }

    private IEnumerator InitializeCombat()
    {
        yield return null;

        if (planet == null)
            planet = FindFirstObjectByType<CubePlanet>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (planet == null)
        {
            Debug.LogError("CombatController: CubePlanet não encontrado.");
            yield break;
        }

        Topology = planet.Topology;

        if (Topology == null)
        {
            Debug.LogError("CombatController: CubeTopology não encontrada no planeta.");
            yield break;
        }

        CollectUnits();
        BuildTurnQueue();
        StartCurrentTurn();

        combatReady = true;
    }

    private void Update()
    {
        if (!combatReady)
            return;

        HandleMouseClick();
    }

    private void HandleMouseClick()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        if (!mouse.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            return;

        Unit clickedUnit = hit.collider.GetComponentInParent<Unit>();

        if (clickedUnit != null)
        {
            UseSelectedActionOnUnit(clickedUnit);
            return;
        }

        TileView clickedTile = hit.collider.GetComponentInParent<TileView>();

        if (clickedTile != null)
        {
            UseSelectedActionOnTile(clickedTile.Coord);
            return;
        }
    }

    private void CollectUnits()
    {
        units.Clear();

        foreach (Unit unit in planet.SpawnedUnits)
        {
            if (unit == null)
                continue;

            units.Add(unit);
            unit.OnDied += OnUnitDied;
        }
    }

    private void BuildTurnQueue()
    {
        turnQueue.Clear();

        foreach (Unit unit in units)
        {
            if (unit == null)
                continue;

            for (int i = 0; i < unit.turnsPerRound; i++)
                turnQueue.Add(unit);
        }

        turnQueue.Sort((a, b) => b.initiative.CompareTo(a.initiative));

        currentTurnIndex = 0;
    }

    private void StartCurrentTurn()
    {
        if (turnQueue.Count == 0)
        {
            currentUnit = null;
            ClearActionButtons();
            return;
        }

        currentUnit = turnQueue[currentTurnIndex];

        if (currentUnit == null)
        {
            EndCurrentTurn();
            return;
        }

        currentUnit.RecoverEnergy();
        selectedAction = null;

        Debug.Log($"Rodada {roundNumber} | Turno de: {currentUnit.name}");

        BuildActionButtons(currentUnit);
    }

    public void EndCurrentTurn()
    {
        selectedAction = null;
        ClearHighlights();

        currentTurnIndex++;

        if (currentTurnIndex >= turnQueue.Count)
        {
            roundNumber++;
            BuildTurnQueue();
        }

        StartCurrentTurn();
    }

    public void SelectAction(UnitAction action)
    {
        if (currentUnit == null)
            return;

        selectedAction = action;

        Debug.Log($"Ação selecionada: {action.displayName}");

        ClearHighlights();

        if (action.ability is EndTurnAbility)
        {
            action.ability.Use(
                currentUnit,
                null,
                currentUnit.currentCoord,
                this
            );

            selectedAction = null;
            ClearHighlights();
            return;
        }

        ShowActionRange(action);
    }

    private void ShowActionRange(UnitAction action)
    {
        if (currentUnit == null || action == null || action.ability == null)
            return;

        if (action.ability is MoveAbility)
        {
            ShowMovementRange();
            return;
        }

        ShowAbilityRange(action);
    }

    private readonly Color moveHighlight = new Color(1f, 1f, 1f, 0.5f);
    private void ShowMovementRange()
    {
        List<CubeCoord> coords = Topology.GetCoordsInRange(
            currentUnit.currentCoord,
            currentUnit.moveRange
        );

        foreach (CubeCoord coord in coords)
        {
            if (coord.Equals(currentUnit.currentCoord))
                continue;

            ShowTileHighlight(
                coord,
                moveHighlight
            );
        }
    }

    private readonly Color emptyHighlight = new Color(1f, 0.9f, 0.1f, 0.5f);
    private readonly Color enemyHighlight = new Color(1f, 0f, 0f, 0.5f);
    private void ShowAbilityRange(UnitAction action)
    {
        int range = action.range;

        HashSet<CubeCoord> enemyCoords = new();

        foreach (Unit unit in units)
        {
            if (unit == null)
                continue;

            if (unit == currentUnit)
                continue;

            if (!currentUnit.IsHostileTo(unit))
                continue;

            int distance = Topology.GetDistance(
                currentUnit.currentCoord,
                unit.currentCoord
            );

            if (distance <= range)
                enemyCoords.Add(unit.currentCoord);
        }

        List<CubeCoord> coords = Topology.GetCoordsInRange(
            currentUnit.currentCoord,
            range
        );

        foreach (CubeCoord coord in coords)
        {
            if (coord.Equals(currentUnit.currentCoord))
                continue;

            if (enemyCoords.Contains(coord))
                continue;

            ShowTileHighlight(
                coord,
                emptyHighlight
            );
        }

        foreach (CubeCoord enemyCoord in enemyCoords)
        {
            ShowTileHighlight(
                enemyCoord,
                enemyHighlight
            );
        }
    }

    public void UseSelectedActionOnUnit(Unit target)
    {
        if (currentUnit == null)
            return;

        if (selectedAction == null)
        {
            Debug.Log("Nenhuma ação selecionada.");
            return;
        }

        if (selectedAction.ability == null)
            return;

        selectedAction.ability.Use(
            currentUnit,
            target,
            target.currentCoord,
            this
        );

        selectedAction = null;
        ClearHighlights();
    }

    public void UseSelectedActionOnTile(CubeCoord coord)
    {
        if (currentUnit == null)
            return;

        if (selectedAction == null)
        {
            Debug.Log("Nenhuma ação selecionada.");
            return;
        }

        if (selectedAction.ability == null)
            return;

        if (selectedAction.ability is MoveAbility)
        {
            if (!highlightedTiles.ContainsKey(coord))
            {
                Debug.Log("Tile fora do alcance destacado.");
                return;
            }
        }

        selectedAction.ability.Use(
            currentUnit,
            null,
            coord,
            this
        );

        selectedAction = null;
        ClearHighlights();
    }

    private void BuildActionButtons(Unit unit)
    {
        ClearActionButtons();

        if (actionPanel == null || actionButtonPrefab == null)
        {
            Debug.LogWarning("CombatController: ActionPanel ou ActionButtonPrefab não atribuído.");
            return;
        }

        List<UnitAction> actions = unit.GetAvailableActions();

        Debug.Log($"{unit.name} possui {actions.Count} ações disponíveis.");

        foreach (UnitAction action in actions)
        {
            Debug.Log("Criando botão: " + action.displayName);

            GameObject buttonObject = Instantiate(actionButtonPrefab, actionPanel);
            buttonObject.SetActive(true);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
            }

            ActionButtonUI buttonUI = buttonObject.GetComponent<ActionButtonUI>();

            if (buttonUI == null)
            {
                Debug.LogError("ActionButtonPrefab precisa ter ActionButtonUI.");
                continue;
            }

            buttonUI.Setup(action, this);
        }
    }

    private void ClearActionButtons()
    {
        if (actionPanel == null)
            return;

        for (int i = actionPanel.childCount - 1; i >= 0; i--)
            Destroy(actionPanel.GetChild(i).gameObject);
    }

    private void OnUnitDied(Unit unit)
    {
        units.Remove(unit);
        turnQueue.RemoveAll(u => u == unit || u == null);

        if (currentUnit == unit)
        {
            EndCurrentTurn();
            return;
        }

        if (currentTurnIndex >= turnQueue.Count)
            currentTurnIndex = 0;
    }

    private void ClearHighlights()
    {
        foreach (GameObject highlight in activeHighlights)
        {
            if (highlight != null)
                Destroy(highlight);
        }

        activeHighlights.Clear();
        highlightedTiles.Clear();
    }

    private void ShowTileHighlight(CubeCoord coord, Color color)
    {
        if (tileHighlightPrefab == null)
        {
            Debug.LogWarning("TileHighlightPrefab não atribuído.");
            return;
        }

        Vector3 position;
        Quaternion rotation;

        planet.TryGetTileHighlightPose(
            coord,
            highlightHeight,
            highlightGap,
            out position,
            out rotation
        );

        GameObject highlightObject = Instantiate(
            tileHighlightPrefab,
            position,
            rotation
        );

        TileHighlight highlight = highlightObject.GetComponent<TileHighlight>();

        if (highlight != null)
        {
            highlight.Setup(color, highlightHeight);
        }

        activeHighlights.Add(highlightObject);
        highlightedTiles[coord] = true;
    }

}