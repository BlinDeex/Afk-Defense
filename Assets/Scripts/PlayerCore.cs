using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PlayerCore : MonoBehaviour
{
    [SerializeField] GameObject _selectionPrefab;

    GameObject _currentInnerSelectionEffect;

    [SerializeField] EmptySlot _selectedEmptySlot;

    public float OffensiveSlotDamageMultiplier { get; private set; }

    public float DefensiveSlotDamageMultiplier { get; private set; }

    BaseTurret _selectedTurret;

    public static PlayerCore Instance;

    [SerializeField] bool _infiniteTurrets;

    [Serializable]
    public class EquippedTurrets
    {
        public string Name;
        public TurretTierEnum Tier;
        public int Cost;
        public GameObject TurretGO;
        public bool IsPlaced;
        [NonSerialized] public byte TurretIndex;
        public GameObject TurretUIModel;
        public string Description;
    }

    [SerializeField] public List<EquippedTurrets> EquippedTurretsList;

    public List<BaseTurret> BuiltTurrets = new();

    private void Awake()
    {
        Instance = this;
        for(byte i = 0; i < EquippedTurretsList.Count; i++)
        {
            EquippedTurretsList[i].TurretIndex = i;
        }
    }

    void Update()
    {
        CheckPCInput();
    }

    public GameObject ProvideTurretUIModel(int index) => EquippedTurretsList.Where(x => x.TurretIndex == index).First().TurretUIModel;

    void CheckPCInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector3.forward);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.CompareTag("Turret"))
                {
                    BaseTurret BT = hit.collider.GetComponent<BaseTurret>();
                    Debug.Log("clicked on turret");
                    bool isNew = false;
                    if (_selectedTurret == null) isNew = true;
                    if (!isNew) isNew = _selectedTurret != BT;
                    if (isNew) _selectedTurret = BT;

                    BT.TurretSelected(isNew);
                    return;
                }
                if (hit.collider.CompareTag("Leader"))
                {

                    return;
                }
                if (hit.collider.CompareTag("EmptySlot"))
                {
                    Debug.Log(hit.collider.name + " empty slot was clicked");
                    EmptySlot slot = hit.collider.gameObject.GetComponent<EmptySlot>();
                    _selectedEmptySlot = slot;
                    
                    AssembleBuildMenu();
                    EnableSelectionEffect();
                    return;
                }
            }
        }
    }

    public void UpgradeTurret(byte statIndex)
    {
        _selectedTurret.GetComponent<BaseTurret>().ApplyUpgrade(statIndex);
    }

    void EnableSelectionEffect()
    {
        Transform slotT = _selectedEmptySlot.transform;
        GameObject innerSelectionGO = Instantiate(_selectionPrefab, slotT.transform.position + new Vector3(0,0.3f,0), Quaternion.identity);
        innerSelectionGO.GetComponent<SelectionTrailRotation>().PrepareEffect(0.3f, -360, 0, slotT);
        _currentInnerSelectionEffect = innerSelectionGO;
    }

    public void DisableSelectionEffect()
    {
        if (_currentInnerSelectionEffect != null)
        _currentInnerSelectionEffect.GetComponent<SelectionTrailRotation>().IsActive = false;
    }

    public void AssembleBuildMenu()
    {
        UIManager.Instance.RemoveAllBuildOptions();
        foreach (EquippedTurrets turret in EquippedTurretsList)
        {
            if (turret.IsPlaced) continue;

            UIManager.Instance.AddBuildOption(turret.Name,
                turret.Tier,
                turret.Cost,
                turret.TurretIndex);
        }

        UIManager.Instance.PullScrollRectToTopBuildMenu();
        UIManager.Instance.ActivateBuildMenu();
    }

    public void ChangeTurretTargetingType() => _selectedTurret.ChangeTargetingType();

    public void ChangeTurretTargetingBehaviour() => _selectedTurret.ChangeTargetingBehaviour();

    public void BuildTurret(byte index)
    {
        EquippedTurrets turretInfo = EquippedTurretsList.Where(x => x.TurretIndex == index).First();
        if(!_infiniteTurrets)
        turretInfo.IsPlaced = true;
        Transform slotTransform = EmptySlotsManager.Instance.ReturnSlotPosition(_selectedEmptySlot.SlotID);
        GameObject newTurret = Instantiate(turretInfo.TurretGO, slotTransform.position, Quaternion.identity);
        BaseTurret BT = newTurret.GetComponent<BaseTurret>();
        BT.BuildTurret(index, _selectedEmptySlot, turretInfo.Cost);
        BuiltTurrets.Add(BT);
        UIManager.Instance.DeactivateBuildMenu();
        EmptySlotsManager.Instance.DisableEmptySlot(_selectedEmptySlot.SlotID);
    }

    public void SellTurret()
    {
        BaseTurret TB = _selectedTurret;
        int equippedTurretIndex = TB.TurretID;
        int goldValue = TB.CurrentSellValue;
        CurrencyManager.Instance.AddGold(goldValue);
        EmptySlotsManager.Instance.EnableEmptySlot(TB.SlotID);
        UIManager.Instance.DeactivateUpgradeMenu();
        EquippedTurretsList.Where(x => x.TurretIndex == equippedTurretIndex).First().IsPlaced = false;
        BuiltTurrets.Remove(_selectedTurret);
        Destroy(_selectedTurret.gameObject);
    }
}
