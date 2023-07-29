using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BaseTurret : MonoBehaviour
{
    [Serializable]
    public class UpgradeTier
    {
        public string UpgradeName;
        public int UpgradeIndex;
        public float UpgradeAddedStat;
        public float UpgradeInitialStat;
        public string UpgradeDescription;
    }

    [Serializable]
    public class UpgradeClassList
    {
        public List<UpgradeTier> UpgradeClass;
    }

    public List<UpgradeClassList> Upgrades;
}
