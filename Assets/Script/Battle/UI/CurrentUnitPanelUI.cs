using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentUnitPanelUI : MonoBehaviour
{
    public Image portraitImage;
    public TMP_Text hpText;
    public TMP_Text energyText;

    public void ShowUnit(Unit unit)
    {
        if (unit == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (portraitImage != null)
        {
            portraitImage.sprite = unit.portrait;
            portraitImage.enabled = unit.portrait != null;
            portraitImage.preserveAspect = true;
        }

        if (hpText != null)
            hpText.text = $"HP: {unit.hp}/{unit.maxHp}";

        if (energyText != null)
            energyText.text = $"EN: {unit.energy}/{unit.maxEnergy}";
    }
}