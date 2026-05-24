using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageButtonUI : MonoBehaviour
{
    public TMP_Text labelText;

    private ExpeditionStage stage;
    private ExpeditionController expeditionController;

    public void Setup(ExpeditionStage newStage, ExpeditionController controller)
    {
        stage = newStage;
        expeditionController = controller;

        if (labelText != null)
            labelText.text = stage.stageName;

        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
        }
    }

    private void OnPressed()
    {
        if (expeditionController == null || stage == null)
            return;

        expeditionController.StartStage(stage);
    }
}