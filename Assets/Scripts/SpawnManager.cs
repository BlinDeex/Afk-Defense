using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    [SerializeField] EnemyInfo[] _strongEnemyList;
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

    [SerializeField] int _basicPointsBank;
    [SerializeField] int _mediumPointsBank;

    float _timeForNextPointGain;
    int _currentWavePointGainStep = 0;
    int _currentWave = 0;
    bool _lastWave = false;
    int _tickElapsed;

    [SerializeField] bool _wavesFinished = false;

    [Serializable]
    class PointGainInfo
    {
        public int buggedField; // reverted
        public int BasicPointsGained;
        public int MediumPointsGained;
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

    private void Start()
    {
        UIManager.Instance.UpdateWaveText(_currentWave + 1, _wavesInfo.Count);
    }

    private void FixedUpdate()
    {
        _tickElapsed++;
        _currentTicksWithNoSpawn++;

        if(!_wavesFinished) UpdatePoints();


        if (_tickElapsed % _actionRate == 0)
        {
            BasicEnemiesSpawns();
        }
    }

    void BasicEnemiesSpawns()
    {
        EnemyInfo[] affordableBasicEnemies = _basicEnemyList.Where(x => x.Cost <= _basicPointsBank).ToArray();

        if (affordableBasicEnemies.Length == 0) return;

        float chanceToSpawn = 0f;

        if(_currentTicksWithNoSpawn >= _maxTicksWithNoSpawn)
        {
            chanceToSpawn = 1f;
        }
        else
        {
            float baseChance = UnityEngine.Random.Range(0f, 1f);
            float agressivenessModifier = 0.5f - _aggressivenessIndex;
            chanceToSpawn = Mathf.Clamp(baseChance - agressivenessModifier, 0f, 1f);
        }

        float dice = UnityEngine.Random.Range(0f, 1f);
        if (dice > chanceToSpawn) return;

        int enemiesToSpawn = UnityEngine.Random.Range(_minimumBasicEnemySpawn, _maximumBasicEnemySpawn + 1);

        for(int i = 0; i < enemiesToSpawn; i++)
        {
            EnemyInfo randomAffordableEnemy =
                affordableBasicEnemies[UnityEngine.Random.Range(0, affordableBasicEnemies.Length)];

            SpawnBasicEnemy(randomAffordableEnemy.Prefab);
            _currentTicksWithNoSpawn = 0;

            _basicPointsBank -= randomAffordableEnemy.Cost;

            affordableBasicEnemies = _basicEnemyList.Where(x => x.Cost <= _basicPointsBank).ToArray();

            if (affordableBasicEnemies.Length == 0) break;
        }
    }

    void UpdatePoints()
    {
        if (_timeForNextPointGain >= Time.time) return;

        if (_currentWavePointGainStep < _wavesInfo[_currentWave].PointsGainInfoThisWave.Count)
        {
            PointGainInfo gainInfo = GetPointGainInfo(_currentWave, _currentWavePointGainStep);
            _basicPointsBank += gainInfo.BasicPointsGained;
            _mediumPointsBank += gainInfo.MediumPointsGained;
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

    void SpawnBasicEnemy(GameObject basicEnemy)
    {
        GameObject enemy = DynamicObjectPooler.Instance.RequestEnemy(basicEnemy);
        BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();
        baseEnemy.PrepareEnemy(_health, _movingPower, enemyUID);
        enemyUID++;
        enemy.transform.position = _spawnPoint.transform.position + new Vector3(UnityEngine.Random.Range(-4f, 4f), 0, 0);
        enemy.SetActive(true);
    }
}
