using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetProvider : MonoBehaviour
{
    [SerializeField] List<BaseEnemy> ActiveEnemies = new();

    List<BaseEnemy> EnemiesByDistanceAscending = new();
    List<BaseEnemy> EnemiesByMaxHealthDescending = new();
    List<BaseEnemy> EnemiesByCurrentHealthAscending = new();

    public static TargetProvider Instance;

    [field: SerializeField] public int EnemiesAlive { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    public void RegisterActiveEnemy(BaseEnemy enemy)
    {
        ActiveEnemies.Add(enemy);
        EnemiesAlive++;
    }

    public void RemoveActiveEnemy(BaseEnemy enemy)
    {
        ActiveEnemies.RemoveAll(x => x == enemy); // need to do this cause of bug adding enemy twice or something
        EnemiesAlive--;
    }

    private void FixedUpdate()
    {
        EnemiesByDistanceAscending = ActiveEnemies.OrderBy(x => x.CurrentDistanceToFinish).ToList();
        EnemiesByMaxHealthDescending = ActiveEnemies.OrderByDescending(x => x.MaxHealth).ToList();
        EnemiesByCurrentHealthAscending = ActiveEnemies.OrderBy(x => x.CurrentHealth).ToList();
    }

    public bool TryGetClosestEnemy(float range, out BaseEnemy result)
    {
        for (int i = 0; i < EnemiesByDistanceAscending.Count; i++)
        {
            BaseEnemy enemy = EnemiesByDistanceAscending[i];
            if (enemy.CurrentDistanceToFinish <= range)
            {
                result = enemy;
                return true;
            }
        }
        result = default;
        return false;
    }

    public bool TryGetFurthestEnemy(float range, out BaseEnemy result)
    {
        for(int i = EnemiesByDistanceAscending.Count - 1; i > 0; i--)
        {
            BaseEnemy enemy = EnemiesByDistanceAscending[i];
            if(enemy.CurrentDistanceToFinish <= range)
            {
                result = enemy;
                return true;
            }
        }
        result = default;
        return false;
    }

    public bool TryGetStrongestEnemy(float range, out BaseEnemy result)
    {
        for (int i = 0; i < EnemiesByMaxHealthDescending.Count; i++)
        {
            BaseEnemy enemy = EnemiesByMaxHealthDescending[i];
            if (enemy.CurrentDistanceToFinish <= range)
            {
                result = enemy;
                return true;
            }
        }
        result = default;
        return false;
    }

    public bool TryGetWeakestEnemy(float range, out BaseEnemy result)
    {
        for (int i = 0; i < EnemiesByCurrentHealthAscending.Count; i++)
        {
            BaseEnemy enemy = EnemiesByCurrentHealthAscending[i];
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
