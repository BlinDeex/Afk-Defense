using System;
using TMPro;
using UnityEngine;

public class NextWaveTimer : MonoBehaviour
{
    TextMeshProUGUI _text;
    float _timeLeft;
    bool empty = false;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void SetNextWaveTimer(float timeInSeconds)
    {
        _timeLeft = timeInSeconds;
        empty = false;
    }

    private void Update()
    {
        if (!empty)
        {
            _timeLeft -= Time.deltaTime;
            TimeSpan timespan = TimeSpan.FromSeconds(_timeLeft);
            _text.text = timespan.ToString(@"mm\:ss");

            if(_timeLeft <= 0)
            {
                empty = true;
                _text.text = "";
            }
        }
    }
}
