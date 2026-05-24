using UnityEngine;
using UnityEngine.UI;

public class TurnPortraitUI : MonoBehaviour
{
    public Image portraitImage;
    public Image backgroundImage;

    public Color currentColor = Color.white;
    public Color normalColor = new Color(1f, 1f, 1f, 0.5f);

    public void Setup(Unit unit, bool isCurrent)
    {
        if (portraitImage != null)
        {
            portraitImage.sprite = unit != null ? unit.portrait : null;
            portraitImage.enabled = unit != null && unit.portrait != null;
            portraitImage.preserveAspect = true;
        }

        if (backgroundImage != null)
            backgroundImage.color = isCurrent ? currentColor : normalColor;
    }
}