using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;

    [SerializeField] float defaultMovingPower = 10f;
    [SerializeField] float modifiedMovingPower;
    float percentMovingPowerGainedThisTick = 0;
    [SerializeField] float _modifiedMovingPower = 1f;
    [SerializeField] float _currentSpeed;
    public float CurrentDistanceToFinish;

    [SerializeField] Vector3 _finishLineStart;
    [SerializeField] Vector3 _finishLineEnd;

    [SerializeField] GameObject _healthBar;
    [SerializeField] GameObject _healthbarScalar;

    [SerializeField] ParticleSystem _deathEffect;
    [SerializeField] bool _deathEffectActive = true;
    [SerializeField] int _deathEffectParticlesCount;

    [SerializeField] float _maxHealth;
    [SerializeField] float _currentHealth;

    [SerializeField] Vector3[] _currencyGainOffsets;
    [SerializeField] GameObject _coinGainEffect;

    public bool _freezeMovement;
    float _damage;
    bool _dead = false;

    [field: SerializeField] public int UID { get; set; }

    int _coinBounty = 50;

    class DoTEffect
    {
        public bool Active;
        public int TicksLeft;
        public float stat;
        public Action Implementation { get; private set; }

        public DoTEffect(Action implementation)
        {
            Implementation = implementation;
        }
    }

    readonly Dictionary<Effect, DoTEffect> _effectsOverTimeDict = new();

    private void Awake()
    {
        _effectsOverTimeDict.Add(0, new DoTEffect(Burning));
    }

    public void ApplyEffect(Effect effect, float stat, int lengthInTicks)
    {
        DoTEffect requiredEffect = _effectsOverTimeDict[effect];

        requiredEffect.Active = true;
        requiredEffect.TicksLeft = lengthInTicks;
        requiredEffect.stat = stat;
    }

    void RunEffects()
    {
        foreach(DoTEffect effect in _effectsOverTimeDict.Values)
        {
            if (effect.Active) effect.Implementation.Invoke();
        }
    }

    void Burning()
    {
        DoTEffect effect = _effectsOverTimeDict[Effect.Burning];
        effect.TicksLeft--;

        _currentHealth -= effect.stat;

        if (effect.TicksLeft <= 0) effect.Active = false;
    }

    virtual public void BaseFixedUpdate()
    {
        percentMovingPowerGainedThisTick = 0;
        RunEffects();
        Movement();
    }

    float DistancePointToLine(Vector2 p, Vector2 a, Vector2 b)
    {
        return Mathf.Abs((b.x - a.x) * (a.y - p.y) - (a.x - p.x) * (b.y - a.y)) / (b - a).sqrMagnitude; // use Magnitude for "real" distance
    }

    void Movement()
    {
        if (_freezeMovement) return;

        _modifiedMovingPower = defaultMovingPower;
        _modifiedMovingPower *= Math.Clamp(1 + (percentMovingPowerGainedThisTick / 100), 0, 10);
        CurrentDistanceToFinish = DistancePointToLine(transform.position, _finishLineStart, _finishLineEnd);

        _rb.velocity += _modifiedMovingPower * Time.fixedDeltaTime * Vector2.down;
        _currentSpeed = _rb.velocity.sqrMagnitude;
    }

    // I need _dead bool to check if last projectile already killed enemy, otherwise this somehow fires
    // twice when TakeDamage is ran very rapidly TODO: IT STILL RUNS TWICE SOMETIMES
    public void TakeDamage(float damage, int type = -1)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0 && !_dead)
        {
            Killed();
            _dead = true;
        }
        UpdateHealthbar();
    }

    public virtual void PrepareEnemy(float health, float movingPower, int uid, float damage, int bounty)
    {
        UID = uid;
        _dead = false;
        _damage = damage;
        _coinBounty = bounty;
        _currentHealth = _maxHealth = health;
        defaultMovingPower = movingPower;
        _healthbarScalar.transform.localScale = new Vector3(1, 1, 1);
        _healthBar.SetActive(false);
        CurrentDistanceToFinish = 999;
    }

    void Killed()
    {
        GameObject coinGainEffect = 
            DynamicObjectPooler.Instance.RequestCurrencyEffect(_coinGainEffect);

        coinGainEffect.transform.position = transform.position + _currencyGainOffsets[UnityEngine.Random.Range(0, _currencyGainOffsets.Length)];
        coinGainEffect.GetComponent<CoinGainEffect>().PrepareCurrencyEffect(_coinBounty);
        coinGainEffect.SetActive(true);
        
        CurrencyManager.Instance.AddGold(_coinBounty);
        ReturnEnemy();
    }

    public void FinishLineReached()
    {
        HealthManager.Instance.TakeDamage(_damage);
        ReturnEnemy();
    }

    public virtual void ReturnEnemy()
    {
        TargetProvider.Instance.RemoveActiveEnemy(this);
        if (_deathEffectActive)
        DynamicObjectPooler.Instance.RequestInstantEffect(_deathEffect, transform.position, Quaternion.identity, _deathEffectParticlesCount);
        gameObject.SetActive(false);
        DynamicObjectPooler.Instance.ReturnEnemy(gameObject);
    }

    void UpdateHealthbar()
    {
        float healthPercent = _currentHealth / _maxHealth;
        _healthbarScalar.transform.localScale = new Vector3(healthPercent, 1, 1);
        _healthBar.SetActive(true);
    }
}
