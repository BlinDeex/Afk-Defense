using TMPro;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _upgradeName;
    [SerializeField] TextMeshProUGUI _initialStat;
    [SerializeField] TextMeshProUGUI _newStat;
    [SerializeField] TextMeshProUGUI _upgradeDesc;
    [SerializeField] TextMeshProUGUI _upgradeCost;
    [SerializeField] byte StatIndex;

    public void AssembleStats(string upgradeName, float initialStat, float newStat, string upgradeDesc, int upgradeCost, byte statIndex)
    {
        _upgradeName.text = upgradeName;
        _initialStat.text = string.Format("{0:N2}", initialStat);
        _newStat.text = string.Format("{0:N2}", newStat);
        _upgradeDesc.text = upgradeDesc;
        _upgradeCost.text = upgradeCost.ToString();
        StatIndex = statIndex;
    }

    public void UpgradePressed()
    {
        if (CurrencyManager.Instance.TryRemoveGold(int.Parse(_upgradeCost.text)))
        {
            PlayerCore.Instance.UpgradeTurret(StatIndex);
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }
}
