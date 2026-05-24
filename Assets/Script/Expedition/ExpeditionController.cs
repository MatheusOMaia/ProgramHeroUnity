using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExpeditionController : MonoBehaviour
{
    [Header("Stages")]
    public List<ExpeditionStage> stages = new();

    [Header("UI")]
    public Transform stageContainer;
    public GameObject stageButtonPrefab;

    [Header("Navigation")]
    public string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        BuildStageButtons();
    }

    private void BuildStageButtons()
    {
        if (stageContainer == null || stageButtonPrefab == null)
        {
            Debug.LogWarning("StageContainer ou StageButtonPrefab não atribuído.");
            return;
        }

        for (int i = stageContainer.childCount - 1; i >= 0; i--)
            Destroy(stageContainer.GetChild(i).gameObject);

        foreach (ExpeditionStage stage in stages)
        {
            if (stage == null)
                continue;

            GameObject buttonObject = Instantiate(stageButtonPrefab, stageContainer);

            StageButtonUI buttonUI = buttonObject.GetComponent<StageButtonUI>();

            if (buttonUI == null)
            {
                Debug.LogError("StageButtonPrefab precisa ter StageButtonUI.");
                continue;
            }

            buttonUI.Setup(stage, this);
        }
    }

    public void StartStage(ExpeditionStage stage)
    {
        if (stage == null)
            return;

        if (string.IsNullOrWhiteSpace(stage.battleSceneName))
        {
            Debug.LogWarning("Stage sem cena de batalha definida.");
            return;
        }

        SceneManager.LoadScene(stage.battleSceneName);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}