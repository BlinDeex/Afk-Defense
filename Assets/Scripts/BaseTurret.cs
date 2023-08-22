using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    public int SlotID { get; protected set; }
    public SlotType SlotType { get; protected set; }
    public int CurrentSellValue { get; protected set; }

    [SerializeField] TargetingType _targetingType;
    [SerializeField] TargetingBehaviour _targetingBehaviour;
    [SerializeField] TurretType _turretType;

    public BaseEnemy CurrentTarget { get; protected set; }
    public int CurrentTargetUID { get; protected set; }
    private bool _lastFrameHasTarget { get; set; }
    public bool HasTarget { get; protected set; }

    public bool JustLostTarget { get; protected set; }

    [field: SerializeField] public bool HasTargeting { get; protected set; }

    [field: SerializeField] public bool RangeBuffSensitive { get; protected set; }
    [field: SerializeField] public bool DamageBuffSensitive { get; protected set; }
    [field: SerializeField] public bool FirerateBuffSensitive { get; protected set; }

    /// <summary>
    /// true only for first update with new target
    /// </summary>
    public bool TargetChanged { get; protected set; }

    [field: SerializeField] public float Range { get; protected set; }

    [field: SerializeField] public float RangeMultiplier { get; protected set; } = 1f;
    [field: SerializeField] public float DamageMultiplier { get; protected set; } = 1f;
    [field: SerializeField] public float FirerateMultiplier { get; protected set; } = 1f;

    private class Effect
    {
        private bool _active;
        
        public bool Active
        {
            get { return _active; }
            set
            {
                if (value == true)
                {
                    OnApply?.Invoke();
                    if (!_active) OnStarted?.Invoke();
                }
                _active = value;
            }
        }

        private int _ticksLeft;
        public int TicksLeft
        {
            get { return _ticksLeft; }
            set
            {
                _ticksLeft = value;
                if (value <= 0) OnEnded?.Invoke();
            }
        }
        public float Strength;
        public float RangeAddition;
        public float DamageAddition;
        public float FirerateAddition;

        public Action OnStarted { get; private set; } // first tick when effect wasnt active but just got applied
        public Action OnApply { get; private set; } // first tick when effect was active and got applied again
        public Action OnEnded { get; private set; } // first tick when effect ran out

        public Action Tick { get; private set; } // effect logic

        public Effect(Action tick, [Optional] Action onApply, [Optional] Action onEnded, [Optional] Action onStarted)
        {
            Tick = tick;

            if(onApply != null) OnApply = onApply;

            if (onEnded != null) OnEnded = onEnded;

            if (onStarted != null) OnStarted = onStarted;
        }
    }

    /// <summary>
    /// must run first in Awake method
    /// </summary>
    protected void BaseEarlyAwake()
    {
        _effectsDict.Add(TurretEffect.AquaPulseBuffer, new(NeonPulseBuffer, NeonPulseBufferOnApply, NeonPulseBufferOnEnded, NeonPulseBufferOnStarted));
    }

    protected float CalculateFinalDamage(float baseDamage)
    {
        switch (SlotType)
        {
            case SlotType.Defensive:
                return baseDamage * DamageMultiplier * PlayerCore.Instance.DefensiveSlotDamageMultiplier;
            case SlotType.Offensive:
                return baseDamage * DamageMultiplier * PlayerCore.Instance.OffensiveSlotDamageMultiplier;
            case SlotType.Leader:
                return baseDamage * DamageMultiplier;
            default:
                Debug.LogError($"Slot type of {SlotType} is not defined!");
                return -1;
        }
    }

    protected float CalculateFinalRange(float baseRange)
    {
        return baseRange * RangeMultiplier;
    }

    /// <summary>
    /// must run first in FixedUpdate method
    /// </summary>
    protected void BaseEarlyFixedUpdate()
    {
        RangeMultiplier = DamageMultiplier = FirerateMultiplier = 1f;

        RunEffects();
    }

    /// <summary>
    /// must run last in FixedUpdate method
    /// </summary>
    protected void BaseLateFixedUpdate()
    {

    }

    private Dictionary<TurretEffect, Effect> _effectsDict = new();

    public void ApplyEffect(TurretEffect appliedEffect, int ticksLeft,float strength, ParticleSystem effectPS = null , int emissionRate = 0)
    {
        Effect effect = _effectsDict[appliedEffect];

        effect.Strength = strength;
        effect.Active = true;
        effect.TicksLeft = ticksLeft;

        if(effectPS != null)
        {

        }
    }

    private void RunEffects()
    {
        foreach (Effect effect in _effectsDict.Values)
        {
            if(effect.Active) effect.Tick.Invoke();
        }
    }

    private void NeonPulseBuffer()
    {
        GeneralTick(_effectsDict[TurretEffect.AquaPulseBuffer]);
    }

    private void NeonPulseBufferOnApply()
    {
        Effect effect = _effectsDict[TurretEffect.AquaPulseBuffer];
        effect.RangeAddition = effect.Strength;
    }

    private void NeonPulseBufferOnStarted()
    {
        Debug.Log("On Started");
    }

    private void NeonPulseBufferOnEnded()
    {
        Debug.Log("OnEnded");
    }

    private void GeneralTick(Effect effect)
    {
        if (effect.TicksLeft <= 0)
        {
            effect.Active = false;
            return;
        }

        Debug.Log("tick");

        if(RangeBuffSensitive) RangeMultiplier += effect.RangeAddition;
        if(DamageBuffSensitive) DamageMultiplier += effect.DamageAddition;
        if(FirerateBuffSensitive) FirerateMultiplier += effect.FirerateAddition;

        effect.TicksLeft--;
    }

    public void ChangeTargetingType()
    {
        _targetingType = Helpers.EnumNextValue(_targetingType);

        UpdateTurretTargetingTexts();
    }

    public void ChangeTargetingBehaviour()
    {
        _targetingBehaviour = Helpers.EnumNextValue(_targetingBehaviour);

        UpdateTurretTargetingTexts();
    }

    protected void UpdateTurretTargetingTexts() =>
        UIManager.Instance.UpdateTurretTargetingTexts(_targetingType, _targetingBehaviour);

    protected void UpdateTarget()
    {
        TargetChanged = false;
        JustLostTarget = false;
        
        if(_targetingBehaviour == TargetingBehaviour.Sticky && HasTarget)
        {
            HasTarget = TargetStillValid();
            if (HasTarget) return;
        }

        switch (_targetingType)
        {
            case TargetingType.First:
                {
                    HasTarget = TargetProvider.Instance.TryGetClosestEnemy(CalculateFinalRange(Range), out BaseEnemy result);
                    if (HasTarget)
                    {
                        UpdateTargetInfo(result);
                    }
                    else if (!HasTarget && _lastFrameHasTarget) JustLostTarget = true;
                    break;
                }
            case TargetingType.Last:
                {
                    HasTarget = TargetProvider.Instance.TryGetFurthestEnemy(CalculateFinalRange(Range), out BaseEnemy result);
                    if (HasTarget)
                    {
                        UpdateTargetInfo(result);
                    }
                    else if (!HasTarget && _lastFrameHasTarget) JustLostTarget = true;
                    break;
                }
            case TargetingType.Strongest:
                {
                    HasTarget = TargetProvider.Instance.TryGetStrongestEnemy(CalculateFinalRange(Range), out BaseEnemy result);
                    if (HasTarget)
                    {
                        UpdateTargetInfo(result);
                    }
                    else if (!HasTarget && _lastFrameHasTarget) JustLostTarget = true;
                    break;
                }
            case TargetingType.Weakest:
                {
                    HasTarget = TargetProvider.Instance.TryGetWeakestEnemy(CalculateFinalRange(Range), out BaseEnemy result);
                    if (HasTarget)
                    {
                        UpdateTargetInfo(result);
                    }
                    else if (!HasTarget && _lastFrameHasTarget) JustLostTarget = true;
                    break;
                }
            default:
                Debug.LogError($"targeting type of {_targetingType} is not defined!");
                break;
        }

        _lastFrameHasTarget = HasTarget;
    }

    private void UpdateTargetInfo(BaseEnemy result)
    {
        CurrentTarget = result;
        if (result.UID != CurrentTargetUID && _lastFrameHasTarget) TargetChanged = true;
        CurrentTargetUID = CurrentTarget.UID;
    }

    protected bool TargetStillValid()
    {
        if (!HasTarget) return false;

        if (CurrentTarget.gameObject == null) return false;
        if (!CurrentTarget.gameObject.activeSelf) return false;
        if (CurrentTarget.UID != CurrentTargetUID) return false;
        if (CurrentTarget.CurrentDistanceToFinish > CalculateFinalRange(Range)) return false;

        return true;
    }

    public void BuildTurret(int turretID, EmptySlot slot, int currentSellValue)
    {
        TurretID = turretID;
        SlotID = slot.SlotID;
        SlotType = slot.SlotType;
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
    {                                 // to show correct corresponding stat like firerate float value
        for (int i = 0; i < Upgrades.Count; i++)
        {
            if (Upgrades[i].UpgradeClass.Count == 0) continue;
            Upgrades[i].UpgradeClass[0].CurrentStat = UpgradesDict[i].Invoke(null).Value;
        }
        UIManager.Instance.ChangeSellAmountText(CurrentSellValue);
    }

    public void TurretSelected(bool newSelected = false)
    {
        if(HasTargeting)
        UIManager.Instance.EnableRangeIndicator(CalculateFinalRange(Range));

        PopulateUIUpgrades(newSelected);
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
        UpdateTurretTargetingTexts();
        UIManager.Instance.ChangeSellAmountText(CurrentSellValue);
        UIManager.Instance.ActivateUpgradeMenu(HasTargeting);
        if (newSelected) UIManager.Instance.PullScrollRectToTopUpgradeMenu();
    }
}
