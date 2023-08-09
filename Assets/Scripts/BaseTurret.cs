using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class BaseTurret : MonoBehaviour
{
    [Serializable]
    public class UpgradeTier
    {
        public string UpgradeName;
        public byte UpgradeIndex;
        public float AddedStat;
        public float CurrentStat;
        public string UpgradeDescription;
        public int Cost;
    }

    [Serializable]
    public class UpgradeClassList
    {
        public List<UpgradeTier> UpgradeClass;
    }

    public List<UpgradeClassList> Upgrades;

    public Dictionary<int, Func<float?, float?>> UpgradesDict { get; protected set; } = new();

    public int TurretID;
    public int SlotTurretIsBuiltOn;
    public int TotalSellValue;

    public abstract void ApplyUpgrade(int index);

    public void RefreshUpgradesInfo()
    {
        for (int i = 0; i < Upgrades.Count; i++)
        {
            if (Upgrades[i].UpgradeClass.Count == 0) continue;
            Upgrades[i].UpgradeClass[0].CurrentStat = UpgradesDict[i].Invoke(null).Value;
        }
    }

    public void PopulateUIUpgrades(bool newSelected = false)
    {
        UIManager.Instance.RemoveAllLeftPanelElements();
        RefreshUpgradesInfo();

        foreach (UpgradeClassList upgradeIndex in Upgrades)
        {
            if (upgradeIndex.UpgradeClass.Count == 0) continue;

            UpgradeTier upgradeInfo = upgradeIndex.UpgradeClass[0];

            UIManager.Instance.AddUpgradeOption(
                upgradeInfo.UpgradeName,
                upgradeInfo.CurrentStat,
                upgradeInfo.CurrentStat + upgradeInfo.AddedStat,
                upgradeInfo.UpgradeDescription,
                upgradeInfo.Cost,
                upgradeInfo.UpgradeIndex);
        }
        UIManager.Instance.ActivateMainUIPanel();
        if (newSelected) UIManager.Instance.PullScrollRectToTop();
    }
}
