using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetProvider : MonoBehaviour
{
    [SerializeField] List<BaseEnemy> ActiveEnemies = new();

    List<BaseEnemy> SortedEnemiesByDistance = new();

    public static TargetProvider Instance;

    [SerializeField] int _smalledUIDAlive;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterActiveEnemy(BaseEnemy enemy)
    {
        ActiveEnemies.Add(enemy);
    }

    public void RemoveActiveEnemy(BaseEnemy enemy)
    {
        ActiveEnemies.Remove(enemy);
    }

    private void FixedUpdate()
    {
        if(ActiveEnemies.Count > 0)
        _smalledUIDAlive = ActiveEnemies.Min(x => x.UID);

        SortedEnemiesByDistance = ActiveEnemies.OrderBy(x => x.CurrentDistanceToFinish).ToList();
    }

    public bool TryGetClosestEnemy(float range, out BaseEnemy result)
    {
        for (int i = 0; i < SortedEnemiesByDistance.Count; i++)
        {
            BaseEnemy enemy = SortedEnemiesByDistance[i];
            if (enemy.CurrentDistanceToFinish <= range)
            {
                result = enemy;
                return true;
            }
        }
        result = default;
        return false;
    }
}
