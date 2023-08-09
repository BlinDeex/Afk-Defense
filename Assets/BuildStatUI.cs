using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildStatUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _turretNameTM;
    [SerializeField] TextMeshProUGUI _tierTM;
    [SerializeField] TextMeshProUGUI _costTM;
    [SerializeField] GameObject _turretModelRoot;
    byte _turretIndex;
    int _cost;
    GameObject _correspondingTurretUIModel;

    public void AssembleBuildPanel(string turretName, string turretTier, int cost, byte turretIndex)
    {
        _turretNameTM.text = turretName;
        _tierTM.text = turretTier;
        _costTM.text = cost.ToString();
        _turretIndex = turretIndex;
        _cost = cost;

        GameObject correspondingTurretUIModel = PlayerCore.Instance.ProvideTurretUIModel(turretIndex);
        Instantiate(correspondingTurretUIModel, _turretModelRoot.transform);
    }

    public void TryBuyTurret()
    {
        bool hasEnoughCoins = CurrencyManager.Instance.TryRemoveGold(_cost);
        if (hasEnoughCoins)
        {
            PlayerCore.Instance.BuildTurret(_turretIndex);
        }
    }
}
