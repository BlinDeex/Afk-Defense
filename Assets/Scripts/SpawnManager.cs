using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject _enemyPrefab;
    [SerializeField] GameObject _spawnPoint;
    [SerializeField] int _cooldownInTicks;
    [SerializeField] float _health;
    [SerializeField] float _movingPower;

    int enemyUID = 1;

    [Serializable]
    public class EnemyInfo
    {
        public GameObject Prefab;
        public int Cost;
    }

    [SerializeField] EnemyInfo[] _basicEnemyList;
    [SerializeField] EnemyInfo[] _mediumEnemyList;
    [SerializeField] EnemyInfo[] _bossEnemyList;
    [SerializeField] EnemyInfo[] _specialEnemyList;


    [Header("Spawner Character")]
    [Range(0.0f, 1.0f)]
    [Tooltip("0 = extreme favor towards saving points, 1 = extreme favor towards spending them immediately")]
    [SerializeField] float _aggressivenessIndex;
    
    [Range(1, 100)]
    [Tooltip("How often spawner takes action")]
    [SerializeField] int _actionRate;
    
    [Range(0, 500)]
    [Tooltip("Maximum amount of passed time without any enemy spawns")]
    [SerializeField] float _maxTicksWithNoSpawn;
    float _currentTicksWithNoSpawn;
    
    [Range(0, 100)]
    [Tooltip("Minimum amount of basic enemies spawned at once")]
    [SerializeField] int _minimumBasicEnemySpawn;
    
    [Range(0, 100)]
    [Tooltip("Maximum amount of basic enemies spawned at once")]
    [SerializeField] int _maximumBasicEnemySpawn;

    [Range(0, 500)]
    [Tooltip("Maximum amount of enemies currently alive")]
    [SerializeField] int _maximumEnemies;

    [Header("Multiplier impacting chance and amount of enemies spawned \n" +
        " made for dynamic difficulty depending on how player is doing")]
    [Tooltip("Should spawning take in consideration current enemy count?")]
    [SerializeField] bool _spawnAccordingToEnemiesAlive;
    
    [Range(0, 500)]
    [Tooltip("Enemy count at which spawning multiplier maxes out")]
    [SerializeField] int _minEnemyCount;

    [Range(0, 500)]
    [Tooltip("Enemy count at which spawning multiplier is at its minimum")]
    [SerializeField] int _maxEnemyCount;

    [Range(0f, 3f)]
    [Tooltip("How much multiplier should increase/reduce spawns when there are minimum amount of enemies alive")]
    [SerializeField] float _minGlobalMultiplier;

    [Range(0f, 3f)]
    [Tooltip("How much multiplier should increase/reduce spawns when there are maximum amount of enemies alive")]
    [SerializeField] float _maxGlobalMultiplier;

    [Header("Enable/Disable particular type of enemies")]
    [SerializeField] bool _enableBasicEnemies;
    [SerializeField] bool _enableMediumEnemies;
    [SerializeField] bool _enableBossEnemies;
    [SerializeField] bool _enableSpecialEnemies;

    float _timeForNextPointGain;
    int _currentWavePointGainStep = 0;
    int _currentWave = 0;
    bool _lastWave = false;
    int _tickElapsed;

    

    [Header("Temporary debug values")]
    [SerializeField] float _currentGlobalMultiplier;
    [SerializeField] int _basicPointsBank;
    [SerializeField] int _mediumPointsBank;
    [SerializeField] int _bossPointsBank;
    [SerializeField] int _specialPointsBank;
    [SerializeField] bool _wavesFinished = false;

    [Serializable]
    class PointGainInfo
    {
        public int buggedField; // reverted
        public int BasicPointsGained;
        public int MediumPointsGained;
        public int BossPointsGained;
        public int SpecialPointsGained;

        public float TimeInSecondsForNextGain;
    }

    [Serializable]
    class WaveInfo
    {
        public List<PointGainInfo> PointsGainInfoThisWave;
        public int GoldGainedForWave;
    }

    [SerializeField] List<WaveInfo> _wavesInfo;

    private void Awake()
    {
        

        if (_wavesInfo.Count == 0) _wavesFinished = true;
        if (_wavesInfo.Count == 1) _lastWave = true;

        _timeForNextPointGain = Time.time; // first gain is instant
    }

    bool ContainsAny(string[] stringsToContain, string value)
    {
        return stringsToContain.Any(value.Contains);
    }

    private void Start()
    {
        UIManager.Instance.UpdateWaveText(_currentWave + 1, _wavesInfo.Count);
    }

    private void FixedUpdate()
    {
        _tickElapsed++;
        _currentTicksWithNoSpawn++;

        if (_spawnAccordingToEnemiesAlive)
        {
            int enemiesAlive = TargetProvider.Instance.EnemiesAlive;
            float lerper = Mathf.InverseLerp(_minEnemyCount, _maxEnemyCount, enemiesAlive);
            _currentGlobalMultiplier = Mathf.Lerp(_maxGlobalMultiplier, _minGlobalMultiplier, lerper);
        }
        else
        {
            _currentGlobalMultiplier = 1;
        }

        if (!_wavesFinished) UpdatePoints();


        if (_tickElapsed % _actionRate == 0)
        {
            if(_enableBasicEnemies)
            TrySpawnEnemyFromList(ref _basicEnemyList, ref _basicPointsBank);
            if(_enableMediumEnemies)
            TrySpawnEnemyFromList(ref _mediumEnemyList, ref _mediumPointsBank);
            if(_enableBossEnemies)
            TrySpawnEnemyFromList(ref _bossEnemyList, ref _bossPointsBank);
            if(_enableSpecialEnemies)
            TrySpawnEnemyFromList(ref _specialEnemyList, ref _specialPointsBank);
        }
    }

    void TrySpawnEnemyFromList(ref EnemyInfo[] enemyList, ref int bank)
    {
        if (TargetProvider.Instance.EnemiesAlive >= _maximumEnemies) return;

        List<EnemyInfo> affordableEnemies = new();

        foreach(EnemyInfo enemy in enemyList)
        {
            if (enemy.Cost <= bank) affordableEnemies.Add(enemy);
        }

        //EnemyInfo[] affordableEnemies = enemyList.Where(x => x.Cost <= bank).ToArray();

        if (affordableEnemies.Count == 0) return;

        float chanceToSpawn;

        if(_currentTicksWithNoSpawn >= _maxTicksWithNoSpawn)
        {
            chanceToSpawn = 1f;
        }
        else
        {
            float baseChance = UnityEngine.Random.Range(0f, 1f);
            float agressivenessModifier = 0.5f - _aggressivenessIndex;
            chanceToSpawn = (baseChance - agressivenessModifier) * _currentGlobalMultiplier;
        }

        float dice = UnityEngine.Random.Range(0f, 1f);

        if (dice > chanceToSpawn) return;

        int enemiesToSpawn = UnityEngine.Random.Range(_minimumBasicEnemySpawn, _maximumBasicEnemySpawn + 1);
        enemiesToSpawn = Mathf.RoundToInt(enemiesToSpawn * _currentGlobalMultiplier);

        if(enemiesToSpawn > 0) _currentTicksWithNoSpawn = 0;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            EnemyInfo randomAffordableEnemy =
                affordableEnemies[UnityEngine.Random.Range(0, affordableEnemies.Count)];

            SpawnEnemy(randomAffordableEnemy.Prefab);

            bank -= randomAffordableEnemy.Cost;

            affordableEnemies = new();

            foreach (EnemyInfo enemy in enemyList)
            {
                if (enemy.Cost <= bank) affordableEnemies.Add(enemy);
            }

            if (affordableEnemies.Count == 0) break;

            if (TargetProvider.Instance.EnemiesAlive >= _maximumEnemies) break;
        }
    }

    void SpawnEnemy(GameObject basicEnemy)
    {
        GameObject enemy = DynamicObjectPooler.Instance.RequestEnemy(basicEnemy);
        BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();
        baseEnemy.PrepareEnemy(_health, _movingPower, enemyUID);
        enemyUID++;
        enemy.transform.position = _spawnPoint.transform.position + new Vector3(UnityEngine.Random.Range(-4f, 4f), 0, 0);
        enemy.SetActive(true);
        TargetProvider.Instance.RegisterActiveEnemy(baseEnemy);
    }

    void UpdatePoints()
    {
        if (_timeForNextPointGain >= Time.time) return;
        
        if (_currentWavePointGainStep < _wavesInfo[_currentWave].PointsGainInfoThisWave.Count)
        {
            PointGainInfo gainInfo = GetPointGainInfo(_currentWave, _currentWavePointGainStep);
            _basicPointsBank += gainInfo.BasicPointsGained;
            _mediumPointsBank += gainInfo.MediumPointsGained;
            _bossPointsBank += gainInfo.BossPointsGained;
            _specialPointsBank += gainInfo.SpecialPointsGained;
            _timeForNextPointGain = Time.time + GetPointGainInfo(_currentWave, _currentWavePointGainStep).TimeInSecondsForNextGain;
            
            _currentWavePointGainStep++;

            if(_currentWavePointGainStep == _wavesInfo[_currentWave].PointsGainInfoThisWave.Count)
            {
                if(!_lastWave)
                UIManager.Instance.EngageNextWaveTimer(GetPointGainInfo(_currentWave, _currentWavePointGainStep - 1).TimeInSecondsForNextGain);
            }
        }
        else
        {
            if (_lastWave)
            {
                _wavesFinished = true;
            }
            else
            {
                NextWave();
            }
        }
    }

    void NextWave()
    {
        AddGoldForWave();
        _currentWave++;
        
        if (_currentWave == _wavesInfo.Count - 1) _lastWave = true;
        UIManager.Instance.UpdateWaveText(_currentWave + 1, _wavesInfo.Count);
        _currentWavePointGainStep = 0;
    }

    void AddGoldForWave()
    {
        CurrencyManager.Instance.AddGold(_wavesInfo[_currentWave].GoldGainedForWave);
    }

    PointGainInfo GetPointGainInfo(int currentWave, int currentGainStep)
    {
        return _wavesInfo[currentWave].PointsGainInfoThisWave[currentGainStep];
    }
}
