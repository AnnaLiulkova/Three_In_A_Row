using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{

    [Serializable]
    private class PlayerRecord
    {
        public string playerName;
        public int score;
    }

    [Serializable]
    private class LeaderboardData
    {
        public List<PlayerRecord> records = new List<PlayerRecord>();
    }

    private const string LeaderboardKey = "ArcadeLeaderboard";

    [Header("UI Елементи")]
    public TMP_InputField nameInputField; 
    public TMP_Text listWinnerText;       
    public Button playButton;          

    private string _currentPlayerName = "";

    private void Start()
    {
        // Блокуємо кнопку Play на старті, бо ім'я ще не введено
        ValidateNameInput("");

        // Підписуємося на подію: щоразу, коли гравець щось друкує, викликається перевірка
        nameInputField.onValueChanged.AddListener(ValidateNameInput);

        // Одразу малюємо таблицю, якщо там вже є збережені дані
        UpdateLeaderboardUI();
    }


    private void ValidateNameInput(string input)
    {
        string cleanInput = input.Replace(" ", "").Replace("\t", "");

        if (cleanInput != input)
        {
            nameInputField.text = cleanInput;
        }

        _currentPlayerName = cleanInput;
        playButton.interactable = !string.IsNullOrEmpty(_currentPlayerName);
    }

    public string GetCurrentPlayerName()
    {
        return _currentPlayerName;
    }


    public void SaveScore(int newScore)
    {
        if (string.IsNullOrEmpty(_currentPlayerName)) return;

        LeaderboardData data = LoadData();
        bool playerFound = false;

        foreach (var record in data.records)
        {
            if (record.playerName == _currentPlayerName)
            {
                playerFound = true;

                if (newScore > record.score)
                {
                    record.score = newScore;
                }
                break;
            }
        }

        if (!playerFound)
        {
            data.records.Add(new PlayerRecord { playerName = _currentPlayerName, score = newScore });
        }

        data.records.Sort((a, b) => b.score.CompareTo(a.score));

        if (data.records.Count > 5)
        {
            data.records.RemoveRange(5, data.records.Count - 5);
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LeaderboardKey, json);
        PlayerPrefs.Save();

        UpdateLeaderboardUI();
    }

    public void UpdateLeaderboardUI()
    {
        if (listWinnerText == null) return;

        LeaderboardData data = LoadData();
        string displayText = "";

        for (int i = 0; i < data.records.Count; i++)
        {
            displayText += $"\"{data.records[i].playerName}\" - ({data.records[i].score})\n";
        }

        if (data.records.Count == 0)
        {
            displayText = "No winners yet!";
        }

        listWinnerText.text = displayText;
    }

    private LeaderboardData LoadData()
    {
        if (PlayerPrefs.HasKey(LeaderboardKey))
        {
            string json = PlayerPrefs.GetString(LeaderboardKey);
            return JsonUtility.FromJson<LeaderboardData>(json);
        }
        return new LeaderboardData();
    }
}