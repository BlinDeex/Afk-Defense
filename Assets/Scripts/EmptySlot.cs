using UnityEngine;

public class EmptySlot : MonoBehaviour
{
    [field: SerializeField] public int SlotID { get; private set; }
    [field: SerializeField] public SlotType SlotType { get; private set; }
}
