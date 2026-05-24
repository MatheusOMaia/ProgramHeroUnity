using System.Collections.Generic;
using UnityEngine;

public class TurnOrderUI : MonoBehaviour
{
    public GameObject portraitPrefab;
    public Transform portraitContainer;

    public void ShowTurnOrder(List<Unit> order, Unit currentUnit)
    {
        Clear();

        if (portraitPrefab == null)
        {
            Debug.LogWarning("TurnOrderUI: Portrait prefab não atribuído.");
            return;
        }

        Transform container = portraitContainer != null ? portraitContainer : transform;

        foreach (Unit unit in order)
        {
            if (unit == null)
                continue;

            GameObject obj = Instantiate(portraitPrefab, container);

            TurnPortraitUI portrait = obj.GetComponent<TurnPortraitUI>();

            if (portrait != null)
                portrait.Setup(unit, unit == currentUnit);
        }
    }

    public void Clear()
    {
        Transform container = portraitContainer != null ? portraitContainer : transform;

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }
}