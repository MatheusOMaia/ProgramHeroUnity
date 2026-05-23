using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionButtonUI : MonoBehaviour
{
    [Header("References")]
    public Button button;
    public Image iconImage;
    public TMP_Text labelText;

    private UnitAction action;
    private CombatController combat;

    public void Setup(UnitAction newAction, CombatController controller)
    {
        action = newAction;
        combat = controller;

        gameObject.name = "ActionButton_" + action.displayName;

        if (labelText != null)
            labelText.text = action.displayName;

        if (iconImage != null)
        {
            iconImage.sprite = action.icon;
            iconImage.enabled = action.icon != null;
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
            iconImage.raycastTarget = false;

            iconImage.transform.SetAsLastSibling();
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
        }
        else
        {
            Debug.LogError("ActionButtonUI: Button não encontrado em " + gameObject.name);
        }
    }

    private void OnPressed()
    {
        if (combat == null || action == null)
            return;

        combat.SelectAction(action);
    }
}