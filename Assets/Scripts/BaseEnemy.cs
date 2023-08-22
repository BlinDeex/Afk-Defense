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

    [field: SerializeField] public float MaxHealth { get; protected set; }
    [field: SerializeField] public float CurrentHealth { get; protected set; }

    [SerializeField] Vector3[] _currencyGainOffsets;
    [SerializeField] GameObject _coinGainEffect;
    [SerializeField] Transform _mostBottomPoint;

    [SerializeField] SpriteRenderer _spriteRenderer;

    public bool CurrentlyDead { get; protected set; }

    public bool _freezeMovement;
    float _damage;
    bool _dead = false;

    [Header("Temporary Visible")]
    [SerializeField] float _damageTakenMultiplier = 1f;

    [field: SerializeField] public int UID { get; set; }

    int _coinBounty = 50;

    class DoTEffect
    {
        public bool Active;
        public int TicksLeft;
        public float Stat;
        /// <summary>
        /// Applies stat only once then while effect is running stat is overwritten if new stat is provided
        /// </summary>
        public bool NotContinuousApplier { get; private set; }
        public Action Implementation { get; private set; }
        public Action<float> OnDemandApplier { get; private set; }

        public DoTEffect(Action implementation, bool notContinuousApplier = false, Action<float> onDemandApplier = null)
        {
            Implementation = implementation;
            NotContinuousApplier = notContinuousApplier;
            OnDemandApplier = onDemandApplier;
        }
    }

    readonly Dictionary<EnemyEffect, DoTEffect> _effectsOverTimeDict = new();
    readonly Dictionary<EnemyEffect, ParticleSystem> _activeEffectParticleSystems = new();

    /// <summary>
    /// must run first in Awake method
    /// </summary>
    protected void BaseAwake()
    {
        _effectsOverTimeDict.Add(EnemyEffect.Burning, new DoTEffect(Burning));
        _effectsOverTimeDict.Add(EnemyEffect.Alienated, new DoTEffect(Alienated, true, AlienatedOnDemand));
        _damageTakenMultiplier = 1f;
    }

    /// <summary>
    /// Projectile applies effect on hit through here
    /// </summary>
    /// <param name="effect">enum of all existing effect types</param>
    /// <param name="stat">abstract stat added used for particular purpose in effect method</param>
    /// <param name="lengthInTicks"></param>
    public void ApplyEffect(EnemyEffect effect, float stat, int lengthInTicks, ParticleSystem effectPS, int emissionRate)
    {
        if (_dead) return;

        DoTEffect requiredEffect = _effectsOverTimeDict[effect];

        if (requiredEffect.NotContinuousApplier) requiredEffect.OnDemandApplier.Invoke(stat);

        if (!requiredEffect.Active)
        {
            ParticleSystem effectVisuals = DynamicObjectPooler.Instance.BorrowEffect(effectPS);
            var emission = effectVisuals.emission;
            emission.rateOverTime = emissionRate;
            var shape = effectVisuals.shape;
            shape.spriteRenderer = _spriteRenderer;
            effectVisuals.Play();
            _activeEffectParticleSystems.Add(effect, effectVisuals);
        }

        requiredEffect.Active = true;
        requiredEffect.TicksLeft = lengthInTicks;
        requiredEffect.Stat = stat;
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
        DoTEffect effect = _effectsOverTimeDict[EnemyEffect.Burning];
        effect.TicksLeft--;

        CurrentHealth -= effect.Stat;

        if (effect.TicksLeft <= 0) effect.Active = false;
    }

    private void Alienated()
    {
        DoTEffect effect = _effectsOverTimeDict[EnemyEffect.Alienated];
        effect.TicksLeft--;

        if (effect.TicksLeft <= 0)
        {
            effect.Active = false;
            _damageTakenMultiplier -= effect.Stat;
            ParticleSystem effectPS = _activeEffectParticleSystems[EnemyEffect.Alienated];
            effectPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _activeEffectParticleSystems.Remove(EnemyEffect.Alienated);
        }
    }

    private void AlienatedOnDemand(float newStat)
    {
        DoTEffect effect = _effectsOverTimeDict[EnemyEffect.Alienated];

        if (effect.Active && effect.Stat != newStat)
        {
            _damageTakenMultiplier += newStat - effect.Stat;
            return;
        }

        if(!effect.Active) _damageTakenMultiplier += newStat;
    }

    virtual public void BaseFixedUpdate()
    {
        percentMovingPowerGainedThisTick = 0;
        RunEffects();
        Movement();
        UpdateParticleSystemsPositions();
    }

    private void UpdateParticleSystemsPositions()
    {
        foreach(ParticleSystem PS in _activeEffectParticleSystems.Values)
        {
            PS.transform.position = transform.position;
        }
    }

    private void ResetAllEffects()
    {
        foreach(ParticleSystem PS in _activeEffectParticleSystems.Values)
        {
            PS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            DynamicObjectPooler.Instance.ReturnBorrowedEffect(PS);
        }

        _activeEffectParticleSystems.Clear();
        foreach(DoTEffect effect in _effectsOverTimeDict.Values)
        {
            effect.Active = false;
        }
    }

    float DistancePointToLine(Vector2 p, Vector2 a, Vector2 b)
    {
        return Mathf.Abs((b.x - a.x) * (a.y - p.y) - (a.x - p.x) * (b.y - a.y)) / (b - a).sqrMagnitude; // use Magnitude for "real" distance
    }

    void Movement()
    {
        CurrentDistanceToFinish = DistancePointToLine(_mostBottomPoint.position, _finishLineStart, _finishLineEnd);

        if (_freezeMovement) return;

        _modifiedMovingPower = defaultMovingPower;
        _modifiedMovingPower *= Math.Clamp(1 + (percentMovingPowerGainedThisTick / 100), 0, 10);
        

        _rb.velocity += _modifiedMovingPower * Time.fixedDeltaTime * Vector2.down;
        _currentSpeed = _rb.velocity.sqrMagnitude;
    }

    // I need _dead bool to check if last projectile already killed enemy, otherwise this somehow fires
    // twice when TakeDamage is ran very rapidly TODO: IT STILL RUNS TWICE SOMETIMES
    public void TakeDamage(float damage, int type = -1) // Projectile on hit runs this method to deal contact damage
    {
        CurrentHealth -= damage * _damageTakenMultiplier;
        UpdateHealthbar();
        if (CurrentHealth <= 0 && !_dead)
        {
            Killed();
            _dead = true;
        }
    }

    public virtual void PrepareEnemy(float health, float movingPower, int uid, float damage, int bounty)
    {
        UID = uid;
        _dead = false;
        _damage = damage;
        _coinBounty = bounty;
        CurrentHealth = MaxHealth = health;
        defaultMovingPower = movingPower;
        _healthbarScalar.transform.localScale = new Vector3(1, 1, 1);
        _healthBar.SetActive(false);
        CurrentDistanceToFinish = 999;
        CurrentlyDead = false;
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
        ResetAllEffects();
        TargetProvider.Instance.RemoveActiveEnemy(this);
        CurrentlyDead = true;
        if (_deathEffectActive)
        DynamicObjectPooler.Instance.RequestInstantEffect(_deathEffect, transform.position, Quaternion.identity, _deathEffectParticlesCount);
        gameObject.SetActive(false);
        DynamicObjectPooler.Instance.ReturnEnemy(gameObject);
    }

    void UpdateHealthbar()
    {
        float healthPercent = CurrentHealth / MaxHealth;
        _healthbarScalar.transform.localScale = new Vector3(healthPercent, 1, 1);
        _healthBar.SetActive(true);
    }
}
