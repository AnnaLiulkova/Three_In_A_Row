using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Меню панелі")]
    public GameObject mainMenuPanel;
    public GameObject modeSelectionPanel;

    [Header("Кнопки рівнів")]
    public Button level1Button;
    public Button level2Button;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        modeSelectionPanel.SetActive(false);
    }

    public void ShowModeSelection()
    {
        mainMenuPanel.SetActive(false);
        modeSelectionPanel.SetActive(true);

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        
        level1Button.interactable = true; 
        level2Button.interactable = (unlockedLevel >= 2); 
    }

    public void LoadArcade() => SceneManager.LoadScene("ArcadeScene");
    public void LoadLevel1() => SceneManager.LoadScene("Level1Scene");
    public void LoadLevel2() => SceneManager.LoadScene("Level2Scene");
    public void QuitGame() => Application.Quit();
}