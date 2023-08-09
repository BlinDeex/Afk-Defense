using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EmptySlotsManager : MonoBehaviour
{
    public static EmptySlotsManager Instance;

    [Serializable]
    class Slot
    {
        public GameObject SlotGO;
        public int Index;
        public bool Enabled;
        public Slot(int index, GameObject slotGO)
        {
            Index = index;
            SlotGO = slotGO;
            Enabled = true;
        }
    }

    [SerializeField] List<Slot> _slotsList = new();

    private void Awake()
    {
        Instance = this;

        RegisterSlots();
    }

    public Transform ReturnSlotPosition(int index)
    {
        return _slotsList.Where(x => x.Index == index).First().SlotGO.transform;
    }

    void RegisterSlots()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            EmptySlot slot = transform.GetChild(i).GetComponent<EmptySlot>();
            _slotsList.Add(new Slot(slot.SlotID, slot.gameObject));
        }
    }

    public void DisableEmptySlot(int index)
    {
        Slot slot = _slotsList.Where(x => x.Index == index).First();
        slot.SlotGO.SetActive(false);
        slot.Enabled = false;
    }

    public void EnableEmptySlot(int index)
    {
        Slot slot = _slotsList.Where(x => x.Index == index).First();
        slot.SlotGO.SetActive(true);
        slot.Enabled = true;
    }
}
