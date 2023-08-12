using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetProvider : MonoBehaviour
{
    [SerializeField] List<BaseEnemy> ActiveEnemies = new();

    List<BaseEnemy> SortedEnemiesByDistance = new();

    public static TargetProvider Instance;

    [field: SerializeField] public int EnemiesAlive { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    public void RegisterActiveEnemy(BaseEnemy enemy)
    {
        ActiveEnemies.Add(enemy);
        Debug.Log($"registering {enemy.UID}");
        EnemiesAlive++;
    }

    public void RemoveActiveEnemy(BaseEnemy enemy)
    {
        Debug.Log($"removing {enemy.UID} enemy");
        ActiveEnemies.RemoveAll(x => x == enemy); // need to do this cause of bug adding enemy twice or something
        EnemiesAlive--;
    }

    private void FixedUpdate()
    {
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
