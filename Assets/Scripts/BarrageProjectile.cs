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
    bool _firstFrame = true;
    int _bothMasks = 4160;
    int ticksSinceNoTarget;
    int _targetUID;
    float _damage;
    float _speed;
    Vector3 _lastVelocityDir;
    Vector3 _lastDistanceTraveled;
    Vector3 _oldPosition;
    Vector3 _lastHitPoint;
    Vector2 _lastNormalHit;

    ParticleSystem _borrowedTrail;

    public void PrepareProjectile(BaseEnemy target, float damage, float speed, Vector3 lastDir)
    {
        _target = target;
        _damage = damage;
        _speed = speed;
        _firstFrame = true;
        _lastVelocityDir = lastDir;
        _willHitNextUpdate = false;

        ticksSinceNoTarget = 0;
        _targetUID = target.UID;
        HasTarget = true;


        _borrowedTrail = DynamicObjectPooler.Instance.BorrowEffect(_projectileTrail);

        _borrowedTrail.transform.position = transform.position;
        var emission = _borrowedTrail.emission;
        emission.rateOverDistance = _trailParticlesCount;
        _borrowedTrail.Play();
    }

    private void Update()
    {
       Trail();
    }

    private void FixedUpdate()
    {
        if (!HasTarget) ticksSinceNoTarget++;

        MoveProjectile();

        if(ticksSinceNoTarget >= 120) ReturnProjectile();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DynamicObjectPooler.Instance.RequestInstantEffect(_projectileHitEffect, transform.position, Quaternion.identity, 30);
        
        if(collision.TryGetComponent(out BaseEnemy enemy)) enemy.TakeDamage(_damage);
        
        ReturnProjectile();
    }

    void MoveProjectile()
    {
        if (!_target.gameObject.activeSelf || _target.UID != _targetUID || _target == null)
        {
            if (_firstFrame) ReturnProjectile();
            HasTarget = false;
        }
        if (_willHitNextUpdate) // teleport to raycast hit so physics can catch collision
        {
            transform.position = _lastHitPoint;
            if (!Physics2D.OverlapPoint(transform.position)) // this might be related cause without it some projectiles will be left frozen behind
            {
                ReturnProjectile();
            }

            return;
        }

        RotateTowardsTarget();

        if (HasTarget)
        {

            _oldPosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, _speed * Time.fixedDeltaTime);
            _lastDistanceTraveled = transform.position - _oldPosition;
            _lastVelocityDir = _lastDistanceTraveled.normalized;
        }
        else
        {
            transform.position += _lastVelocityDir * _speed * Time.fixedDeltaTime;
        }

        FutureHitCheck(); // get majority of values used in GuardingCircleShield class

        _firstFrame = false;
    }

    void FutureHitCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _lastVelocityDir, _lastDistanceTraveled.magnitude, _bothMasks);
        if(hit == true)
        {
            _willHitNextUpdate = true;
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

    void Trail()
    {
        _borrowedTrail.transform.position = transform.position;
    }

    public Vector2 ReturnRaycastHitNormal() => _lastNormalHit;

    public Vector2 ReturnRaycastHitPoint() => _lastHitPoint;

    public float ReturnProjectileDamage() => _damage;

    public Vector2 ReturnLastProjectileDirection() => _lastVelocityDir;

    public void NonEnemyHitReturnProjectile()
    {
        DynamicObjectPooler.Instance.RequestInstantEffect(_projectileHitEffect, transform.position, Quaternion.identity, 30);

        _borrowedTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        DynamicObjectPooler.Instance.ReturnBorrowedEffect(_borrowedTrail);

        gameObject.SetActive(false);
        DynamicObjectPooler.Instance.ReturnProjectile(gameObject);
    }
}
