using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienProjectile : BaseProjectile
{
    bool _spawnedThisFrame;
    float _percentDamageIncrease;
    int _effectLengthInTicks;

    [SerializeField] ParticleSystem _alienatedEffectPS;
    int _emissionRate;
    public override void NonEnemyHitReturnProjectile()
    {
        ReturnProjectile();
    }

    public void PrepareProjectile(float damage, Vector3 lastDir,
        float percentDamageIncrease, int effectLengthInTicks, int effectEmissionRate)
    {
        _projectileDamage = damage;
        _lastVelocityDirNormalized = lastDir;
        _spawnedThisFrame = true;
        _percentDamageIncrease = percentDamageIncrease;
        _effectLengthInTicks = effectLengthInTicks;
        _emissionRate = effectEmissionRate;
    }

    private void FixedUpdate()
    {
        if (_spawnedThisFrame)
        {
            _spawnedThisFrame = false;
        }
        else
        {
            ReturnProjectile();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out BaseEnemy enemy))
        {
            enemy.ApplyEffect(EnemyEffect.Alienated, _percentDamageIncrease,
                _effectLengthInTicks, _alienatedEffectPS, _emissionRate);

            enemy.TakeDamage(_projectileDamage);
            ReturnProjectile();
            return;
        }
    }

    private void Awake()
    {
        BaseAwake();
    }

    void ReturnProjectile()
    {
        DynamicObjectPooler.Instance.ReturnProjectile(gameObject);
        gameObject.SetActive(false);
    }
}
