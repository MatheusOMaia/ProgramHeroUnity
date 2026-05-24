using UnityEngine;

[CreateAssetMenu(menuName = "Expedition/Stage")]
public class ExpeditionStage : ScriptableObject
{
    public string stageName = "Fase";
    public string battleSceneName = "BasePlanet";

    [TextArea]
    public string description;
}