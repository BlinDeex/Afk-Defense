using System;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    float _gameTime;
    TextMeshProUGUI _timerText;

    private void Awake()
    {
        _timerText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _gameTime += Time.deltaTime;
        TimeSpan timespan = TimeSpan.FromSeconds(_gameTime);
        _timerText.text = timespan.ToString(@"mm\:ss");
    }
}
