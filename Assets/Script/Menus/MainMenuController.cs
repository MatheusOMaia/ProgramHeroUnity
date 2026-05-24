using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    public string expeditionSceneName = "Expedition";

    public void StartExpedition()
    {
        SceneManager.LoadScene(expeditionSceneName);
    }

    public void OpenSettings()
    {
        Debug.Log("Configurações ainda não implementadas.");
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo.");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}