using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [SerializeField] TextMeshProUGUI _goldText;
    [SerializeField] TextMeshProUGUI _diamondText;
    


    public int GoldAmount { get; private set; }

    public int DiamondsAmount { get; private set; }

    [SerializeField] int _startingGold;
    [SerializeField] int _startingDiamonds;

    private void Awake()
    {
        Instance = this;
        GoldAmount += _startingGold;
        DiamondsAmount += _startingDiamonds;
        _goldText.text = GoldAmount.ToString();
        _diamondText.text = DiamondsAmount.ToString();
    }
    

    public void AddGold(int amount)
    {
        GoldAmount += amount;
        UpdateTexts();
    }

    

    public void AddDiamonds(int amount)
    {
        DiamondsAmount += amount;
        UpdateTexts();
    }

    public bool TryRemoveGold(int amount)
    {
        if (amount > GoldAmount) return false;

        GoldAmount -= amount;

        return true;
    }

    void UpdateTexts()
    {
        _goldText.text = GoldAmount.ToString();
        _diamondText.text = DiamondsAmount.ToString();
    }

    public bool TryRemoveDiamonds(int amount)
    {
        if (amount > DiamondsAmount) return false;

        DiamondsAmount -= amount;

        return true;
    }
}
