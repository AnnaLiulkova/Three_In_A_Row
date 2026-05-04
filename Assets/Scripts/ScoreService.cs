using TMPro;
using UnityEngine;

public class ScoreService : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;
    private int _score;

    private void Start()
    {
        _score = 0;
        _scoreText.text = "0";
    }

    public void AddScore(int score)
    {
        _score += score;
        _scoreText.text = _score.ToString();
        
        if (GameManager.Instance != null) GameManager.Instance.OnScoreUpdated(_score);
    }

    public void ResetScore()
    {
        _score = 0;
        _scoreText.text = "0";
    }

    public int GetCurrentScore() => _score;
}