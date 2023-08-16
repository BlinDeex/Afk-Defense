using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public abstract class BaseTurret : MonoBehaviour
{
    [Serializable]
    public class UpgradeTier // particular tier of upgrade type
    {
        public string UpgradeName;
        public byte UpgradeIndex;
        public float AddedStat;
        [NonSerialized] public float CurrentStat;
        public string UpgradeDescription;
        public int Cost;
    }

    [Serializable]
    public class UpgradeClassList
    {
        public List<UpgradeTier> UpgradeClass; // upgrade type
    }

    public List<UpgradeClassList> Upgrades; // upgrade types

    public Dictionary<int, Func<float?, float?>> UpgradesDict { get; protected set; } = new();

    public int TurretID { get; protected set; }
    public int SlotTurretIsBuiltOn { get; protected set; }
    public int CurrentSellValue { get; protected set; }

    [SerializeField] TargetingType _targetingType;
    [SerializeField] TargetingBehaviour _targetingBehaviour;

    public BaseEnemy CurrentTarget { get; protected set; }
    public int CurrentTargetUID { get; protected set; }
    public bool HasTarget { get; protected set; }
    public bool TargetChanged { get; protected set; }

    [field: SerializeField] public float Range { get; protected set; }

    public void ChangeTargetingType(TargetingType type)
    {
        _targetingType = type;
    }

    public void ChangeTargetingBehaviour(TargetingBehaviour behaviour)
    {
        _targetingBehaviour = behaviour;
    }

    protected void UpdateTarget()
    {
        TargetChanged = false;

        if(_targetingBehaviour == TargetingBehaviour.Sticky && HasTarget)
        {
            HasTarget = TargetStillValid();
            if (HasTarget) return;
        }

        switch (_targetingType)
        {
            case TargetingType.First:
                {
                    HasTarget = TargetProvider.Instance.TryGetClosestEnemy(Range, out BaseEnemy result);
                    if (HasTarget)
                    {
                        CurrentTarget = result;
                        if (result.UID != CurrentTargetUID) TargetChanged = true;
                        CurrentTargetUID = CurrentTarget.UID;
                    }
                    break;
                }
            case TargetingType.Last:
                {
                    HasTarget = TargetProvider.Instance.TryGetFurthestEnemy(Range, out BaseEnemy result);
                    Debug.Log(HasTarget);
                    if (HasTarget)
                    {
                        CurrentTarget = result;
                        if (result.UID != CurrentTargetUID) TargetChanged = true;
                        CurrentTargetUID = CurrentTarget.UID;
                    }
                    break;
                }
            case TargetingType.Strongest:
                {
                    HasTarget = TargetProvider.Instance.TryGetStrongestEnemy(Range, out BaseEnemy result);
                    if (HasTarget)
                    {
                        CurrentTarget = result;
                        if (result.UID != CurrentTargetUID) TargetChanged = true;
                        CurrentTargetUID = CurrentTarget.UID;
                    }
                    break;
                }
            case TargetingType.Weakest:
                {
                    HasTarget = TargetProvider.Instance.TryGetWeakestEnemy(Range, out BaseEnemy result);
                    if (HasTarget)
                    {
                        CurrentTarget = result;
                        if (result.UID != CurrentTargetUID) TargetChanged = true;
                        CurrentTargetUID = CurrentTarget.UID;
                    }
                    break;
                }
            default:
                Debug.LogError("Targeting type is not defined!");
                break;
        }
    }

    protected bool TargetStillValid()
    {
        if (!HasTarget) return false;

        if (CurrentTarget.gameObject == null ||
            !CurrentTarget.gameObject.activeSelf ||
            CurrentTarget.UID != CurrentTargetUID ||
            CurrentTarget.CurrentDistanceToFinish > Range)
        {
            return false;
        }
        else return true;
    }

    public void BuildTurret(int turretID, int slotTurretIsBuiltOn, int currentSellValue)
    {
        TurretID = turretID;
        SlotTurretIsBuiltOn = slotTurretIsBuiltOn;
        CurrentSellValue = currentSellValue;
    }

    public virtual void ApplyUpgrade(int index)
    {
        if (Upgrades[index] == null)
        {
            Debug.LogError($"upgrade with index of {index} doesnt exist!");
            return;
        }

        float statValue = Upgrades[index].UpgradeClass[0].AddedStat;
        CurrentSellValue += (int)(Upgrades[index].UpgradeClass[0].Cost * 0.6f);
        UpgradesDict[index].Invoke(statValue);
        Upgrades[index].UpgradeClass.RemoveAt(0);
        PostUpgradeVisualRefresh();
        PopulateUIUpgrades();
    }

    public abstract void PostUpgradeVisualRefresh();

    public void RefreshUpgradesInfo() // next upgrade will show up in upgrade menu and it needs
                                      // to show correct corresponding stat like firerate float value
    {

        for (int i = 0; i < Upgrades.Count; i++)
        {
            if (Upgrades[i].UpgradeClass.Count == 0) continue;
            Upgrades[i].UpgradeClass[0].CurrentStat = UpgradesDict[i].Invoke(null).Value;
        }
        UIManager.Instance.ChangeSellAmountText(CurrentSellValue);
    }

    public void PopulateUIUpgrades(bool newSelected = false)
    {
        UIManager.Instance.RemoveAllUpgradeOptions();
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
        UIManager.Instance.ChangeSellAmountText(CurrentSellValue);
        UIManager.Instance.ActivateUpgradeMenu();
        if (newSelected) UIManager.Instance.PullScrollRectToTopUpgradeMenu();
    }
}
