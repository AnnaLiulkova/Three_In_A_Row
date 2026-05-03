using UnityEngine;
using TMPro;


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
    }
}