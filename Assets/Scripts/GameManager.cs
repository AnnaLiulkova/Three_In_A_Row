using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameMode { Arcade, Campaign }
    
    [Header("Сервіси")]
    public BoardService boardService;
    public ScoreService scoreService;
    public LeaderboardManager leaderboardManager;

    [Header("UI Панелі")]
    public GameObject mainMenuPanel;
    public GameObject modeSelectionPanel;
    public GameObject levelSelectionPanel;
    public GameObject gameUIPanel;
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject endScorePanel;  
    public GameObject pauseMenuPanel;
    public GameObject leaderboardPanel; 

    [Header("UI Елементи")]
    public TMP_Text topInfoText; 
    public TMP_Text endScoreTextArcade; 
    public UnityEngine.UI.Button level2Button;

    [Header("=== Налаштування Аркади ===")]
    public float arcadeTimeLimit = 180f;
    public ArrayLayout arcadeLayout;

    [Header("=== Налаштування Рівня 1 ===")]
    public int level1TargetScore = 1500;
    public int level1MaxMoves = 40;
    public ArrayLayout level1Layout;

    [Header("=== Налаштування Рівня 2 ===")]
    public int level2TargetScore = 3000;
    public int level2MaxMoves = 35;
    public ArrayLayout level2Layout;

    private GameMode _currentMode;
    private int _currentLevelIndex;
    private int _targetScore;
    private int _movesLeft;
    private float _timeLeft;
    private bool _isGameOver = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f; 
        ShowMainMenu();
    }

    private void Update()
    {
        if (_isGameOver) return;

        if (_currentMode == GameMode.Arcade)
        {
            _timeLeft -= Time.deltaTime;

            if (_timeLeft <= 0)
            {
                _timeLeft = 0;
                UpdateUI();    
                GameOver(false); 
            }
            else
            {
                UpdateUI(); 
            }
        }
    }


    // НАВІГАЦІЯ ПО МЕНЮ

    public void ShowMainMenu()
    {
        Time.timeScale = 1f; 
        _isGameOver = true;
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    public void ShowModeSelection()
    {
        HideAllPanels();
        modeSelectionPanel.SetActive(true);
    }

    public void ShowLevelSelection()
    {
        HideAllPanels();
        levelSelectionPanel.SetActive(true);

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        level2Button.interactable = (unlockedLevel >= 2);
    }

    private void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        modeSelectionPanel.SetActive(false);
        levelSelectionPanel.SetActive(false);
        gameUIPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        endScorePanel.SetActive(false);
        pauseMenuPanel.SetActive(false);

        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        HideAllPanels();
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
 
        if (FindObjectOfType<LeaderboardManager>() != null)
        {
           FindObjectOfType<LeaderboardManager>().UpdateLeaderboardUI();
        }
    }   

    // ПАУЗА

    public void PauseGame()
    {
        if (_isGameOver) return;
        
        Time.timeScale = 0f; 
        pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; 
        pauseMenuPanel.SetActive(false);
    }

    // ЗАПУСК РІВНІВ
    public void StartArcade()
    {
        _currentMode = GameMode.Arcade;
        _timeLeft = arcadeTimeLimit;
        SetupGameUI(arcadeLayout);
    }

    public void StartLevel1()
    {
        _currentMode = GameMode.Campaign;
        _currentLevelIndex = 1;
        _targetScore = level1TargetScore;
        _movesLeft = level1MaxMoves;
        SetupGameUI(level1Layout);
    }

    public void StartLevel2()
    {
        _currentMode = GameMode.Campaign;
        _currentLevelIndex = 2;
        _targetScore = level2TargetScore;
        _movesLeft = level2MaxMoves;
        SetupGameUI(level2Layout);
    }

    private void SetupGameUI(ArrayLayout layout)
    {
        Time.timeScale = 1f; 
        HideAllPanels();
        gameUIPanel.SetActive(true);

        scoreService.ResetScore();
        boardService.StartNewGame(layout); 
        
        _isGameOver = false;
        UpdateUI();
    }

    // ЛОГІКА ХОДІВ ТА ОЧОК

    public void OnMoveMade()
    {
        if (_isGameOver || _currentMode != GameMode.Campaign) return;

        _movesLeft--;
        UpdateUI();

        if (_movesLeft <= 0)
        {
            Invoke(nameof(CheckEndGameDelayed), 1.5f);
        }
    }

    public void OnScoreUpdated(int currentScore)
    {
        if (_isGameOver) return;

        if (_currentMode == GameMode.Campaign && currentScore >= _targetScore)
        {
            GameOver(true);
        }
    }

    private void CheckEndGameDelayed()
    {
        if (_isGameOver) return;
        GameOver(false); 
    }

    private void UpdateUI()
    {
        if (_currentMode == GameMode.Arcade)
        {
            int m = Mathf.FloorToInt(_timeLeft / 60);
            int s = Mathf.FloorToInt(_timeLeft % 60);
            topInfoText.text = $"TIME: {m:00}:{s:00}";
        }
        else
        {
            topInfoText.text = $"MOVES: {_movesLeft}"; 
        }
    }


    // КІНЕЦЬ ГРИ ТА РЕСТАРТ

    private void GameOver(bool isWin)
    {
        _isGameOver = true;
        
        if (_currentMode == GameMode.Arcade)
        {
    if (leaderboardManager != null)
    {
        leaderboardManager.SaveScore(scoreService.GetCurrentScore());
    }

    if (endScoreTextArcade != null)
    {
        endScoreTextArcade.text = "SCORE: " + scoreService.GetCurrentScore();
    }
    endScorePanel.SetActive(true);
    return; 
}

        if (isWin)
        {
            int unlockedLvl = PlayerPrefs.GetInt("UnlockedLevel", 1);
            if (_currentLevelIndex >= unlockedLvl)
            {
                PlayerPrefs.SetInt("UnlockedLevel", _currentLevelIndex + 1);
            }
            winPanel.SetActive(true);
        }
        else
        {
            losePanel.SetActive(true);
        }
    }

    public void RestartCurrentLevel()
    {
        if (_currentMode == GameMode.Arcade) StartArcade();
        else if (_currentLevelIndex == 1) StartLevel1();
        else if (_currentLevelIndex == 2) StartLevel2();
    }

    public void QuitGame() => Application.Quit();
}