using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _waveText;
    [SerializeField] TextMeshProUGUI _nextWaveTimer;
    Color32 _gray = new(144, 144, 144, 255);
    Color32 _green = new(0, 154, 0, 255);
    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateWaveText(int currentWave, int maxWave)
    {
        _waveText.text = $"Wave {currentWave}/{maxWave}";
    }

    public void EngageNextWaveTimer(float timeLeftInSeconds)
    {
        _nextWaveTimer.GetComponent<NextWaveTimer>().SetNextWaveTimer(timeLeftInSeconds);
    }
}
