using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _waveText;
    [SerializeField] TextMeshProUGUI _nextWaveTimer;
    public static UIManager Instance;

    [SerializeField] GameObject _buildPanelPrefab;
    [SerializeField] GameObject _upgradePanelPrefab;

    [SerializeField] GameObject _buildScrollPanelFrame;
    [SerializeField] GameObject _buildScrollPanel;
    [SerializeField] GameObject _buildMenu;

    [SerializeField] GameObject _upgradeMenu;
    [SerializeField] GameObject _upgradeScrollPanelFrame;
    [SerializeField] GameObject _upgradeScrollPanel;

    [SerializeField] GameObject _gearButton;
    [SerializeField] TextMeshProUGUI _sellText;

    [SerializeField] TextMeshProUGUI _turretTargetingTypeText;
    [SerializeField] TextMeshProUGUI _turretTargetingBehaviourText;

    [SerializeField] GameObject _turretTargetingTypeButton;
    [SerializeField] GameObject _turretTargetingBehaviourButton;

    [SerializeField] GameObject _rangeIndicator;
    const float RANGE_MULTIPLIER = 9.388f;

    private void Awake()
    {
        Instance = this;
    }

    public void ChangeGearButtonState(bool state)
    {
        _gearButton.SetActive(state);
    }

    public void EnableRangeIndicator(float range)
    {
        _rangeIndicator.transform.localPosition = new Vector3(0, range * RANGE_MULTIPLIER, 0);
        _rangeIndicator.SetActive(true);
    }

    public void UpdateRangeIndicator(float range)
    {
        _rangeIndicator.transform.localPosition = new Vector3(0, range * RANGE_MULTIPLIER, 0);
    }

    public void DisableRangeIndicator()
    {
        _rangeIndicator.SetActive(false);
    }

    public void AddUpgradeOption(string upgradeName, float initialStat, float newStat, string upgradeDesc, int upgradeCost, byte upgradeIndex)
    {
        GameObject newPanel = Instantiate(_upgradePanelPrefab, _upgradeScrollPanel.transform);
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

    public void AddBuildOption(string turretName, TurretTierEnum Tier, int cost, byte turretIndex)
    {
        GameObject newPanel = Instantiate(_buildPanelPrefab, _buildScrollPanel.transform);
        BuildStatUI newPanelInfo = newPanel.GetComponent<BuildStatUI>();
        newPanelInfo.AssembleBuildPanel(
            turretName,
            Tier,
            cost,
            turretIndex);
    }

    public void UpdateTurretTargetingTexts(TargetingType type, TargetingBehaviour behaviour)
    {
        _turretTargetingTypeText.text = type.ToString();
        _turretTargetingBehaviourText.text = behaviour.ToString();
    }

    public void ActivateBuildMenu()
    {
        _buildMenu.SetActive(true);
        _upgradeMenu.SetActive(false);
    }

    public void ActivateUpgradeMenu(bool hasTargeting)
    {
        _turretTargetingTypeButton.SetActive(hasTargeting);
        _turretTargetingBehaviourButton.SetActive(hasTargeting);
        _upgradeMenu.SetActive(true);

        _buildMenu.SetActive(false);
    }

    public void ChangeSellAmountText(int amount)
    {
        _sellText.text = $"+{amount}";
    }

    public void PullScrollRectToTopBuildMenu()
    {
        _buildScrollPanelFrame.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
    }

    public void PullScrollRectToTopUpgradeMenu()
    {
        _upgradeScrollPanelFrame.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
    }

    public void DeactivateBuildMenu()
    {
        PlayerCore.Instance.DisableSelectionEffect();
        _buildMenu.SetActive(false);
    }

    public void DeactivateUpgradeMenu()
    {
        PlayerCore.Instance.DisableSelectionEffect();
        DisableRangeIndicator();
        _upgradeMenu.SetActive(false);
    }

    public void RemoveAllBuildOptions()
    {
        foreach (Transform child in _buildScrollPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void RemoveAllUpgradeOptions()
    {
        foreach (Transform child in _upgradeScrollPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
