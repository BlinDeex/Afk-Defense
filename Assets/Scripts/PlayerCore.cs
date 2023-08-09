using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCore : MonoBehaviour
{


    int _selectedEmptySlot;
    BaseTurret _selectedTurret;

    public static PlayerCore Instance;

    [System.Serializable]
    public class EquippedTurrets
    {
        public string Name;
        public string Tier;
        public int Cost;
        public GameObject TurretGO;
        public bool IsPlaced;
        public byte TurretIndex;
        public GameObject TurretUIModel;
    }

    [SerializeField] public List<EquippedTurrets> EquippedTurretsList;

    private void Awake()
    {
        Instance = this;
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
                    bool isNew = false;
                    if (_selectedTurret == null) isNew = true;
                    if (!isNew) isNew = _selectedTurret != BT;
                    if (isNew) _selectedTurret = BT;

                    BT.PopulateUIUpgrades(isNew);
                    return;
                }
                if (hit.collider.CompareTag("Leader"))
                {

                    return;
                }
                if (hit.collider.CompareTag("EmptySlot"))
                {
                    _selectedEmptySlot = hit.collider.gameObject.GetComponent<EmptySlot>().SlotID;
                    AssembleBuildMenu();
                    return;
                }
            }
        }
    }

    public void UpgradeTurret(byte statIndex)
    {
        _selectedTurret.GetComponent<BaseTurret>().ApplyUpgrade(statIndex);
    }

    public void AssembleBuildMenu()
    {
        UIManager.Instance.RemoveAllLeftPanelElements();
        foreach (EquippedTurrets turret in EquippedTurretsList)
        {
            if (turret.IsPlaced) continue;

            UIManager.Instance.AddBuildOption(turret.Name,
                turret.Tier,
                turret.Cost,
                turret.TurretIndex);
        }

        UIManager.Instance.PullScrollRectToTop();
        UIManager.Instance.ActivateMainUIPanel();
    }

    public void BuildTurret(byte index)
    {
        EquippedTurrets turretInfo = EquippedTurretsList.Where(x => x.TurretIndex == index).First();
        turretInfo.IsPlaced = true;
        Transform slotTransform = EmptySlotsManager.Instance.ReturnSlotPosition(_selectedEmptySlot);
        GameObject newTurret = Instantiate(turretInfo.TurretGO, slotTransform.position, Quaternion.identity);
        newTurret.GetComponent<BaseTurret>().TurretID = index;
        newTurret.GetComponent<BaseTurret>().SlotTurretIsBuiltOn = _selectedEmptySlot;
        UIManager.Instance.DeactivateMainUIPanel();
        EmptySlotsManager.Instance.DisableEmptySlot(_selectedEmptySlot);
    }

    public void SellTurret()
    {
        BaseTurret TB = _selectedTurret;
        int equippedTurretIndex = TB.TurretID;
        int goldValue = TB.TotalSellValue;
        CurrencyManager.Instance.AddGold(goldValue);
        EmptySlotsManager.Instance.EnableEmptySlot(TB.SlotTurretIsBuiltOn);
        UIManager.Instance.DeactivateMainUIPanel();
        EquippedTurretsList.Where(x => x.TurretIndex == equippedTurretIndex).First().IsPlaced = false;
        Destroy(_selectedTurret);
    }
}
