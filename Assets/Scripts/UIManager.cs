using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _waveText;
    [SerializeField] TextMeshProUGUI _nextWaveTimer;
    Color32 _gray = new(144, 144, 144, 255);
    Color32 _green = new(0, 154, 0, 255);
    public static UIManager Instance;

    [SerializeField] GameObject _buildPanelPrefab;
    [SerializeField] GameObject _upgradePanelPrefab;

    [SerializeField] GameObject _scrollPanelFrame;
    [SerializeField] GameObject _scrollPanel;
    [SerializeField] GameObject _mainPanel;

    private void Awake()
    {
        Instance = this;
    }

    public void AddUpgradeOption(string upgradeName, float initialStat, float newStat, string upgradeDesc, int upgradeCost, byte upgradeIndex)
    {
        GameObject newPanel = Instantiate(_upgradePanelPrefab, _scrollPanel.transform);
        UpgradePanel newPanelInfo = newPanel.GetComponent<UpgradePanel>();
        newPanelInfo.AssembleStats(upgradeName, initialStat, newStat, upgradeDesc, upgradeCost, upgradeIndex);
    }

    public void UpdateWaveText(int currentWave, int maxWave)
    {
        _waveText.text = $"Wave {currentWave}/{maxWave}";
    }

    public void EngageNextWaveTimer(float timeLeftInSeconds)
    {
        _nextWaveTimer.GetComponent<NextWaveTimer>().SetNextWaveTimer(timeLeftInSeconds);
    }

    public void AddBuildOption(string turretName, string turretTier, int cost, byte turretIndex)
    {
        GameObject newPanel = Instantiate(_buildPanelPrefab, _scrollPanel.transform);
        BuildStatUI newPanelInfo = newPanel.GetComponent<BuildStatUI>();
        newPanelInfo.AssembleBuildPanel(
            turretName,
            turretTier,
            cost,
            turretIndex);
    }

    public void ActivateMainUIPanel()
    {
        if (!_mainPanel.activeSelf)
        {
            _mainPanel.SetActive(true);
        }
    }

    public void PullScrollRectToTop()
    {
        _scrollPanelFrame.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
    }

    public void DeactivateMainUIPanel()
    {
        if (_mainPanel.activeSelf)
        {
            _mainPanel.SetActive(false);
        }
    }

    public void RemoveAllLeftPanelElements()
    {
        foreach (Transform child in _scrollPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
