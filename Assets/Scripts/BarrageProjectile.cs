using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrageProjectile : MonoBehaviour, IProjectile
{
    BaseEnemy _target;
    [SerializeField] ParticleSystem _projectileHitEffect;
    [SerializeField] ParticleSystem _projectileTrail;
    [SerializeField] int _trailParticlesCount;
    [field: SerializeField] public bool HasTarget { get; private set; }

    bool _willHitNextUpdate;
    bool _hasHitPoint;
    int BOTH_MASKS;
    int ticksSinceNoTarget;
    int _targetUID;
    float _damage;
    float _speed;
    Vector2 _lastNormalHit;
    Vector3 _lastVelocityDirNormalized;
    Vector3 _lastDistanceTraveled;
    Vector3 _oldPosition;
    Vector3 _lastHitPoint;

    ParticleSystem _borrowedTrail;

    public void PrepareProjectile(BaseEnemy target, float damage, float speed, Vector3 lastDir)
    {
        _target = target;
        _targetUID = target.UID;
        _damage = damage;
        _speed = speed;
        _lastVelocityDirNormalized = lastDir;
        ResetValues();

        _borrowedTrail = DynamicObjectPooler.Instance.BorrowEffect(_projectileTrail);
        _borrowedTrail.transform.position = transform.position;
        var emission = _borrowedTrail.emission;
        emission.rateOverDistance = _trailParticlesCount;
        _borrowedTrail.Play();
    }

    void ResetValues()
    {
        ticksSinceNoTarget = 0;
        _willHitNextUpdate = false;
        _hasHitPoint = false;
        HasTarget = true;
    }

    private void Awake()
    {
        BOTH_MASKS = LayerMask.GetMask("Enemy", "NonEnemyObstacle");
    }

    private void Update()
    {
        _borrowedTrail.transform.position = transform.position;
    }

    private void FixedUpdate()
    {
        if(!HasTarget) ticksSinceNoTarget++;

        MoveProjectile();

        if(ticksSinceNoTarget >= 120) ReturnProjectile();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DynamicObjectPooler.Instance.RequestInstantEffect(_projectileHitEffect, transform.position, Quaternion.identity, 30);
        
        if(collision.TryGetComponent(out BaseEnemy enemy))
        {
            enemy.TakeDamage(_damage);
            ReturnProjectile();
            return;
        }
    }

    void MoveProjectile()
    {
        if (!_target.gameObject.activeSelf || _target.UID != _targetUID || _target == null) HasTarget = false;

        if (_willHitNextUpdate)
        {
            _willHitNextUpdate = false;
            transform.position = _lastHitPoint;
            _hasHitPoint = false;
            return;
        }

        RotateTowardsTarget();

        if (HasTarget)
        {
            _oldPosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, _speed * Time.fixedDeltaTime);
            _lastDistanceTraveled = transform.position - _oldPosition;
            _lastVelocityDirNormalized = _lastDistanceTraveled.normalized;
        }
        else
        {
            transform.position += _speed * Time.fixedDeltaTime * _lastVelocityDirNormalized;
        }

        FutureHitCheck();
    }

    void FutureHitCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _lastVelocityDirNormalized, _lastDistanceTraveled.magnitude, BOTH_MASKS);
        Debug.DrawRay(transform.position, _lastVelocityDirNormalized * _lastDistanceTraveled.magnitude, Color.red);
        if(hit == true)
        {
            _willHitNextUpdate = true;
            _hasHitPoint = true;
            _lastHitPoint = hit.point;
            _lastNormalHit = hit.normal;
        }
    }

    private void RotateTowardsTarget()
    {
        Vector2 direction = _oldPosition - transform.position;
        transform.up = -direction.normalized;
    }

    void ReturnProjectile()
    {
        _borrowedTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        
        DynamicObjectPooler.Instance.ReturnBorrowedEffect(_borrowedTrail);
        
        DynamicObjectPooler.Instance.ReturnProjectile(gameObject);
        gameObject.SetActive(false);
    }

    public Vector2 ReturnRaycastHitNormal() => _lastNormalHit;

    public Vector2 ReturnRaycastHitPoint() => _lastHitPoint;

    public float ReturnProjectileDamage() => _damage;

    public Vector2 ReturnLastProjectileDirection() => _lastVelocityDirNormalized;

    public void NonEnemyHitReturnProjectile()
    {
        DynamicObjectPooler.Instance.RequestInstantEffect(_projectileHitEffect, transform.position, Quaternion.identity, 30);

        _borrowedTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        DynamicObjectPooler.Instance.ReturnBorrowedEffect(_borrowedTrail);

        gameObject.SetActive(false);
        DynamicObjectPooler.Instance.ReturnProjectile(gameObject);
    }

    public bool HasHitPointValue() => _hasHitPoint;
}
